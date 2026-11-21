using System.Net.Http.Json;
using ExtraccionOpiniones.Dominio;
using Microsoft.Extensions.Options;

namespace ExtraccionOpiniones.Infraestructura;

public class ApiExtractor : IExtractor
{
    private readonly IHttpClientFactory fabrica;
    private readonly ConfiguracionFuentes config;
    private readonly ILogger<ApiExtractor> logger;

    public ApiExtractor(IHttpClientFactory fabrica, IOptions<ConfiguracionFuentes> opciones, ILogger<ApiExtractor> logger)
    {
        this.fabrica = fabrica;
        config = opciones.Value;
        this.logger = logger;
    }

    public string Nombre => "Comentarios API";

    public async Task<List<OpinionExtraida>> ExtraerAsync(CancellationToken token)
    {
        var lista = new List<OpinionExtraida>();
        var cliente = fabrica.CreateClient();

        var comentarios = await cliente.GetFromJsonAsync<List<ComentarioApi>>(config.UrlApi, token)
                          ?? new List<ComentarioApi>();

        foreach (var c in comentarios)
        {
            lista.Add(new OpinionExtraida
            {
                Origen = "RedSocial",
                IdExterno = c.IdComment,
                IdCliente = c.IdCliente,
                IdProducto = c.IdProducto,
                Fecha = c.Fecha,
                Comentario = c.Comentario,
                Clasificacion = c.Fuente
            });
        }

        logger.LogInformation("ApiExtractor recibio {Cantidad} comentarios de {Url}", lista.Count, config.UrlApi);
        return lista;
    }
}
