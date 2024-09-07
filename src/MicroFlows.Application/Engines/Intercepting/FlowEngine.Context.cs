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
using System.Reflection;
using System.Linq;
using MicroFlows.Domain.Models;
using Microsoft.CodeAnalysis;
using MicroFlows.Application.Exceptions;

namespace MicroFlows.Application.Engines.Interceptors;
    
// InterceptorFlowRunEngine keeps state of running flow and cannot be shared with other scopes
internal partial class FlowEngine
{
    public async Task<FlowContext> PreloadContextHistory(string refId)
    {
        if (_contextHistory == null)
        {
            _contextHistory = await _flowRepository.GetFlowHistory(refId);
        }

        _context = _contextHistory.Last();
        _context = TypeHelper.CloneObject(_context);
        return _context;
    }

    private async Task AddContextToHistory(FlowContext context)
    {
        var copy = TypeHelper.CloneObject(context);
        _contextHistory.Add(copy);
    }

    private FlowContext? TryGetTaskExecutionContextFromHistory(string refId, string taskName, int callIndex)
    {
        var records = _contextHistory;

        for (int i = 0; i < records.Count; i++)
        {
            //if (records[i].CurrentTask == taskName && callIndex >= i)
            if (records[i].CurrentTask == taskName)
            {
                var startPosition = i;

                // iterate to last task with the same name
                while ((startPosition + 1) < records.Count)
                {
                    startPosition++;

                    if (records[startPosition].CurrentTask != taskName)
                    {
                        startPosition--;
                        break;
                    }
                }

                return records[startPosition];
            }
        }

        return null;
    }
}
