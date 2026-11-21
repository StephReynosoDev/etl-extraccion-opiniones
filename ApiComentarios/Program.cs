using System.Globalization;
using CsvHelper;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/comentarios", () =>
{
    var ruta = Path.Combine(AppContext.BaseDirectory, "Datos", "social_comments.csv");
    var lista = new List<object>();

    using var lector = new StreamReader(ruta);
    using var csv = new CsvReader(lector, CultureInfo.InvariantCulture);

    csv.Read();
    csv.ReadHeader();

    while (csv.Read())
    {
        lista.Add(new
        {
            idComment = csv.GetField(0),
            idCliente = csv.GetField(1),
            idProducto = csv.GetField(2),
            fuente = csv.GetField(3),
            fecha = csv.GetField(4),
            comentario = csv.GetField(5)
        });
    }

    return Results.Ok(lista);
});

app.Run("http://localhost:5100");
