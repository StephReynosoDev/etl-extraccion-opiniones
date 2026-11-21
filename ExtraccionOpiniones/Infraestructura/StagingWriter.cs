using System.Text.Encodings.Web;
using System.Text.Json;
using ExtraccionOpiniones.Dominio;
using Microsoft.Extensions.Options;

namespace ExtraccionOpiniones.Infraestructura;

public class StagingWriter
{
    private readonly ConfiguracionFuentes config;

    public StagingWriter(IOptions<ConfiguracionFuentes> opciones)
    {
        config = opciones.Value;
    }

    public async Task GuardarAsync(string nombre, List<OpinionExtraida> datos)
    {
        var carpeta = Path.GetFullPath(config.CarpetaStaging);
        Directory.CreateDirectory(carpeta);

        var opciones = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var ruta = Path.Combine(carpeta, nombre + ".json");
        await File.WriteAllTextAsync(ruta, JsonSerializer.Serialize(datos, opciones));
    }
}
