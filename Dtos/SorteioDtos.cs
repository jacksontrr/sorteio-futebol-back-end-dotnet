namespace Futebol.Api.Dtos;

public record CriarSorteioDto(
    string Nome
);

public record ExecutarSorteioDto(
    int SorteioId,
    List<int> JogadorIds
);
