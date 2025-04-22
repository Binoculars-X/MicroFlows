using BlazorForms.Flows.Definitions;
using BlazorForms.Flows.Engine.Fluent;
using BlazorForms.Forms;
using BlazorForms.Shared.Extensions;
using MicroFlows.Application.Helpers;
using System.Text.Json;
using System.Threading;

namespace MicroFlows.AdminUI.Forms;

public class FormFlowList : FormListBase<FlowListModel>
{
	protected override void Define(FormListBuilder<FlowListModel> builder)
    {
        builder.List(p => p.Data, e =>
        {
            //e.DisplayName = "Profiles";

            e.Property(p => p.RefId).IsPrimaryKey().Label("RefId");
            e.Property(p => p.ExternalId);
            e.Property(p => p.CorrelationId);
            //e.Property(p => p.ProfileId);
            e.Property(p => p.Status).Label("Status");
            e.Property(p => p.ShortName).Label("Flow").MaxLength(32);
            e.Property(p => p.Tag).Label("Tag").MaxLength(32);
            e.Property(p => p.CreatedOn).Label("Created").Format(GlobalSettings.DateTimeFormat);
            e.Property(p => p.UpdatedOn).Label("Updated").Format(GlobalSettings.DateTimeFormat);

            //e.ContextButton("Details", typeof(FormHistoryDetailsDialogFlow),
            //    BlazorForms.Shared.FlowReferenceOperation.DialogForm);

        });
    }
}
public class FlowInstanceListFlow : ListFlowBase<FlowListModel, FormFlowList>
{
    public const string SEARCH = "SEARCH";
    private readonly IFlowRepository _flowRepository;
    private readonly LocalSettings _localSettings;

	public FlowInstanceListFlow(IFlowRepository flowRepository, LocalSettings localSettings)
	{
        _flowRepository = flowRepository;
        _localSettings = localSettings;
	}

	public override async Task<FlowListModel> LoadDataAsync(QueryOptions queryOptions)
    {
        if (Params.DynamicInput.ContainsKey(SEARCH))
        {
            var query = JsonSerializer.Deserialize<FlowExtendedSearchQuery>(Params[SEARCH]);
            SetSorting(query, queryOptions);

            var data = await _flowRepository.ExtendedSearch(query);

            return new FlowListModel
            {
                Data = data.Lines.Select(l => new FlowDetailsModel(l)
                {
                    ShortName = l.Name.Split('.').Last(),
                    CreatedOn = TimeZoneHelper.ConvertToTimeZone(l.Created, _localSettings.TimeZone)?.DateTime,
                    UpdatedOn = TimeZoneHelper.ConvertToTimeZone(l.Modified, _localSettings.TimeZone)?.DateTime,
                }).ToList(),
                Count = data.Count,
            };
        }
        else
        {
            return new FlowListModel();
        }
    }

    private void SetSorting(FlowExtendedSearchQuery? query, QueryOptions queryOptions)
    {
        if (!string.IsNullOrEmpty(queryOptions.SortColumn))
        {
            query.Sort = queryOptions.SortColumn switch 
            { 
                "ShortName" => FlowSearchSortOrder.Name,
                "CreatedOn" => FlowSearchSortOrder.Created,
                "UpdatedOn" => FlowSearchSortOrder.Modified,
                _ => Enum.Parse<FlowSearchSortOrder>(queryOptions.SortColumn)
            };

            query.Ascending = queryOptions.SortDirection switch
            {
                BlazorForms.Shared.SortDirectionType.Asc => true,
                BlazorForms.Shared.SortDirectionType.Desc => false,
                _ => null
            };
        }
    }

    public const int DIFF_LENGTH = 52;

    private string? ShortText(string text)
    {
        return (text?.Length ?? 0) > DIFF_LENGTH ? text.Substring(0, DIFF_LENGTH) : text;
    }
}

public class FlowListModel : IFlowModel
{
    public int? Count { get; set; }
    public virtual List<FlowDetailsModel>? Data { get; set; } = [];
}

public record FlowDetailsModel : FlowExtendedSearchResultLine
{
    public FlowDetailsModel(FlowExtendedSearchResultLine original) : base(original)
    {
    }

    public string? ShortName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
