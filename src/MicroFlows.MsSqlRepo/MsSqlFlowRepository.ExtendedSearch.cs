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
using FluentResults;

namespace MicroFlows.MsSqlRepo;

public partial class MsSqlFlowRepository
{
    public async Task<FlowExtendedSearchResult> ExtendedSearch(FlowExtendedSearchQuery query)
    {
        var list = new List<FlowExtendedSearchResultLine>();

        var jsonColumn = query.IncludeModel == true ? "\r\n , flow_json" : "";

        var q = @$"
select id as RefId
, external_id
, correlation_id
, exec_status
, exec_task
, flow_name
, tag
, created_on
, modified_on{jsonColumn}
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
                q += " and external_id = @p1";
                cmd.Parameters.AddWithValue("p1", query.ExternalId);
            }

            if (!string.IsNullOrEmpty(query.RefId))
            {
                q += " and id = @p2";
                cmd.Parameters.AddWithValue("p2", query.RefId);
            }

            if (!string.IsNullOrEmpty(query.Tag))
            {
                q += " and tag = @p3";
                cmd.Parameters.AddWithValue("p3", query.Tag);
            }

            if (!string.IsNullOrEmpty(query.CorrelationId))
            {
                q += " and correlation_id = @p4";
                cmd.Parameters.AddWithValue("p4", query.CorrelationId);
            }

            if (!string.IsNullOrEmpty(query.Name))
            {
                q += " and flow_name like @p5";
                cmd.Parameters.AddWithValue("p5", '%' + query.Name + '%');
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                var statuses = '\'' + query.Status.Replace(" ", "\',\'") + '\'';
                q += $" and exec_status in ({statuses})";
            }

            cmd.CommandText = q;
            await connection.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                FlowStoreModel model = query.IncludeModel == true
                    ? JsonSerializer.Deserialize<FlowStoreModel>(GetNullableString(reader, 9))
                    : null;

                var record = new FlowExtendedSearchResultLine(
                    reader.GetGuid(0).ToString(),
                    GetNullableString(reader, 1),
                    GetNullableString(reader, 2),
                    ParseNullableEnum<FlowStateEnum>(reader, 3),
                    GetNullableString(reader, 4),
                    GetNullableString(reader, 5),
                    GetNullableString(reader, 6),
                    GetNullableDateTimeOffset(reader, 7),
                    GetNullableDateTimeOffset(reader, 8),
                    model
                    );

                list.Add(record);
            }
        }

        // ToDo: populate count
        var result = new FlowExtendedSearchResult(list, 0);
        return result;
    }
}
