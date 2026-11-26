using System.Diagnostics;
using System.Text;
using CargaDimensiones.Modelos;
using CargaDimensiones.Servicios;

Console.OutputEncoding = Encoding.UTF8;

var cargador = new Cargador();
var resultados = new List<Resultado>();
var reloj = Stopwatch.StartNew();

Console.WriteLine("=== CARGA DE DIMENSIONES - DATAWAREHOUSE ===");

string[] dimensiones = { "cliente", "producto", "fuente", "tiempo", "sentimiento" };

foreach (var dimension in dimensiones)
{
    switch (dimension)
    {
        case "cliente":
            resultados.Add(cargador.CargarClientes());
            break;
        case "producto":
            resultados.Add(cargador.CargarProductos());
            break;
        case "fuente":
            resultados.Add(cargador.CargarFuentes());
            break;
        case "tiempo":
            resultados.Add(cargador.CargarTiempo());
            break;
        case "sentimiento":
            resultados.Add(cargador.CargarSentimientos());
            break;
    }
}

reloj.Stop();

foreach (var r in resultados)
{
    Console.WriteLine();
    Console.WriteLine("--- " + r.Nombre + " ---");
    Console.WriteLine("Leidos: " + r.Leidos + " | Insertados: " + r.Insertados + " | Actualizados: " + r.Actualizados + " | Sin cambios: " + r.SinCambios + " | Errores: " + r.Errores.Count);
    foreach (var e in r.Errores)
    {
        Console.WriteLine("   " + e);
    }
}

Console.WriteLine();
Console.WriteLine("=== RESUMEN FINAL ===");
Console.WriteLine("Total leidos: " + resultados.Sum(r => r.Leidos));
Console.WriteLine("Total insertados: " + resultados.Sum(r => r.Insertados));
Console.WriteLine("Total actualizados: " + resultados.Sum(r => r.Actualizados));
Console.WriteLine("Total sin cambios: " + resultados.Sum(r => r.SinCambios));
Console.WriteLine("Total errores: " + resultados.Sum(r => r.Errores.Count));
Console.WriteLine("Tiempo total: " + reloj.ElapsedMilliseconds + " ms");
