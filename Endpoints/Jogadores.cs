using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Futebol.Api.Domain;
using Futebol.Api.Dtos;
using Futebol.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Futebol.Api.Endpoints;

public static class JogadoresEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/jogadores", async (RegisterJogadorDto dto, ClaimsPrincipal user, FutebolDbContext db) =>
        {
            var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var organizadorId = await db.Organizadores
                .Where(o => o.UserId == userId && o.Ativo)
                .Select(o => (int?)o.Id)
                .FirstOrDefaultAsync();

            if (organizadorId is null)
                return Results.Unauthorized();

            var jogador = new Jogador
            {
                OrganizadorId = organizadorId.Value,
                Nome = dto.Nome,
                Posicao = dto.Posicoes != null ? string.Join(",", dto.Posicoes) : null,
                Destaque = dto.Destaque,
                Peso = dto.Peso,
                Observacoes = dto.Observacoes,
            };

            db.Jogadores.Add(jogador);
            await db.SaveChangesAsync();

            var jogadorFull = await db.Jogadores
                    .Include(j => j.Organizador)
                    .FirstOrDefaultAsync(j => j.Id == jogador.Id);

            var jogadorDto = new JogadorResponseDto(jogadorFull.Id, jogadorFull.Nome, jogadorFull.Posicao, jogadorFull.Destaque, jogadorFull.Peso, jogadorFull.Ativo, jogadorFull.Observacoes, jogadorFull.CreatedAt);

            return Results.Created($"/api/jogadores/{jogador.Id}", new ApiResponse<JogadorResponseDto> { Data = jogadorDto });
        });

        app.MapGet("/api/jogadores", async ([FromQuery] string? q, [FromQuery] bool? ativo, ClaimsPrincipal user, FutebolDbContext db) =>
        {
            var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var organizadorId = await db.Organizadores
                .Where(o => o.UserId == userId && o.Ativo)
                .Select(o => (int?)o.Id)
                .FirstOrDefaultAsync();

            if (organizadorId is null)
                return Results.Unauthorized();

            var jogadoresQuery = db.Jogadores
                .AsNoTracking()
                .Where(j => j.OrganizadorId == organizadorId.Value);

            if (ativo.HasValue)
            {
                jogadoresQuery = jogadoresQuery.Where(j => j.Ativo == ativo.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                jogadoresQuery = jogadoresQuery.Where(j => EF.Functions.Like(j.Nome, $"%{q}%"));
            }

            var jogadores = await jogadoresQuery
                .Select(j => new JogadorListItemDto(
                    j.Id,
                    j.Nome,
                    j.Posicao != null
                        ? j.Posicao.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                        : new List<string>(),
                    j.Observacoes ?? string.Empty,
                    j.Destaque,
                    j.Peso,
                    j.Ativo,
                    j.CreatedAt))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<JogadorListItemDto>> { Data = jogadores });
        }).RequireAuthorization();

        app.MapPut("/api/jogadores/{id:int}", async (
            int id,
            AtualizarJogadorDto dto,
            ClaimsPrincipal user,
            FutebolDbContext db) =>
        {
            var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var organizadorId = await db.Organizadores
                .Where(o => o.UserId == userId && o.Ativo)
                .Select(o => (int?)o.Id)
                .FirstOrDefaultAsync();

            if (organizadorId is null)
                return Results.Unauthorized();

            var jogador = await db.Jogadores.FirstOrDefaultAsync(j => j.Id == id && j.OrganizadorId == organizadorId.Value);
            if (jogador is null)
                return Results.NotFound();

            jogador.Nome = dto.Nome;
            jogador.Posicao = dto.Posicoes != null ? string.Join(",", dto.Posicoes) : jogador.Posicao;
            jogador.Observacoes = dto.Observacoes;
            jogador.Destaque = dto.Destaque;
            jogador.Peso = dto.Peso;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object> { Data = new { id = jogador.Id } });
        }).RequireAuthorization();

        app.MapPut("/api/jogadores/{id:int}/status", async (
            int id,
            AtualizarStatusJogadorDto dto,
            ClaimsPrincipal user,
            FutebolDbContext db) =>
        {
            var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var organizadorId = await db.Organizadores
                .Where(o => o.UserId == userId && o.Ativo)
                .Select(o => (int?)o.Id)
                .FirstOrDefaultAsync();

            if (organizadorId is null)
                return Results.Unauthorized();

            var jogador = await db.Jogadores.FirstOrDefaultAsync(j => j.Id == id && j.OrganizadorId == organizadorId.Value);
            if (jogador is null)
                return Results.NotFound();

            jogador.Ativo = dto.Ativo;
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object> { Data = new { id = jogador.Id, ativo = jogador.Ativo } });
        }).RequireAuthorization();
    }
}
