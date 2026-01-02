namespace Futebol.Api.Domain;

public class Sorteio
{
    public int Id { get; set; }
    public Guid OrganizadorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeTimes { get; set; }
    public string Status { get; set; } = "Aberto";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Time> Times { get; set; } = new List<Time>();

    public void Finalizar() => Status = "Finalizado";
}
