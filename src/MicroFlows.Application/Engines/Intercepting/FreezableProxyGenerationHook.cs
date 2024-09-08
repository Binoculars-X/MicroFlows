using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace MicroFlows.Application.Engines.Interceptors;
public class FreezableProxyGenerationHook : IProxyGenerationHook
{
    private readonly string[] _systemMethods = [
        "Call", 
        "CallAsync", 
        "ExecuteActivity",
        "ExecuteActivityAsync",
        "WaitForCondition",
        "WaitForConditionAsync",
        "WaitForSignalAsync",
        ];

    private IFlow _flow;

    public FreezableProxyGenerationHook(IFlow flow)
    {
        _flow = flow;
    }

    public override int GetHashCode()
    {
        return _flow.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return _flow == (obj as FreezableProxyGenerationHook)._flow;
    }

    public bool ShouldInterceptMethod(Type type, MethodInfo memberInfo)
    {
        if (typeof(FlowBase).IsAssignableFrom(type) && _systemMethods.Contains(memberInfo.Name))
        {
            return true;
        }

        return false;
        //if (memberInfo.Name == "Execute" && typeof(FlowBase).IsAssignableFrom(type))
        //{
        //    return false;
        //}

        //return true;
    }

    public void NonVirtualMemberNotification(Type type, MemberInfo memberInfo)
    {
    }

    public void MethodsInspected()
    {
    }

    public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
    {

    }
}
