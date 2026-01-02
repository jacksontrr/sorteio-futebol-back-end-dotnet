namespace Futebol.Api.Domain;

public class Time
{
    public int Id { get; set; }
    public int SorteioId { get; set; }
    public string Nome { get; set; } = string.Empty;

    public Sorteio Sorteio { get; set; } = null!;
    public ICollection<TimeJogador> Jogadores { get; set; } = new List<TimeJogador>();
}