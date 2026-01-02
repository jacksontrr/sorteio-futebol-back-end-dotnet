namespace Futebol.Api.Dtos;

public record UserResponseDto(
    Guid Id,
    string Nome,
    string Email
);

public record OrganizadorResponseDto(
    int Id,
    string Nome,
    string Codigo,
    bool Ativo
);