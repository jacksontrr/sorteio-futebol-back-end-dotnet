namespace Futebol.Api.Domain;

public class User
{
    public Guid Id { get; set; }

    // Dados básicos
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Auth (simples por enquanto)
    public string PasswordHash { get; set; } = string.Empty;

    // Recuperação de Senha
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }

    public bool Ativo { get; set; } = true;
    public bool ContaGoogle { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relacionamento
    public ICollection<Organizador> Organizadores { get; set; } = new List<Organizador>();
}