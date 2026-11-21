using ExtraccionOpiniones.Dominio;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ExtraccionOpiniones.Infraestructura;

public class DatabaseExtractor : IExtractor
{
    private readonly ConfiguracionFuentes config;
    private readonly ILogger<DatabaseExtractor> logger;

    public DatabaseExtractor(IOptions<ConfiguracionFuentes> opciones, ILogger<DatabaseExtractor> logger)
    {
        config = opciones.Value;
        this.logger = logger;
    }

    public string Nombre => "Resenas Web BD";

    public async Task<List<OpinionExtraida>> ExtraerAsync(CancellationToken token)
    {
        var lista = new List<OpinionExtraida>();

        await using var con = new SqlConnection(config.CadenaConexionResenas);
        await con.OpenAsync(token);

        await using var cmd = new SqlCommand(
            "SELECT IdReview, IdCliente, IdProducto, CONVERT(VARCHAR(10), Fecha, 23), Comentario, Rating FROM dbo.ResenasWeb", con);
        await using var lector = await cmd.ExecuteReaderAsync(token);

        while (await lector.ReadAsync(token))
        {
            lista.Add(new OpinionExtraida
            {
                Origen = "ResenaWeb",
                IdExterno = Convert.ToString(lector[0]) ?? "",
                IdCliente = Convert.ToString(lector[1]) ?? "",
                IdProducto = Convert.ToString(lector[2]) ?? "",
                Fecha = Convert.ToString(lector[3]) ?? "",
                Comentario = Convert.ToString(lector[4]) ?? "",
                Calificacion = Convert.ToString(lector[5]) ?? ""
            });
        }

        logger.LogInformation("DatabaseExtractor leyo {Cantidad} filas de dbo.ResenasWeb", lista.Count);
        return lista;
    }
}
