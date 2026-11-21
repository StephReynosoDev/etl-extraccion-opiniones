namespace ExtraccionOpiniones.Dominio;

public interface IExtractor
{
    string Nombre { get; }
    Task<List<OpinionExtraida>> ExtraerAsync(CancellationToken token);
}
