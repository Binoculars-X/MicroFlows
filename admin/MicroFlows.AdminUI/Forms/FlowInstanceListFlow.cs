using BlazorForms.Flows.Definitions;
using BlazorForms.Flows.Engine.Fluent;
using BlazorForms.Forms;
using BlazorForms.Shared.Extensions;
using System.Text.Json;
using System.Threading;

namespace MicroFlows.AdminUI.Forms;

public class FormFlowList : FormListBase<FlowListModel>
{
    public const string SEARCH_ID = "SEARCH_ID";

	protected override void Define(FormListBuilder<FlowListModel> builder)
    {
        builder.List(p => p.Data, e =>
        {
            //e.DisplayName = "Profiles";

            e.Property(p => p.Id).IsPrimaryKey().Label("Id").IsHidden();
            e.Property(p => p.ExternalId);
            //e.Property(p => p.ProfileId);
            e.Property(p => p.Source).Label("Source");
            e.Property(p => p.DifferenceShort).Label("Difference").MaxLength(32);
            e.Property(p => p.Operation);
            e.Property(p => p.UserId).Label("By").MaxLength(16);
            e.Property(p => p.Date).Label("Date").Format("dd/MM/yyyy HH:mm");

            //e.ContextButton("Details", typeof(FormHistoryDetailsDialogFlow),
            //    BlazorForms.Shared.FlowReferenceOperation.DialogForm);

        });
    }
}
public class FlowInstanceListFlow : ListFlowBase<FlowListModel, FormFlowList>
{
    public const string SEARCH = "SEARCH";
    private readonly IFlowRepository _flowRepository;

	public FlowInstanceListFlow(IFlowRepository flowRepository)
	{
        _flowRepository = flowRepository;
	}

	public override async Task<FlowListModel> LoadDataAsync(QueryOptions queryOptions)
    {
        if (Params.DynamicInput.ContainsKey(SEARCH))
        {
            var query = JsonSerializer.Deserialize<FlowExtendedSearchQuery>(Params[SEARCH]);

            return new FlowListModel
            {
                Data = [.. (new FlowModel[] { new FlowModel { ExternalId = "123" } })],
                Count = 1,
            };
        }
        else
        {
            return new FlowListModel();
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
    public virtual List<FlowModel>? Data { get; set; } = [];
}

public class FlowModel : IFlowModel
{
    public int Id { get; set; }
    public string? ProfileId { get; set; } = null!;
    public string? ExternalId { get; set; } = null!;
    public string? FromJson { get; set; }
    public string? ToJson { get; set; }
    public string? DifferenceJson { get; set; }
    public string? DifferenceShort { get; set; }
    public string? Source { get; set; } = null!;
    public string? Operation { get; set; } = null!;
    public string? UserId { get; set; } = null!;
    public DateTime Date { get; set; }
}

