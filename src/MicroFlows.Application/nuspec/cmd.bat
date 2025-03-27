rem

cd C:\repos\MicroFlows\src\MicroFlows.Application\nuspec\
nuget.exe pack MicroFlows.nuspec -NonInteractive -OutputDirectory c:\temp\Nuget -Verbosity Detailed

rem cd C:\repos\BlazorForms\src\rendering\BlazorForms.Rendering.Flows\
rem dotnet pack BlazorForms.Rendering.Flows.csproj --output  c:\temp\Nuget

rem cd C:\repos\BlazorForms\src\rendering\BlazorForms.Rendering.MudBlazorUI\
rem dotnet pack BlazorForms.Rendering.MudBlazorUI.csproj --output  c:\temp\Nuget

pause