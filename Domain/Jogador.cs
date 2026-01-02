namespace Futebol.Api.Domain;

public class Jogador
{
    public int Id { get; set; }
    public int OrganizadorId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Posicao { get; set; }
    public string? Observacoes { get; set; }
    public bool Destaque { get; set; }
    public bool Peso { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organizador Organizador { get; set; } = null!;
    public ICollection<TimeJogador> Times { get; set; } = new List<TimeJogador>();
}
