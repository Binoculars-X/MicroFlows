using BlazorForms;
using MicroFlows.AdminUI.Components;
using MicroFlows.AdminUI.Forms;
using MicroFlows;
using MicroFlows.AdminUI.Components.Flows;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// BlazorForms
builder.Services.AddServerSideBlazorForms();
builder.Services.AddBlazorFormsMudBlazorUI();
builder.Services.AddBlazorFormsServerModelAssemblyTypes(typeof(FlowInstanceListFlow));

builder.Services.AddMicroFlowsAdmin();

builder.Services.AddMicroFlowsMsSqlRepo(configuration,
    new MsSqlFlowRepositorySettings
    {
        ConnectionString = configuration.GetConnectionString("MicroFlowsSql"),
        CreateDatabase = true,
        DatabaseName = "MicroFlowsDB"
    });

// DI
builder.Services.AddScoped<LocalSettings>();
builder.Services.AddScoped<FlowHistoryViewModel>();
builder.Services.AddTransient<FlowHistoryItemViewModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// BlazorForms
app.BlazorFormsRun();

app.Run();
