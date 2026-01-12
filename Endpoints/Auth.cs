using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Futebol.Api.Dtos;
using Futebol.Api.Utils;
using Futebol.Api.Infrastructure;
using Futebol.Api.Infrastructure.Email;
using Futebol.Api.Domain;
using Futebol.Api.Configuration;

namespace Futebol.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void Map(this WebApplication app)
        {
            app.MapPost("/api/auth/register/organizador", async (RegisterDto dto, FutebolDbContext db) =>
            {
                if (await db.Users.AnyAsync(o => o.Email == dto.Email))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Email já cadastrado." } });

                var user = new User
                {
                    Nome = dto.Nome,
                    Email = dto.Email,
                    PasswordHash = Security.HashSenha(dto.Password),
                    ContaGoogle = false,
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                string code;
                do
                {
                    code = Guid.NewGuid().ToString("N").Substring(0, 8);
                } while (await db.Organizadores.AnyAsync(o => o.Codigo == code));

                var organizador = new Organizador
                {
                    Nome = dto.Nome,
                    Codigo = code,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                };

                db.Organizadores.Add(organizador);
                await db.SaveChangesAsync();

                return Results.Ok(new ApiResponse<OrganizadorResponseDto> { Data = new OrganizadorResponseDto(organizador.Id, organizador.Nome, organizador.Codigo, organizador.Ativo) });
            });

            app.MapPost("/api/auth/register/jogador", async (RegisterJogadorDto dto, FutebolDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(dto.Codigo))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Código do organizador é obrigatório." } });

                var organizador = await db.Organizadores.FirstOrDefaultAsync(o => o.Codigo == dto.Codigo);
                if (organizador == null)
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Código de organizador inválido." } });

                var jogador = new Jogador
                {
                    Nome = dto.Nome,
                    Observacoes = dto.Observacoes,
                    Organizador = organizador,
                    Ativo = true,
                    Destaque = dto.Destaque,
                    Peso = dto.Peso,
                    Posicao = dto.Posicoes != null ? string.Join(",", dto.Posicoes) : null
                };

                db.Jogadores.Add(jogador);
                await db.SaveChangesAsync();

                var jogadorFull = await db.Jogadores
                    .Include(j => j.Organizador)
                    .FirstOrDefaultAsync(j => j.Id == jogador.Id);

                if (jogadorFull == null)
                    return Results.NotFound(new ErrorResponse { Error = new ApiError { Message = "Jogador não encontrado após criação." } });

                var jogadorDto = new JogadorResponseDto(jogadorFull.Id, jogadorFull.Nome, jogadorFull.Posicao, jogadorFull.Destaque, jogadorFull.Peso, jogadorFull.Ativo, jogadorFull.Observacoes, jogadorFull.CreatedAt);

                return Results.Created($"/api/auth/jogadores/{jogador.Id}", new ApiResponse<JogadorResponseDto> { Data = jogadorDto });
            });

            app.MapPost("/api/auth/login", async (LoginDto dto, FutebolDbContext db, IConfiguration config) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(o => o.Email == dto.Email);
                if (user == null)
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Credenciais inválidas." } });

                if (user.PasswordHash != Security.HashSenha(dto.Password))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Credenciais inválidas." } });

                var expiresInDays = dto.Remember ? 7 : 1;
                var token = Security.GenerateJwt(user.Id, "organizador", config, TimeSpan.FromDays(expiresInDays));

                return Results.Ok(new ApiResponse<object> { Data = new { token } });
            });

            app.MapPost("/api/auth/google", async (GoogleLoginDto dto, FutebolDbContext db, IConfiguration config, HttpContext httpContext) =>
            {
                var clientId = httpContext.Request.Headers["X-Google-Client-Id"].FirstOrDefault() ?? string.Empty;
                if (string.IsNullOrEmpty(clientId))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Client ID do Google é obrigatório." } });

                var payload = await Security.VerifyGoogleToken(dto.Token, clientId);
                if (payload == null)
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Token do Google inválido." } });

                var email = payload.Email;
                var user = await db.Users.FirstOrDefaultAsync(o => o.Email == email);
                if (user == null)
                {
                    user = new User
                    {
                        Nome = payload.Name,
                        Email = email,
                        PasswordHash = Security.HashSenha(Guid.NewGuid().ToString()),
                        ContaGoogle = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();

                    string code;
                    do
                    {
                        code = Guid.NewGuid().ToString("N").Substring(0, 8);
                    } while (await db.Organizadores.AnyAsync(o => o.Codigo == code));

                    var organizador = new Organizador
                    {
                        Nome = payload.Name,
                        UserId = user.Id,
                        Codigo = code,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Organizadores.Add(organizador);
                    await db.SaveChangesAsync();
                }

                var token = Security.GenerateJwt(user.Id, "organizador", config, TimeSpan.FromDays(1));
                return Results.Ok(new ApiResponse<object> { Data = new { token } });
            });

            app.MapPost("/api/auth/refresh", (RefreshTokenDto dto, IConfiguration config) =>
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(dto.Token);

                    // Verifica se ainda é válido (não expirou)
                    if (token.ValidTo < DateTime.UtcNow)
                        return Results.Unauthorized();

                    var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                        return Results.Unauthorized();

                    var newToken = Security.GenerateJwt(userId, "organizador", config, TimeSpan.FromDays(1));
                    return Results.Ok(new ApiResponse<object> { Data = new { token = newToken } });
                }
                catch
                {
                    return Results.Unauthorized();
                }
            });

            app.MapGet("/api/auth/profile", async (ClaimsPrincipal user, FutebolDbContext db) =>
            {
                var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var userEntity = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (userEntity == null)
                    return Results.NotFound();

                var organizador = await db.Organizadores.FirstOrDefaultAsync(o => o.UserId == userId);
                var codigo = organizador?.Codigo ?? string.Empty;

                var profile = new UserProfileDto(userEntity.Nome, userEntity.Email, userEntity.ContaGoogle, codigo);
                return Results.Ok(new ApiResponse<UserProfileDto> { Data = profile });
            }).RequireAuthorization();

            app.MapPost("/api/auth/change-password", async (ChangePasswordDto dto, ClaimsPrincipal user, FutebolDbContext db) =>
            {
                var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var userEntity = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (userEntity == null)
                    return Results.NotFound();

                // Se não é conta Google, verifica a senha atual
                if (!userEntity.ContaGoogle)
                {
                    if (string.IsNullOrEmpty(dto.SenhaAtual))
                        return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Senha atual é obrigatória." } });

                    if (userEntity.PasswordHash != Security.HashSenha(dto.SenhaAtual))
                        return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Senha atual inválida." } });
                }

                if (string.IsNullOrEmpty(dto.NovaSenha))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Nova senha é obrigatória." } });

                userEntity.PasswordHash = Security.HashSenha(dto.NovaSenha);
                // Marca como não mais conta Google pura depois que altera a senha
                userEntity.ContaGoogle = false;
                db.Users.Update(userEntity);
                await db.SaveChangesAsync();

                return Results.Ok(new ApiResponse<object> { Data = new { message = "Senha alterada com sucesso." } });
            }).RequireAuthorization();

            app.MapPost("/api/auth/update-name", async (UpdateNameDto dto, ClaimsPrincipal user, FutebolDbContext db) =>
            {
                var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var userEntity = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (userEntity == null)
                    return Results.NotFound();

                if (string.IsNullOrEmpty(dto.Nome))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Nome é obrigatório." } });

                userEntity.Nome = dto.Nome;
                db.Users.Update(userEntity);

                // Atualizar nome do organizador também
                var organizador = await db.Organizadores.FirstOrDefaultAsync(o => o.UserId == userId);
                if (organizador != null)
                {
                    organizador.Nome = dto.Nome;
                    db.Organizadores.Update(organizador);
                }

                await db.SaveChangesAsync();

                return Results.Ok(new ApiResponse<object> { Data = new { message = "Nome atualizado com sucesso." } });
            }).RequireAuthorization();

            app.MapPost("/api/auth/update-codigo", async (UpdateCodigoDto dto, ClaimsPrincipal user, FutebolDbContext db) =>
            {
                var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                if (string.IsNullOrEmpty(dto.Codigo))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Código é obrigatório." } });

                var organizador = await db.Organizadores.FirstOrDefaultAsync(o => o.UserId == userId);
                if (organizador == null)
                    return Results.NotFound();

                // Verificar se o código já existe para outro organizador
                if (await db.Organizadores.AnyAsync(o => o.Codigo == dto.Codigo && o.Id != organizador.Id))
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Este código já está em uso." } });

                organizador.Codigo = dto.Codigo;
                db.Organizadores.Update(organizador);
                await db.SaveChangesAsync();

                return Results.Ok(new ApiResponse<object> { Data = new { message = "Código atualizado com sucesso." } });
            }).RequireAuthorization();

            // Recuperação de Senha
            app.MapPost("/api/auth/recuperar-senha", async (RecuperarSenhaDto dto, FutebolDbContext db, IEmailService emailService, AppSettings appSettings) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return Results.Ok(new { message = "Se o e-mail existir, você receberá um link de recuperação." });

                // Gerar token de recuperação (válido por 24 horas)
                var resetToken = Guid.NewGuid().ToString();
                user.ResetPasswordToken = resetToken;
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24);

                db.Users.Update(user);
                await db.SaveChangesAsync();

                try
                {
                    // Construir URL de reset usando a URL base do frontend configurada
                    var resetLink = $"{appSettings.FrontendUrl}/redefinir-senha?token={resetToken}";

                    // Enviar e-mail
                    await emailService.SendPasswordRecoveryEmailAsync(user.Email, resetToken, resetLink);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                    // Mesmo com erro no email, retornamos sucesso para não expor informações
                }

                return Results.Ok(new { message = "E-mail de recuperação enviado com sucesso." });
            }).AllowAnonymous();

            // Redefinição de Senha
            app.MapPost("/api/auth/redefinir-senha", async (RedefinirSenhaDto dto, FutebolDbContext db) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == dto.Token);
                if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                    return Results.BadRequest(new ErrorResponse { Error = new ApiError { Message = "Token inválido ou expirado." } });

                user.PasswordHash = Security.HashSenha(dto.NovaSenha);
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;

                db.Users.Update(user);
                await db.SaveChangesAsync();

                return Results.Ok(new { message = "Senha redefinida com sucesso. Faça login novamente." });
            }).AllowAnonymous();
        }
    }
}