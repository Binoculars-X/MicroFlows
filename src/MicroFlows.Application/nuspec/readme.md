# MicroFlows
Use MicroFlows to implement Stateful workflows for microservices

**DI registration and usage:**

```
// program
...
    services.AddMicroFlows(configuration)
                .RegisterFlow<SampleFlow>()
                .RegisterFlow<OrderFlow>()
                ;

    services.AddMicroFlowsMsSqlRepo(configuration,
                new MsSqlFlowRepositorySettings
                {
                    // your connection string
                    ConnectionString = _msSqlContainer.GetConnectionString()
                });
...

    // constructor
    public void MyService(IFlowProvider flowProvider)
    {
        _flowProvider = flowProvider;
    }
...

    public async Task RunOrderProcessing
    {
        var ps = new FlowParams{ ps.FlowName = typeof(OrderFlow).FullName };
        ps["OrderId"] = "A000123";
        await _flowProvider.ExecuteFlow(ps); 
    }
...

// flow example
public class OrderFlow : FlowBase
{
    public bool InitPassed { get; set; }
    public bool UpdatePassed { get; set; }
    public bool InlinePassed { get; set; }
    public bool CallInlinePassed { get; set; }

    public async Task Flow()
    {
        await CallAsync(Init);
        await CallAsync(async () => await Update(1, "String"));
        
        InlinePassed = true;

        Call(() => CallInlinePassed = true);
    }

    private async Task Init()
    {
        InitPassed = true;
    }

    private async Task Update(int? key, string? name)
    {
        UpdatePassed = true;
    }
}
```

**Release Notes**

**1.0.0+**
- Added MsSqlFlowRepository
- Added Fluent Flows

**0.5.0+**
- Added Non Public Fields and Properties to model
- Migrated to new JsonPathToModel
- Added flow validations

**0.2.0-0.4.3**
- Added Flow Signals
- bug fixing

**0.1.***
- Initial import from ProCodersPtyLtd/BlazorForms
