using Futebol.Api.Domain;
using Futebol.Api.Dtos;
using Futebol.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Futebol.Api.Endpoints
{
    public static class SorteiosEndpoints
    {
        public static void Map(this WebApplication app)
        {
            app.MapPost("/api/sorteios", async (CriarSorteioDto dto, ClaimsPrincipal user, FutebolDbContext db) =>
            {
                var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();
                var sorteio = new Sorteio
                {
                    OrganizadorId = userId,
                    Nome = dto.Nome
                };

                db.Sorteios.Add(sorteio);
                await db.SaveChangesAsync();
                return Results.Created($"/api/sorteios/{sorteio.Id}", sorteio.Id);
            }).RequireAuthorization();

            app.MapPost("/api/sorteios/{id:int}/times", async (
                int id,
                AdicionarMultiplosTimesDto dtos,
                FutebolDbContext db) =>
            {
                var sorteio = await db.Sorteios.FirstAsync(x => x.Id == id);
                var created = new List<TimeDto>();

                foreach (var dto in dtos.times)
                {
                    var time = new Time
                    {
                        SorteioId = id,
                        Nome = dto.Nome
                    };

                    db.Times.Add(time);
                    await db.SaveChangesAsync(); // ensure TimeId is generated

                    foreach (var jogadorId in dto.JogadorIds)
                    {
                        var timeJogador = new TimeJogador
                        {
                            TimeId = time.Id,
                            JogadorId = jogadorId
                        };
                        db.TimeJogadores.Add(timeJogador);
                    }

                    created.Add(new TimeDto(time.Id, time.SorteioId, time.Nome));
                }

                sorteio.QuantidadeTimes = dtos.times.Count;
                db.Sorteios.Update(sorteio);
                await db.SaveChangesAsync();

                return Results.Ok(created);
            }).RequireAuthorization();

            app.MapGet("/api/sorteios/{id:int}/times", async (
                int id,
                FutebolDbContext db) =>
            {
                var exists = await db.Sorteios.AnyAsync(s => s.Id == id);
                if (!exists) return Results.NotFound();

                var times = await db.Times
                    .Where(t => t.SorteioId == id)
                    .Select(t => new TimeDto(t.Id, t.SorteioId, t.Nome))
                    .ToListAsync();

                return Results.Ok(times);
            }).AllowAnonymous();

            app.MapGet("/api/sorteios/{sorteioId:int}/times/{timeId:int}/jogadores", async (
                int sorteioId,
                int timeId,
                FutebolDbContext db) =>
            {
                var exists = await db.Sorteios.AnyAsync(s => s.Id == sorteioId);
                if (!exists) return Results.NotFound();

                var jogadores = await db.TimeJogadores
                    .Where(tj => tj.TimeId == timeId)
                    .Select(tj => new { tj.JogadorId, Jogador = tj.Jogador })
                    .ToListAsync();

                return Results.Ok(jogadores.Select(j => new
                {
                    id = j.JogadorId,
                    nome = j.Jogador.Nome,
                    posicoes = j.Jogador.Posicao?.Split(',') ?? Array.Empty<string>(),
                    destaque = j.Jogador.Destaque,
                    peso = j.Jogador.Peso
                }).ToList());
            }).AllowAnonymous();

            app.MapPost("/api/sorteios/{id:int}/finalizar", async (
                int id,
                ExecutarSorteioDto dto,
                FutebolDbContext db) =>
            {
                var sorteio = await db.Sorteios.FirstAsync(x => x.Id == id);
                sorteio.Finalizar();
                await db.SaveChangesAsync();

                return Results.Ok();
            }).RequireAuthorization();

            app.MapGet("/api/sorteios", async (FutebolDbContext db, ClaimsPrincipal user) =>
            {
                var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var sorteios = await db.Sorteios
                    .Where(s => s.OrganizadorId == userId)
                    .ToListAsync();

                return Results.Ok(sorteios);
            }).RequireAuthorization();
        }
    }
}
