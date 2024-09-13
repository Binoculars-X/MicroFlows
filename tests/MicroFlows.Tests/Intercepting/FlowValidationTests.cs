using MicroFlows.Application.Exceptions;
using MicroFlows.Domain.Models;
using MicroFlows.Tests.TestFlows;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Intercepting;

public class FlowValidationTests : IFlow
{
    public List<SignalJournalEntry> SignalJournal => throw new NotImplementedException();

    public string RefId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    [Fact]
    public void Should_Not_Allow_PrivateModel()
    {
        var ex = Assert.Throws<FlowValidationException>(() =>
        {
            IConfiguration configuration = null!;

            var app = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    configuration = config.Build();
                })
                .ConfigureServices(services =>
                {
                    services.AddMicroFlows(configuration)
                        .RegisterFlow<SamplePrivateFieldsFlow>();
                })
                .Build();
        });

        Assert.Contains(nameof(SamplePrivateFieldsFlow), ex.Message);
    }

    [Fact]
    public void Should_Allow_ReadOnlyPrivates()
    {
        IConfiguration configuration = null!;

        var app = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                services.AddMicroFlows(configuration)
                    .RegisterFlow<SampleStoringFlow>();
            })
            .Build();
    }

    [Fact]
    public void Should_Not_Allow_MissedFlowMethod()
    {
        var ex = Assert.Throws<FlowValidationException>(() =>
        {
            IConfiguration configuration = null!;

            var app = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    configuration = config.Build();
                })
                .ConfigureServices(services =>
                {
                    services.AddMicroFlows(configuration)
                        .RegisterFlow<FlowValidationTests>();
                })
                .Build();
        });

        Assert.Contains(nameof(FlowValidationTests), ex.Message);
    }

    public void Flow()
    { }

    public async Task Flow(int id)
    { }
}
