using MicroFlows.Tests.TestFlows;
using MicroFlows.Tests.TestSampleFlows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.Helpers;

public static class ConfigHelper
{
    public static IServiceProvider GetConfigurationServices(Action<IServiceCollection, IConfiguration> action)
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        action(services, configuration);
        return services.BuildServiceProvider();
    }
}
