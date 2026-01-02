namespace Futebol.Api.Dtos;

public record RegistrarResultadoDto(
    int TimeCasa,
    int GolsCasa,
    int GolsVisitante,
    int TimeVisitante,
    int SorteioId
);

public record PartidaDto(
    int Id,
    int SorteioId,
    int TimeCasaId,
    int TimeVisitanteId,
    int GolsCasa,
    int GolsVisitante
);
