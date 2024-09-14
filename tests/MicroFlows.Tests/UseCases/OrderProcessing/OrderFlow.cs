using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.OrderProcessing;

public class OrderFlow : FlowBase
{
    // dependencies
    //private readonly IFlowsProvider _flowsProvider;

    // signals
    public static string InvoiceCreatedSignal = "InvoiceCreatedSignal";

    // model
    public string? OrderId;
    public DateTime? OrderCreatedDate;
    public string? InvoiceId;
    public bool IsFailed;
    public bool IsSuccessful;

    public DateTime? InvoiceDate;
    public InvoiceCreatedSignalPayload InvoicePayload;

    public OrderFlow(/*IFlowsProvider flowsProvider*/)
    {
        //_flowsProvider = flowsProvider;
    }

    public async Task Flow()
    {
        AddSignalHandler(InvoiceCreatedSignal, InvoiceCreatedSignalHandler);

        await CallAsync(Init);
        await CallAsync(StartInvoiceFlow);
        
        await WaitForSignalAsync(InvoiceCreatedSignal);

        await CallAsync(Finish);
    }

    private async Task Finish()
    {
        if (InvoicePayload.ErrorMessage != null)
        {
            IsFailed = true;
            return;
        }

        IsSuccessful = true;
    }

    private async Task InvoiceCreatedSignalHandler(SignalPayload payload)
    {
        InvoicePayload = payload.GetValue<InvoiceCreatedSignalPayload>()!;
    }

    private async Task StartInvoiceFlow()
    {
        var ps = new FlowParams();
        ps.ExternalId = InvoiceId;
        // CorrelationId contains initial ExternalID, generated outside
        ps.CorrelationId = Params.ExternalId;
        ps.ParentItemId = OrderId;
        ps.FlowType = typeof(InvoiceFlow);

        await ExecuteFlow(ps);
        // Another flow must be run in a separate thread
        //await Task.Run(() =>
        //{
        //    _flowsProvider.ExecuteFlow(ps);
        //});

    }

    private async Task Init()
    {
        OrderId = $"ORDER.{Params.ExternalId}";
        InvoiceId = $"INV.{Params.ExternalId}";
        OrderCreatedDate = DateTime.UtcNow;
    }
}

public class InvoiceCreatedSignalPayload
{
    public DateTime? InvoiceDate {  get; set; }
    public string? ErrorMessage {  get; set; }
}
