namespace Futebol.Api.Domain;

public class Partida
{
    public int Id { get; set; }
    public int SorteioId { get; set; }
    public int TimeCasaId { get; set; }
    public int TimeVisitanteId { get; set; }

    public int? GolsCasa { get; set; }
    public int? GolsVisitante { get; set; }

    public string Status { get; set; } = "Pendente";
}
