using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.OrderProcessing;

public class InvoiceFlow : FlowBase<InvoiceModel>
{
    // dependencies
    //private readonly IFlowsProvider _flowsProvider;
    private readonly IInvoiceRepository _repository;

    // signals
    public static string InvoiceCreatedSignal = "InvoiceCreatedSignal";

    // model
    public bool IsSuccessful;
    public DateTime? InvoiceDate;
    public string? ErrorMessage;

    public InvoiceFlow(/*IFlowsProvider flowsProvider, */IInvoiceRepository repository)
    {
        //_flowsProvider = flowsProvider;
        _repository = repository;
    }

    public async Task Flow()
    {
        await CallAsync(Init);
        await CallAsync(SaveInvoice);

        if (IsSuccessful)
        {
            InvoiceDate = DateTime.UtcNow;
        }
        else
        {
            ErrorMessage = $"Failed to create invoice {Model.Id}";
        }

        await CallAsync(NotifyOrder);
    }

    private async Task NotifyOrder()
    {
        var ps = new FlowParams();
        // CorrelationId contains initial ExternalID, generated outside
        ps.ExternalId = Params.CorrelationId;
        ps.FlowType = typeof(OrderFlow);

        var payload = new InvoiceCreatedSignalPayload()
        {
            InvoiceDate = InvoiceDate,
            ErrorMessage = ErrorMessage
        };

        await SendSignal(ps, OrderFlow.InvoiceCreatedSignal, payload);
        //await Task.Run(() =>
        //{
        //    _flowsProvider.SendSignal(ps, OrderFlow.InvoiceCreatedSignal, payload);
        //});
    }

    private async Task SaveInvoice()
    {
        IsSuccessful = await _repository.Save(Model);
    }

    private async Task Init()
    {
        Model.Id = Params.ExternalId;
        Model.CorrelationId = Params.CorrelationId;
        Model.OrderId = Params.ParentItemId;
    }
}
