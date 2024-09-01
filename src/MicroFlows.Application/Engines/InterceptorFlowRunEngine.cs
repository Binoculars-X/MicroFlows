using Castle.DynamicProxy;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Application.Engines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BlazorForms.Proxyma;
using System.Reflection;
using BlazorForms;
using System.Linq;
using MicroFlows.Domain.Models;

namespace MicroFlows;
    
// InterceptorFlowRunEngine keeps state of running flow and cannot be shared with other scopes
internal partial class InterceptorFlowRunEngine : IAsyncInterceptor, IFlowRunEngine
{
    private readonly ILogger<InterceptorFlowRunEngine> _logger;
    private readonly IServiceProvider _services;
    private readonly IProxymaProvider _proxyProvider;

    // running flow state
    FlowBase? _flow;
    FlowBase? _flowProxy;
    Type? _runningFlowType;
    private string[] _systemTasks = new string[] { "Begin", "BeginAsync", "EndAsync" };
    FlowContext _context;
    private int _callIndex;
    private List<string> _executionCallStack;
    private List<FlowContext> _contextHistory;

    public InterceptorFlowRunEngine(ILogger<InterceptorFlowRunEngine> logger, 
        IServiceProvider serviceProvider,
        IProxymaProvider proxyProvider)
    {
        _logger = logger;
        _services = serviceProvider;
        _proxyProvider = proxyProvider;
    }

    public async Task ExecuteFlow(string flowTypeName)
    {
        var type = TypeHelper.ResolveType(flowTypeName);
        await ExecuteFlow(type);
    }

    public async Task ExecuteFlow(Type flowType)
    {
        // construct flow
        _flow = _services.GetService(flowType) as FlowBase;
        _runningFlowType = flowType;

        var options = new ProxyGenerationOptions(new FreezableProxyGenerationHook(_flow));
        var flowParameters = TypeHelper.GetConstructorParameters(_services, flowType);

        try
        {
            _flowProxy = _proxyProvider.CreateClassProxy(flowType, flowParameters, _flow, options, [this]) as FlowBase;
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "CreateClassProxy failed");
            throw;
        }
    }

    public Task ResumeFlow(string flowId)
    {
        throw new NotImplementedException();
    }

    
}
