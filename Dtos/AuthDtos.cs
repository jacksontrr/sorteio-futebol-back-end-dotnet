namespace Futebol.Api.Dtos;

public record RegisterDto(string Nome, string Email, string Password);

public record RegisterJogadorDto(string Nome, List<string> Posicoes, string? Observacoes = null, bool Destaque = false, bool Peso = false, string? Codigo = null);

public record LoginDto(string Email, string Password, bool Remember);

public record AuthResponseDto(string Token, DateTime ExpiresAtUtc, int UserId, string Nome, string Email);

public record GoogleLoginDto(string Token);

public record RefreshTokenDto(string Token);

public record RefreshTokenResponseDto(string Token, DateTime ExpiresAtUtc);

public record UserProfileDto(string Nome, string Email, bool ContaGoogle, string Codigo);

public record ChangePasswordDto(string? SenhaAtual, string NovaSenha);

public record UpdateNameDto(string Nome);

public record UpdateCodigoDto(string Codigo);