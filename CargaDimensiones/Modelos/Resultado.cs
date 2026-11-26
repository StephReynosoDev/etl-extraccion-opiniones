namespace CargaDimensiones.Modelos;

public class Resultado
{
    public string Nombre { get; set; } = "";
    public int Leidos { get; set; }
    public int Insertados { get; set; }
    public int Actualizados { get; set; }
    public int SinCambios { get; set; }
    public List<string> Errores { get; set; } = new List<string>();
}
