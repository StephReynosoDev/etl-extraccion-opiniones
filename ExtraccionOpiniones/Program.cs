using ExtraccionOpiniones.Aplicacion;
using ExtraccionOpiniones.Dominio;
using ExtraccionOpiniones.Infraestructura;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ConfiguracionFuentes>(builder.Configuration.GetSection("Fuentes"));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IExtractor, CsvExtractor>();
builder.Services.AddSingleton<IExtractor, DatabaseExtractor>();
builder.Services.AddSingleton<IExtractor, ApiExtractor>();
builder.Services.AddSingleton<StagingWriter>();
builder.Services.AddHostedService<ExtraccionWorker>();

var host = builder.Build();
host.Run();
