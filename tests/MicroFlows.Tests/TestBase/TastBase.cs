using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MicroFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFlows.Tests.TestSampleFlows;
using MicroFlows.Tests.TestFlows;

namespace MicroFlows.Tests.TestBase;
public abstract class TastBase
{
    protected IServiceProvider _services = null!;

    public TastBase()
    {
        IConfiguration configuration = null!;

        var app = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
                config.AddConfiguration(hostingContext.Configuration);
                config.AddJsonFile("appsettings.json");
                config.AddJsonFile($"appsettings.Development.json", true, true);
                //config.AddUserSecrets(typeof(ExceptionHandlingMiddleware).Assembly);
                configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                services.AddMicroFlows(configuration)
                    .RegisterFlow<SampleFlow>()
                    .RegisterFlow<SampleStoringFlow>()
                    ;
                
            })
            .Build();

        _services = app.Services;
    }
}
