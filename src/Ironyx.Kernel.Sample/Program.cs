
using Ironyx.Kernel;
using Ironyx.Kernel.Sample.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));


builder.UseKernel()
    .AddCommand<SampleCommand, SampleCommandHandler>()
    .AddQuery<SampleQuery, SampleQuery.Result>();

var app = builder.Build();

app.MapKernel();

app.Run();
