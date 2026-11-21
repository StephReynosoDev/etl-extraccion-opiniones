using System.Globalization;
using CsvHelper;
using ExtraccionOpiniones.Dominio;
using Microsoft.Extensions.Options;

namespace ExtraccionOpiniones.Infraestructura;

public class CsvExtractor : IExtractor
{
    private readonly ConfiguracionFuentes config;
    private readonly ILogger<CsvExtractor> logger;

    public CsvExtractor(IOptions<ConfiguracionFuentes> opciones, ILogger<CsvExtractor> logger)
    {
        config = opciones.Value;
        this.logger = logger;
    }

    public string Nombre => "Encuestas CSV";

    public async Task<List<OpinionExtraida>> ExtraerAsync(CancellationToken token)
    {
        var lista = new List<OpinionExtraida>();
        var ruta = Path.Combine(AppContext.BaseDirectory, config.RutaCsv);

        using var lector = new StreamReader(ruta);
        using var csv = new CsvReader(lector, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            lista.Add(new OpinionExtraida
            {
                Origen = "Encuesta",
                IdExterno = csv.GetField(0) ?? "",
                IdCliente = csv.GetField(1) ?? "",
                IdProducto = csv.GetField(2) ?? "",
                Fecha = csv.GetField(3) ?? "",
                Comentario = csv.GetField(4) ?? "",
                Clasificacion = csv.GetField(5) ?? "",
                Calificacion = csv.GetField(6) ?? ""
            });
        }

        logger.LogInformation("CsvExtractor leyo {Cantidad} filas de {Ruta}", lista.Count, ruta);
        return lista;
    }
}
