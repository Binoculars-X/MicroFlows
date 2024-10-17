using JsonPathToModel;
using MicroFlows.Application;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Tests.Helpers;
using MicroFlows.Tests.Intercepting;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.OrderProcessing;

public class OrderProcessingTests
{
    private readonly IServiceProvider _services;
    private readonly IFlowProvider _flowsProvider;
    private readonly IFlowRepository _flowsRepository;
    private readonly Mock<IInvoiceRepository> _invoiceRepository;

    public OrderProcessingTests()
    {
        _invoiceRepository = new Mock<IInvoiceRepository>();

        _services = ConfigHelper.GetConfigurationServices((services, configuration) => 
        {
            services
                .AddSingleton<IFlowRepository, MemoryFlowRepository>()
                .AddSingleton<IInvoiceRepository>(_invoiceRepository.Object)
                .AddMicroFlows(configuration)
                    .RegisterFlow<OrderFlow>()
                    .RegisterFlow<InvoiceFlow>()
                ;
        });

        _flowsProvider = _services.GetService<IFlowProvider>();
        _flowsRepository = _services.GetService<IFlowRepository>();
    }

    [Fact]
    public async Task OrderProcessingFlows_Should_FinishSuccessfully()
    {
        _invoiceRepository.Setup(c => c.Save(It.IsAny<InvoiceModel>())).Returns(Task.FromResult(true));

        var ps = new FlowParams();
        ps.ExternalId = "1234";
        ps.FlowType = typeof(OrderFlow);

        // on the first pass OrderFlow will be stopped to wating signal from InvoiceFlow
        var ctx = await _flowsProvider.ExecuteFlow(ps);
        var flow = await _flowsRepository.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_StartInvoiceFlow:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("WaitForSignalAsync:3", flow.ContextHistory[3].CurrentTask);

        // on the second pass the signal should be received
        var ctx2 = await _flowsProvider.ExecuteFlow(ps);
        Assert.Equal(ctx.RefId, ctx2.RefId);

        var flow2 = await _flowsRepository.GetFlowModel(ctx2.RefId);
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);

        var callstack = flow2.ContextHistory.Select(c => c.CurrentTask).Distinct().ToList();
        Assert.Equal(6, callstack.Count);
        Assert.Equal("CallAsync_Finish:4", callstack[4]);
        Assert.Equal(true, flow2.ContextHistory.Last().Model.Records["$.IsSuccessful"].Deserialize());
        Assert.Equal("End:5", callstack[5]);
    }

    [Fact]
    public async Task OrderProcessingFlows_Should_FailWhenCannotSaveInvoice()
    {
        _invoiceRepository.Setup(c => c.Save(It.IsAny<InvoiceModel>())).Returns(Task.FromResult(false));

        var ps = new FlowParams();
        ps.ExternalId = "12345";
        ps.FlowType = typeof(OrderFlow);

        var ctx = await _flowsProvider.ExecuteFlow(ps);
        var flow = await _flowsRepository.GetFlowModel(ctx.RefId);

        Assert.Equal(4, flow.ContextHistory.Count);
        Assert.Equal(ResultStateEnum.Success, ctx.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Stop, ctx.ExecutionResult.FlowState);

        Assert.Equal("Begin:0", flow.ContextHistory[0].CurrentTask);
        Assert.Equal("CallAsync_Init:1", flow.ContextHistory[1].CurrentTask);
        Assert.Equal("CallAsync_StartInvoiceFlow:2", flow.ContextHistory[2].CurrentTask);
        Assert.Equal("WaitForSignalAsync:3", flow.ContextHistory[3].CurrentTask);

        var ctx2 = await _flowsProvider.ExecuteFlow(ps);
        Assert.Equal(ctx.RefId, ctx2.RefId);

        var flow2 = await _flowsRepository.GetFlowModel(ctx2.RefId);
        Assert.Equal(ResultStateEnum.Success, ctx2.ExecutionResult.ResultState);
        Assert.Equal(FlowStateEnum.Finished, ctx2.ExecutionResult.FlowState);

        var callstack = flow2.ContextHistory.Select(c => c.CurrentTask).Distinct().ToList();
        Assert.Equal(6, callstack.Count);
        Assert.Equal("CallAsync_Finish:4", callstack[4]);
        Assert.Equal(true, flow2.ContextHistory.Last().Model.Records["$.IsFailed"].Deserialize());
        Assert.Equal("End:5", callstack[5]);
    }
}
