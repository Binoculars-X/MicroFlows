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
using MicroFlows.Domain.Enums;

namespace MicroFlows.MsSqlRepo;

public partial class MsSqlFlowRepository : IFlowRepository
{
    public const string CONNECTION_STRING_KEY = "MsSqlFlowRepositoryConnectionString";
    public const string TABLE_NAME = "flow_run";

    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly MsSqlFlowRepositorySettings _settings;

    public MsSqlFlowRepository(IConfiguration configuration, MsSqlFlowRepositorySettings? settings = null)
    {
        _settings = settings ?? new MsSqlFlowRepositorySettings();
        _tableName = GetTableNameOnly();

        if (_settings?.DatabaseName != null)
        {
            _tableName = $"{_settings.DatabaseName}..{_tableName}";
        }

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

        CheckDbTablesExist();
    }

    private string GetTableNameOnly()
    {
        return _settings?.TableName ?? TABLE_NAME;
    }

    private void CheckDbTablesExist()
    {
        if (_settings.CreateDatabase == true)
        {
            var dbQuery = @$"
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{_settings.DatabaseName}')
BEGIN
    CREATE DATABASE [{_settings.DatabaseName}]
END
";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(dbQuery, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }

        var q = @$"
use [{_settings?.DatabaseName ?? "master"}];
if not exists (select * from sysobjects where name='{GetTableNameOnly()}' and xtype='U')
begin
    create table {_tableName} (
        id uniqueidentifier not null,
        external_id varchar(64) null,
        correlation_id varchar(64) null,
        exec_status tinyint not null,
        flow_json varchar(max) not null,
        flow_name varchar(256) not null,
        tag varchar(256) null,
        created_on datetimeoffset(7) null,
        modified_on datetimeoffset(7) null,
        time_lock datetimeoffset(7) null,
        ver timestamp not null,
        CONSTRAINT [PK_{GetTableNameOnly()}] PRIMARY KEY CLUSTERED 
        (
	        [id] ASC
        )
    );

    CREATE NONCLUSTERED INDEX [{GetTableNameOnly()}_external_id] ON {_tableName}
    (
	    [external_id] ASC
    );

    CREATE NONCLUSTERED INDEX [{GetTableNameOnly()}_exec_status] ON {_tableName}
    (
	    [exec_status] ASC
    );

    CREATE NONCLUSTERED INDEX [{GetTableNameOnly()}_time_lock] ON {_tableName}
    (
	    [time_lock] ASC
    );
end
";

        using (SqlConnection connection = new SqlConnection(_connectionString))
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
        ctx.ExecutionResult.ResultState = Domain.Enums.ResultStateEnum.Success;

        var flowModel = new FlowStoreModel()
        {
            RefId = ctx.RefId,
            ExternalId = flowParams.ExternalId,
            FlowTypeName = flow.GetType().FullName!,
            ContextHistory = [ctx],
            SignalJournal = flow.SignalJournal!,
        };

        RefreshFlowStoreModelRoot(flowModel);

        if (!flowParams.FlowOptions.NoStorage)
        {
            var json = JsonSerializer.Serialize(flowModel);
            var rec = GetFlowRecord(flowModel);

            var q = $@"
INSERT INTO {_tableName}(id, flow_json, external_id, exec_status, created_on, flow_name)
SELECT @p1, @p2, @p3, @p4, @p5, @p6;
";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(q, connection);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("p1", ctx.RefId);
                cmd.Parameters.AddWithValue("p2", json);
                cmd.Parameters.AddWithValue("p3", ((object)rec.ExternalId) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p4", rec.Status);
                cmd.Parameters.AddWithValue("p5", DateTimeOffset.UtcNow);
                cmd.Parameters.AddWithValue("p6", rec.FlowName);
                await connection.OpenAsync();
                var result = await cmd.ExecuteNonQueryAsync();
            }
        }

        return ctx;
    }

    public record FlowRecord(string RefId, string? ExternalId, string FlowName, FlowStateEnum? Status);

    private FlowRecord GetFlowRecord(FlowStoreModel m)
    {
        var status = m.ContextHistory.Last().ExecutionResult.FlowState;
        return new FlowRecord(m.RefId, m.ExternalId, m.FlowTypeName, status);
    }

    /// <summary>
    /// Populates root fields for faster search
    /// !!! Data Dublication !!!
    /// </summary>
    /// <param name="flowModel"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void RefreshFlowStoreModelRoot(FlowStoreModel flowModel)
    {
        var ctx = flowModel.ContextHistory.Last();

        if (ctx != null)
        {
            flowModel.Result = ctx.ExecutionResult.ResultState;
            flowModel.State = ctx.ExecutionResult.FlowState;
            flowModel.ExceptionMessage = ctx.ExecutionResult.ExceptionMessage;
            flowModel.Tag = ctx.Params.Tag;
            flowModel.ExternalId = ctx.Params.ExternalId;
        }
    }

    public async Task UpdateFlowModel(FlowStoreModel flowModel)
    {
        RefreshFlowStoreModelRoot(flowModel);
        var json = JsonSerializer.Serialize(flowModel);
        var rec = GetFlowRecord(flowModel);

        var q = $@"
UPDATE {_tableName}
SET flow_json = @p1, external_id = @p3, exec_status = @p4, modified_on = @p5
WHERE id = @p2 AND ver = @p6;
";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand cmd = new SqlCommand(q, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", json);
            cmd.Parameters.AddWithValue("p2", flowModel.RefId);
            cmd.Parameters.AddWithValue("p3", ((object)rec.ExternalId) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p4", rec.Status);
            cmd.Parameters.AddWithValue("p5", DateTimeOffset.UtcNow);
            cmd.Parameters.AddWithValue("p6", flowModel.Timestamp);
            await connection.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();

            if (result != 1)
            {
                throw new OptimisticLockMsSqlFlowRepositoryException($"Flow {flowModel.FlowTypeName} with RefId {flowModel.RefId} update returned not single row count: {result}");
            }
        }

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand cmd = new SqlCommand($"select ver from {_tableName} where id=@p1", connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", flowModel.RefId);
            await connection.OpenAsync();
            flowModel.Timestamp = (await cmd.ExecuteScalarAsync()) as byte[]; 
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
        return model;
    }

    public async Task<FlowStoreModel?> GetFlowModel(string refId)
    {
        var q = $"select id, flow_json, ver from {_tableName} where id = @p1";

        using (SqlConnection connection = new SqlConnection(_connectionString))
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
                model.Timestamp = reader.GetValue(2) as byte[];
                return model!;
            }

            return null;
            //throw new FlowNotFoundException($"Cannot find flow for refId '{refId}'");
        }
    }

    public async Task<List<SearchFlowDetails>> SearchFlow(FlowSearchQuery query)
    {
        var list = new List<SearchFlowDetails>();
        
        var q = @$"
select id
, JSON_VALUE(flow_json, '$.ExternalId') externalId
, JSON_VALUE(flow_json, '$.Tag') tag
, JSON_VALUE(flow_json, '$.State') state
, JSON_VALUE(flow_json, '$.Result') result
from {_tableName}
";

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
                q += " and JSON_VALUE(flow_json, '$.ExternalId') = @p1";
                cmd.Parameters.AddWithValue("p1", query.ExternalId);
            }

            if (!string.IsNullOrEmpty(query.RefId))
            {
                q += " and id = @p2";
                cmd.Parameters.AddWithValue("p2", query.RefId);
            }

            if (!string.IsNullOrEmpty(query.Tag))
            {
                q += " and JSON_VALUE(flow_json, '$.Tag') = @p3";
                cmd.Parameters.AddWithValue("p3", query.Tag);
            }

            if (query.State != null)
            {
                q += " and JSON_VALUE(flow_json, '$.State') = @p4";
                cmd.Parameters.AddWithValue("p4", query.State);
            }

            if (query.Result != null)
            {
                q += " and JSON_VALUE(flow_json, '$.Result') = @p5";
                cmd.Parameters.AddWithValue("p5", query.Result);
            }

            cmd.CommandText = q;
            await connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var model = new SearchFlowDetails(
                    reader.GetGuid(0).ToString(),
                    GetNullableString(reader, 1),
                    GetNullableString(reader, 2),
                    GetNullableEnum<FlowStateEnum>(reader, 3),
                    GetNullableEnum<ResultStateEnum>(reader, 4));

                list.Add(model!);
            }
        }

        return list;
    }

    private string? GetNullableString(SqlDataReader reader, int i)
    {
        if (reader.IsDBNull(i))
        {
            return null;
        }

        return reader.GetString(i);
    }

    private T? GetNullableEnum<T>(SqlDataReader reader, int i) where T : Enum
    {
        if (reader.IsDBNull(i))
        {
            return (T?)(object?)null;
        }

        int v = Convert.ToInt32(reader.GetValue(i));
        var result = (T)(object)v;
        return result;
    }

    private DateTimeOffset? GetNullableDateTimeOffset(SqlDataReader reader, int i)
    {
        if (reader.IsDBNull(i))
        {
            return null;
        }

        return reader.GetDateTimeOffset(i);
    }

    public async Task<List<FlowStoreModel>> SearchFlowModel(FlowSearchQuery query)
    {
        var list = new List<FlowStoreModel>();
        var q = $"select id, flow_json, ver from {_tableName}";

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
                q += " and JSON_VALUE(flow_json, '$.ExternalId') = @p1";
                cmd.Parameters.AddWithValue("p1", query.ExternalId);
            }

            if (!string.IsNullOrEmpty(query.RefId))
            {
                q += " and id = @p2";
                cmd.Parameters.AddWithValue("p2", query.RefId);
            }

            cmd.CommandText = q;
            await connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var json = reader.GetString(1);
                var model = JsonSerializer.Deserialize<FlowStoreModel>(json);
                model.Timestamp = reader.GetValue(2) as byte[];
                list.Add(model!);
            }
        }
            
        return list;
    }

    public async Task<List<FlowContext>> GetFlowHistory(string refId)
    {
        var model = await GetFlowModel(refId);
        var history = model.ContextHistory;
        return history;
    }

    public async Task<List<FlowContext>?> FindFlowHistory(FlowSearchQuery query)
    {
        var list = await SearchFlowModel(query);
        return list.FirstOrDefault()?.ContextHistory;
    }

    public async Task<List<FlowInstanceDetails>> GetUnprocessedFlowsWithTimeLock(int batchSize, int timeLock)
    {
        var list = new List<FlowInstanceDetails>();
        var resultList = new List<FlowInstanceDetails>();
        
        var q = $@"
select top {batchSize} id, flow_name, ver 
from {_tableName} 
where exec_status < {(int)FlowStateEnum.Finished} and (time_lock is null or time_lock < @p1)
order by ver, exec_status ";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand cmd = new SqlCommand(q, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", DateTimeOffset.UtcNow);
            await connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new FlowInstanceDetails(
                    reader.GetGuid(0).ToString(), 
                    reader.GetString(1), 
                    reader.GetValue(2) as byte[]));
            }
        }

        foreach (var record in list)
        {
            if (await LockFlow(record, timeLock))
            {
                resultList.Add(record);
            }
        }

        return resultList;
    }

    public async Task<bool> LockFlow(FlowInstanceDetails instance, int timeLock)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            var uq = $"update {_tableName} set time_lock=@p1 where id=@p2 and ver=@p3 and (time_lock is null or time_lock < @p4)";
            SqlCommand cmd = new SqlCommand(uq, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", DateTimeOffset.UtcNow.AddMicroseconds(timeLock));
            cmd.Parameters.AddWithValue("p2", instance.RefId);
            cmd.Parameters.AddWithValue("p3", instance.Version);
            cmd.Parameters.AddWithValue("p4", DateTimeOffset.UtcNow);
            await connection.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();
            return result == 1;
        }
    }

    public async Task<bool> UnlockFlow(FlowInstanceDetails instance)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            var uq = $"update {_tableName} set time_lock=@p1 where id=@p2 and ver=@p3";
            SqlCommand cmd = new SqlCommand(uq, connection);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("p1", DateTimeOffset.UtcNow);
            cmd.Parameters.AddWithValue("p2", instance.RefId);
            cmd.Parameters.AddWithValue("p3", instance.Version);
            await connection.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();
            return result == 1;
        }
    }
}
