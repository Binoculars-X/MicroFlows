using JsonPathToModel;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using MicroFlows.Application.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroFlows.MsSqlRepo;

public class MsSqlFlowRepository : IFlowRepository
{
    public const string CONNECTION_STRING_KEY = "MsSqlFlowRepositoryConnectionString";
    public const string TABLE_NAME = "flow_run";

    private readonly string _connectionString;
    private readonly string _tableName;

    public MsSqlFlowRepository(IConfiguration configuration, MsSqlFlowRepositorySettings? settings = null)
    {
        _tableName = settings?.TableName ?? TABLE_NAME;

        if (string.IsNullOrEmpty(_tableName))
        {
            throw new ArgumentOutOfRangeException(nameof(_tableName));
        }

        if (!string.IsNullOrEmpty(settings?.ConnectionString))
        {
            _connectionString = settings.ConnectionString;
        }
        else
        {
            var key = settings?.ConnectionStringKey ?? CONNECTION_STRING_KEY;
            _connectionString = configuration.GetConnectionString(key);
        }

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new ArgumentOutOfRangeException(nameof(_connectionString));
        }

        CheckTablesExist();
    }

    private void CheckTablesExist()
    {
        var q = @$"
if not exists (select * from sysobjects where name='{_tableName}' and xtype='U')
    create table {_tableName} (
        id uniqueidentifier not null,
        flow_json varchar(max) not null,
        CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED 
        (
	        [id] ASC
        )
    )";

        using (SqlConnection connection = new SqlConnection(
                       _connectionString))
        {
            SqlCommand command = new SqlCommand(q, connection);
            command.Connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public async Task<FlowContext> CreateFlowContext(IFlow flow, FlowParams flowParams)
    {
        var ctx = new FlowContext();
        ctx.Model.ImportFrom(flow, new ImportOptions { ExcludeStartsWith = "__" });
        ctx.Params = flowParams;
        ctx.RefId = Guid.NewGuid().ToString();
        ctx.ExecutionResult.FlowState = Domain.Enums.FlowStateEnum.Start;

        var flowModel = new FlowStoreModel()
        {
            RefId = ctx.RefId,
            ExternalId = flowParams.ExternalId,
            FlowTypeName = flow.GetType().FullName!,
            ContextHistory = [ctx],
            SignalJournal = flow.SignalJournal!,
        };

        var json = JsonSerializer.Serialize(flowModel);

        var q = $@"
INSERT INTO {_tableName}
SELECT @p1, @p2;
";

        using (SqlConnection connection = new SqlConnection(
                       _connectionString))
        {
            SqlCommand cmd = new SqlCommand(q, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", ctx.RefId);
            cmd.Parameters.AddWithValue("p2", json);
            await connection.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();
        }

        //_flowModelDictionary[ctx.RefId] = flowModel;

        return ctx;
    }

    public async Task UpdateFlowModel(FlowStoreModel flowModel)
    {
        var json = JsonSerializer.Serialize(flowModel);

        var q = $@"
UPDATE {_tableName}
SET flow_json = @p1
WHERE id = @p2;
";

        using (SqlConnection connection = new SqlConnection(
                       _connectionString))
        {
            SqlCommand cmd = new SqlCommand(q, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", json);
            cmd.Parameters.AddWithValue("p2", flowModel.RefId);
            await connection.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task SaveContextHistory(List<FlowContext> contextHistory)
    {
        var id = contextHistory.First().RefId;
        var model = await GetFlowModel(id);
        model.ContextHistory = contextHistory;
        await UpdateFlowModel(model);
    }

    public async Task<FlowStoreModel> UpdateFlow(IFlow flow)
    {
        // merge Flow SignalJournal
        // ToDo: impplement other Update scenarious
        var model = await GetFlowModel(flow.RefId);
        model.SignalJournal.AddRange(flow.SignalJournal);
        await UpdateFlowModel(model);
        var clone = TypeHelper.CloneObject(model);
        return clone;
        //_flowModelDictionary[flow.RefId].SignalJournal.AddRange(flow.SignalJournal);
        //var model = _flowModelDictionary[flow.RefId];
        //var clone = TypeHelper.CloneObject(model);
        //return Task.FromResult(clone);
    }

    public async Task<FlowStoreModel> GetFlowModel(string refId)
    {
        // _flowModelDictionary[refId]

        var q = $"select id, flow_json from {_tableName} where id = @p1";

        using (SqlConnection connection = new SqlConnection(
                       _connectionString))
        {
            SqlCommand cmd = new SqlCommand(q, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", refId);
            await connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var json = reader.GetString(1);
                var model = JsonSerializer.Deserialize<FlowStoreModel>(json);
                return model!;
            }
                
            throw new FlowNotFoundException($"Cannot find flow for refId '{refId}'");
        }
    }

    public async Task<List<FlowStoreModel>> SearchFlowModel(FlowSearchQuery query)
    {
        var list = new List<FlowStoreModel>();
        var q = $"select id, flow_json from {_tableName}";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand cmd = new SqlCommand(q, connection);
            cmd.CommandType = System.Data.CommandType.Text;

            if (query.IsNotEmpty())
            {
                q += " where 1=1";
            }

            if (!string.IsNullOrEmpty(query.ExternalId))
            {
                q += " and JSON_VALUE(flow_json, '$.externalId') = @p1";
                cmd.Parameters.AddWithValue("p1", query.ExternalId);
            }

            if (!string.IsNullOrEmpty(query.RefId))
            {
                q += " and id = @p2";
                cmd.Parameters.AddWithValue("p2", query.RefId);
            }

            await connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var json = reader.GetString(1);
                var model = JsonSerializer.Deserialize<FlowStoreModel>(json);
                list.Add(model!);
            }
        }
            
        return list;
    }

    public async Task<List<FlowContext>> GetFlowHistory(string refId)
    {
        var model = await GetFlowModel(refId);
        var history = model.ContextHistory;
        var clone = TypeHelper.CloneObject(history);
        return clone;
    }

    public async Task<List<FlowContext>?> FindFlowHistory(FlowSearchQuery query)
    {
        var list = await SearchFlowModel(query);
        return list.FirstOrDefault()?.ContextHistory;

        //if (query.RefId != null)
        //{
        //    return await GetFlowHistory(query.RefId);
        //}

        //// ToDo: implement search flow in DB table
        ////if (query.ExternalId != null)
        ////{
        ////    var record = _flowModelDictionary.Values.FirstOrDefault(f => f.ExternalId == query.ExternalId);
        ////    return record?.ContextHistory;
        ////}

        //return null;
    }
}
