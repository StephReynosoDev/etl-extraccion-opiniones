using System.Data;
using Microsoft.Data.SqlClient;
using CargaDimensiones.Modelos;

namespace CargaDimensiones.Servicios;

public class Cargador
{
    private readonly string conexionOrigen = "Server=STEPH\\SQLEXPRESS;Database=OpinionesClientes;Trusted_Connection=True;TrustServerCertificate=True;";
    private readonly string conexionDestino = "Server=STEPH\\SQLEXPRESS;Database=DataWarehouse;Trusted_Connection=True;TrustServerCertificate=True;";

    public Resultado CargarClientes()
    {
        return CargarDesdeConsulta("DimCliente",
            "SELECT id_cliente, nombre, correo FROM Cliente",
            "sp_CargarDimCliente",
            new string[] { "@id", "@nombre", "@correo" });
    }

    public Resultado CargarProductos()
    {
        return CargarDesdeConsulta("DimProducto",
            "SELECT id_producto, nombre, categoria FROM Producto",
            "sp_CargarDimProducto",
            new string[] { "@id", "@nombre", "@categoria" });
    }

    public Resultado CargarFuentes()
    {
        return CargarDesdeConsulta("DimFuente",
            "SELECT id_fuente, nombre FROM FuentesDatos",
            "sp_CargarDimFuente",
            new string[] { "@id", "@nombre" });
    }

    public Resultado CargarTiempo()
    {
        var resultado = new Resultado { Nombre = "DimTiempo" };

        try
        {
            using var origen = new SqlConnection(conexionOrigen);
            using var destino = new SqlConnection(conexionDestino);
            origen.Open();
            destino.Open();

            DateTime inicio;
            DateTime fin;

            using (var comando = new SqlCommand("SELECT MIN(fecha), MAX(fecha) FROM Opinion", origen))
            using (var lector = comando.ExecuteReader())
            {
                lector.Read();
                if (lector.IsDBNull(0))
                {
                    resultado.Errores.Add("No hay fechas en la tabla Opinion");
                    return resultado;
                }
                inicio = lector.GetDateTime(0);
                fin = lector.GetDateTime(1);
            }

            for (var dia = inicio; dia <= fin; dia = dia.AddDays(1))
            {
                resultado.Leidos++;
                try
                {
                    string accion = Ejecutar(destino, "sp_CargarDimTiempo",
                        new string[] { "@fecha" }, new object[] { dia });
                    Aplicar(resultado, accion);
                }
                catch (SqlException ex)
                {
                    resultado.Errores.Add("Fecha " + dia.ToString("yyyy-MM-dd") + ": " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            resultado.Errores.Add("Error general: " + ex.Message);
        }

        return resultado;
    }

    public Resultado CargarSentimientos()
    {
        var resultado = new Resultado { Nombre = "DimSentimiento" };
        string[,] datos =
        {
            { "Positivo", "Opinion favorable" },
            { "Neutro", "Opinion intermedia" },
            { "Negativo", "Opinion desfavorable" }
        };

        try
        {
            using var destino = new SqlConnection(conexionDestino);
            destino.Open();

            for (int i = 0; i < datos.GetLength(0); i++)
            {
                resultado.Leidos++;
                try
                {
                    string accion = Ejecutar(destino, "sp_CargarDimSentimiento",
                        new string[] { "@nombre", "@descripcion" },
                        new object[] { datos[i, 0], datos[i, 1] });
                    Aplicar(resultado, accion);
                }
                catch (SqlException ex)
                {
                    resultado.Errores.Add("Sentimiento " + datos[i, 0] + ": " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            resultado.Errores.Add("Error general: " + ex.Message);
        }

        return resultado;
    }

    private Resultado CargarDesdeConsulta(string nombre, string consulta, string procedimiento, string[] parametros)
    {
        var resultado = new Resultado { Nombre = nombre };

        try
        {
            using var origen = new SqlConnection(conexionOrigen);
            using var destino = new SqlConnection(conexionDestino);
            origen.Open();
            destino.Open();

            using var comando = new SqlCommand(consulta, origen);
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                resultado.Leidos++;
                try
                {
                    var valores = new object[parametros.Length];
                    lector.GetValues(valores);
                    string accion = Ejecutar(destino, procedimiento, parametros, valores);
                    Aplicar(resultado, accion);
                }
                catch (SqlException ex)
                {
                    resultado.Errores.Add("Registro " + resultado.Leidos + ": " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            resultado.Errores.Add("Error general: " + ex.Message);
        }

        return resultado;
    }

    private string Ejecutar(SqlConnection con, string procedimiento, string[] nombres, object[] valores)
    {
        using var cmd = new SqlCommand(procedimiento, con);
        cmd.CommandType = CommandType.StoredProcedure;
        for (int i = 0; i < nombres.Length; i++)
        {
            cmd.Parameters.AddWithValue(nombres[i], valores[i]);
        }
        return Convert.ToString(cmd.ExecuteScalar()) ?? "";
    }

    private void Aplicar(Resultado resultado, string accion)
    {
        switch (accion)
        {
            case "INSERTADO":
                resultado.Insertados++;
                break;
            case "ACTUALIZADO":
                resultado.Actualizados++;
                break;
            default:
                resultado.SinCambios++;
                break;
        }
    }
}
