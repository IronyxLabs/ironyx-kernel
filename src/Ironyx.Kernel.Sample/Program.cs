
using Ironyx.Kernel;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Sample.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((_, configuration) =>
{
    configuration.ReadFrom.Configuration(builder.Configuration);
});


builder.UseKernel()
    .AddCommand<SampleCommand>()

    .AddHandler<SampleCommand, SampleCommandHandler>();

var app = builder.Build();

app.MapKernel();

app.MapPost("/command", async context => await context.RequestServices.GetRequiredService<ICommandDispatcher>().DispatchAsync((await context.Request.ReadFromJsonAsync<SampleCommand>())!, context.RequestAborted));

app.Run();
