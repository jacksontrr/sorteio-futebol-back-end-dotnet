using Futebol.Api.Domain;
using Futebol.Api.Dtos;
using Futebol.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Futebol.Api.Endpoints;

public static class OrganizadoresEndpoints
{
    public static void Map(WebApplication app)
    {
        /* app.MapPost("/organizadores", async (
            CriarOrganizadorDto dto,
            FutebolDbContext db) =>
        {
            var organizador = new Organizador
            {
                UserId = dto.UserId,
                Nome = dto.Nome
            };

            db.Organizadores.Add(organizador);
            await db.SaveChangesAsync();

            return Results.Created($"/organizadores/{organizador.Id}", organizador.Id);
        });

        app.MapGet("/organizadores/{id:int}", async (
            int id,
            FutebolDbContext db) =>
        {
            var organizador = await db.Organizadores
                .Include(o => o.Jogadores)
                .FirstOrDefaultAsync(o => o.Id == id);

            return organizador is null
                ? Results.NotFound()
                : Results.Ok(organizador);
        }); */
    }
}
