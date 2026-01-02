namespace Futebol.Api.Dtos;

public record JogadorResponseDto(
    int Id,
    string Nome,
    string? Posicao,
    bool Destaque,
    bool Peso,
    bool Ativo,
    string Observacoes,
    DateTime CreatedAt
);

public record JogadorListItemDto(
    int Id,
    string Nome,
    List<string> Posicoes,
    string Observacoes,
    bool Destaque,
    bool Peso,
    bool Ativo,
    DateTime CreatedAt
);

public record AtualizarJogadorDto(
    string Nome,
    List<string>? Posicoes,
    string? Observacoes,
    bool Destaque,
    bool Peso
);

public record AtualizarStatusJogadorDto(
    bool Ativo
);
