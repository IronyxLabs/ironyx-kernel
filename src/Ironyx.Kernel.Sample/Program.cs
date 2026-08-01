
using Ironyx.Kernel;
using Ironyx.Kernel.Sample.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));


builder.UseKernel()
    .AddGrpc(5000)

    .AddCommand<SampleCommand, SampleCommandHandler>()
    .AddQuery<SampleQuery, SampleQuery.Result, SampleQueryHandler>()

    .AddCommandSender(new Uri("http://localhost:5000"));

var app = builder.Build();

app.MapKernel();

app.Run();
