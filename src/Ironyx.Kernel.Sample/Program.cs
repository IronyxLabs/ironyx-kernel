
using Ironyx.Kernel;
using Ironyx.Kernel.Sample.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));


builder.UseKernel()
    .AddCommand<SampleCommand>()
    .AddQuery<SampleQuery, SampleQuery.Result>()

    .AddHandler<SampleCommand, SampleCommandHandler>()
    .AddHandler<SampleQuery, SampleQuery.Result, SampleQueryHandler>();

var app = builder.Build();

app.MapKernel();

app.Run();
