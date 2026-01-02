using Futebol.Api.Domain;
using Futebol.Api.Dtos;
using Futebol.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Futebol.Api.Endpoints;

public static class PartidasEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/partidas", async (
            RegistrarResultadoDto dto,
            FutebolDbContext db) =>
        {

            Partida partida = new Partida
            {
                GolsCasa = dto.GolsCasa,
                GolsVisitante = dto.GolsVisitante,
                TimeCasaId = dto.TimeCasa,
                TimeVisitanteId = dto.TimeVisitante,
                SorteioId = dto.SorteioId,
                Status = "Finalizado"
            };
            db.Partidas.Add(partida);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapGet("/api/partidas/sorteio/{sorteioId:int}", async (
            int sorteioId,
            FutebolDbContext db) =>
        {
            var partidas = await db.Partidas
                .Where(p => p.SorteioId == sorteioId)
                .Select(p => new PartidaDto(
                    p.Id,
                    p.SorteioId,
                    p.TimeCasaId,
                    p.TimeVisitanteId,
                    p.GolsCasa.Value,
                    p.GolsVisitante.Value))
                .ToListAsync();

            return Results.Ok(partidas);
        }).AllowAnonymous();
    }
}
