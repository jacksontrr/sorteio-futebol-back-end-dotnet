namespace Futebol.Api.Dtos;

public record TimeDto(int Id, int SorteioId, string Nome);
public record AdicionarTimeDto(string Nome, List<int> JogadorIds);

public record AdicionarMultiplosTimesDto(List<AdicionarTimeDto> times);

