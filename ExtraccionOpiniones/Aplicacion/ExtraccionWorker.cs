using System.Diagnostics;
using ExtraccionOpiniones.Dominio;
using ExtraccionOpiniones.Infraestructura;

namespace ExtraccionOpiniones.Aplicacion;

public class ExtraccionWorker : BackgroundService
{
    private readonly IEnumerable<IExtractor> extractores;
    private readonly StagingWriter staging;
    private readonly ILogger<ExtraccionWorker> logger;
    private readonly IHostApplicationLifetime vida;

    public ExtraccionWorker(IEnumerable<IExtractor> extractores, StagingWriter staging,
        ILogger<ExtraccionWorker> logger, IHostApplicationLifetime vida)
    {
        this.extractores = extractores;
        this.staging = staging;
        this.logger = logger;
        this.vida = vida;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inicia el proceso de extraccion con {Cantidad} fuentes", extractores.Count());
        var reloj = Stopwatch.StartNew();

        var tareas = extractores.Select(e => EjecutarAsync(e, stoppingToken)).ToList();
        var resultados = await Task.WhenAll(tareas);

        reloj.Stop();
        logger.LogInformation("Proceso terminado: {Total} registros extraidos en {Ms} ms", resultados.Sum(), reloj.ElapsedMilliseconds);

        vida.StopApplication();
    }

    private async Task<int> EjecutarAsync(IExtractor extractor, CancellationToken token)
    {
        var reloj = Stopwatch.StartNew();

        try
        {
            var datos = await extractor.ExtraerAsync(token);
            await staging.GuardarAsync(extractor.Nombre.Replace(" ", "_"), datos);

            reloj.Stop();
            logger.LogInformation("{Fuente}: {Cantidad} registros guardados en staging en {Ms} ms",
                extractor.Nombre, datos.Count, reloj.ElapsedMilliseconds);
            return datos.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al extraer de {Fuente}", extractor.Nombre);
            return 0;
        }
    }
}
