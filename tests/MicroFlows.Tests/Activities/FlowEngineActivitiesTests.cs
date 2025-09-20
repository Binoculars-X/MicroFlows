using Castle.DynamicProxy;
using JsonPathToModel;
using MicroFlows.Application.Engines.Interceptors;
using MicroFlows.Application.Exceptions;
using MicroFlows.Application.Helpers;
using MicroFlows.Domain.Enums;
using MicroFlows.Domain.Interfaces;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MicroFlows.Tests.Activities.FlowEngineActivitiesTests;

namespace MicroFlows.Tests.Activities;

public partial class FlowEngineActivitiesTests : TestBase
{
    public class OrderCancelledActivityPayload
    { }

    public class OrderCancelledActivity
    {
        // SignalPayload should be called by the framework to deserialize to target type OrderCancelledActivityPayload
        public async Task Run(SampleModel Model, OrderCancelledActivityPayload payload)
        {
        }
    }

    public class SampleModel
    {
    }

    public class SampleCheckSignalActivityFlow : FlowBase<SampleModel>
    {


        // signals
        public const string OrderAcceptedSignal = "OrderAccpetedSignal";
        public const string OrderCancelledSignal = "OrderCancelledSignal";

        // model consists of all public serializable properties
        public string OrderId { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string OrderStatus { get; set; }

        public override void RegisterSignals()
        {
            AddSignalActivity(OrderCancelledSignal, typeof(OrderCancelledActivity));
        }

        public async Task Flow()
        {
            Call(Init);

            await CheckSignalReceivedAsync(OrderCancelledSignal);
            await WaitForSignalAsync(OrderAcceptedSignal);
            await CheckSignalReceivedAsync(OrderCancelledSignal);

            await CallAsync(Finish);
        }



        private async Task Finish()
        {
            if (CancelDate != null)
            {
                OrderStatus = "Cancelled";
            }
            else
            {
                OrderStatus = "Processed";
                FinishDate = DateTime.UtcNow;
            }
        }

        private void Init()
        {
            OrderStatus = "Generated";
        }
    }
}