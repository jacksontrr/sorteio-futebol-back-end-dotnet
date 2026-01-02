namespace Futebol.Api.Domain;

public class TimeJogador
{
    public int Id { get; set; }
    public int TimeId { get; set; }
    public int JogadorId { get; set; }

    public Time Time { get; set; } = null!;
    public Jogador Jogador { get; set; } = null!;
}
