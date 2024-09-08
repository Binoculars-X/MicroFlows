# MicroFlows
Use MicroFlows to implement Stateful workflows for microservices

**DI registration:**

```
// program
...
    services.AddMicroFlows(configuration)
                .RegisterFlow<SampleFlow>()
                .RegisterFlow<SampleStoringFlow>()
                ;
...

// constructor
public void MyService(IFlowsProvider flowsProvider)
...
```

**Usage:**

```
// ToDo: add usage
```

**Release Notes**

**0.2.0-0.2.***
- Added Flow Signals
- bug fixing

**0.1.***
- Initial import from ProCodersPtyLtd/BlazorForms
