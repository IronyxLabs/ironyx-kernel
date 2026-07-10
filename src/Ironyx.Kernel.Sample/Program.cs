
using Ironyx.Kernel;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Receivers;
using Ironyx.Kernel.Sample.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.UseKernel()
    .AddHandler<SampleCommand, SampleCommandHandler>();

var app = builder.Build();

app.MapGrpcService<GrpcEndpoint>();
app.MapPost("/command", async context => await context.RequestServices.GetRequiredService<ICommandDispatcher>().DispatchAsync((await context.Request.ReadFromJsonAsync<SampleCommand>())!, context.RequestAborted));

app.Run();
