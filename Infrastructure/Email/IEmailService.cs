namespace Futebol.Api.Infrastructure.Email;

public interface IEmailService
{
    Task SendPasswordRecoveryEmailAsync(string email, string token, string resetLink);
    Task SendWelcomeEmailAsync(string email, string name);
}
