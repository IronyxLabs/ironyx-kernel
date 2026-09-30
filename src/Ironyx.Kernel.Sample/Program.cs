
using Ironyx.Kernel;
using Ironyx.Kernel.Sample.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));


builder.UseKernel()
    .AddGrpc(builder => builder.AddRequestReceiver(5000).AddRequestSender(new Uri("http://localhost:5000/")))

    .AddCommand<SampleCommand, SampleCommandHandler>(builder => builder.AddValidator<SampleCommandValidator>())
    .AddQuery<SampleQuery, SampleQuery.Result, SampleQueryHandler>();


var app = builder.Build();

app.MapKernel();

app.Run();
