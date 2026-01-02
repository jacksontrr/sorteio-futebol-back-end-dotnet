using Futebol.Api.Domain;
using Futebol.Api.Dtos;
using Futebol.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Futebol.Api.Endpoints;

public static class UsersEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/user", async (ClaimsPrincipal user, FutebolDbContext db) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var organizador = await db.Organizadores.Include(o => o.User)
                .FirstOrDefaultAsync(u => u.User.Id == Guid.Parse(userIdClaim.Value));
            if (organizador == null)
            {
                return Results.NotFound();
            }
            var users = new OrganizadorResponseDto(
                organizador.Id,
                organizador.Nome,
                organizador.Codigo,
                organizador.Ativo
            );
            return Results.Ok(users);
        });
    }

    // Hash simples (suficiente para MVP)
    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
