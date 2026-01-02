namespace Futebol.Api.Domain;

public class Organizador
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public string Nome { get; set; } = string.Empty;

    public string Codigo { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
}