using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace Futebol.Api.Infrastructure.Email;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public bool EnableEmailSending { get; set; } = true;
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger, IWebHostEnvironment environment)
    {
        _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task SendPasswordRecoveryEmailAsync(string email, string token, string resetLink)
    {
        try
        {
            var htmlBody = GetPasswordRecoveryEmailTemplate(resetLink, token);

            await SendEmailAsync(
                email,
                "Recuperar Senha - FutebolSort",
                htmlBody
            );

            _logger.LogInformation("✅ Email de recuperação de senha enviado para {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar email de recuperação para {Email}", email);
            if (!_environment.IsDevelopment())
                throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        try
        {
            var htmlBody = GetWelcomeEmailTemplate(name);

            await SendEmailAsync(
                email,
                "Bem-vindo ao FutebolSort",
                htmlBody
            );

            _logger.LogInformation("✅ Email de boas-vindas enviado para {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar email de boas-vindas para {Email}", email);
            if (!_environment.IsDevelopment())
                throw;
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_emailSettings.EnableEmailSending || _environment.IsDevelopment())
        {
            _logger.LogInformation("📧 [DEV MODE] Email não enviado.\n  De: {FromEmail}\n  Para: {ToEmail}\n  Assunto: {Subject}", 
                _emailSettings.FromEmail, toEmail, subject);
            await Task.CompletedTask;
            return;
        }

        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                EnableSsl = _emailSettings.UseSSL,
                Timeout = 30000,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email enviado com sucesso para {ToEmail}", toEmail);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "❌ Erro SMTP ao enviar email para {ToEmail}: {Message}", toEmail, smtpEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar email para {ToEmail}: {Message}", toEmail, ex.Message);
            throw;
        }
    }

    private string GetPasswordRecoveryEmailTemplate(string resetLink, string token)
    {
        return $@"
            <!DOCTYPE html>
            <html dir=""ltr"" lang=""pt-BR"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Recuperar Senha - FutebolSort</title>
                <style>
                    body {{
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', sans-serif;
                        line-height: 1.6;
                        color: #333;
                        background: linear-gradient(135deg, #f0fdf4 0%, #f0f9ff 100%);
                        margin: 0;
                        padding: 20px;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background: white;
                        border-radius: 12px;
                        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                    }}
                    .header {{
                        background: linear-gradient(135deg, #16a34a 0%, #0ea5e9 100%);
                        color: white;
                        padding: 30px 20px;
                        text-align: center;
                    }}
                    .header h1 {{
                        margin: 0;
                        font-size: 28px;
                        font-weight: 700;
                    }}
                    .content {{
                        padding: 30px 20px;
                    }}
                    .content p {{
                        margin: 0 0 15px 0;
                        font-size: 16px;
                    }}
                    .cta-button {{
                        display: inline-block;
                        background: linear-gradient(135deg, #16a34a 0%, #0ea5e9 100%);
                        color: white;
                        padding: 12px 30px;
                        border-radius: 6px;
                        text-decoration: none;
                        font-weight: 600;
                        margin: 20px 0;
                        text-align: center;
                    }}
                    .cta-button:hover {{
                        opacity: 0.9;
                    }}
                    .footer {{
                        background: #f9fafb;
                        padding: 20px;
                        text-align: center;
                        font-size: 12px;
                        color: #666;
                        border-top: 1px solid #e5e7eb;
                    }}
                    .warning {{
                        background: #fef3c7;
                        border-left: 4px solid #f59e0b;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .warning p {{
                        margin: 0;
                        color: #92400e;
                        font-size: 14px;
                    }}
                    .info-box {{
                        background: #f3f4f6;
                        padding: 15px;
                        border-radius: 6px;
                        margin: 20px 0;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>🏆 FutebolSort</h1>
                        <p style=""margin: 10px 0 0 0; font-size: 14px;"">Recuperação de Senha</p>
                    </div>

                    <div class=""content"">
                        <p>Olá!</p>
                        <p>Você solicitou para redefinir a senha da sua conta no FutebolSort. Clique no botão abaixo para redefinir sua senha:</p>

                        <center>
                            <a href=""{HtmlEncoder.Default.Encode(resetLink)}"" class=""cta-button"">Redefinir Senha</a>
                        </center>

                        <p style=""text-align: center; font-size: 12px; color: #666; margin-top: 20px;"">
                            Ou copie e cole este link no seu navegador:
                        </p>
                        <div class=""info-box"">
                            <code style=""word-break: break-all; color: #16a34a; font-weight: 500;"">
                                {HtmlEncoder.Default.Encode(resetLink)}
                            </code>
                        </div>

                        <div class=""warning"">
                            <p>⚠️ Este link é válido por apenas <strong>24 horas</strong>.</p>
                        </div>

                        <p>Se você não solicitou esta recuperação de senha, pode ignorar este email com segurança.</p>

                        <p style=""color: #666; font-size: 14px; margin-top: 30px;"">
                            Atenciosamente,<br>
                            <strong>Time FutebolSort</strong>
                        </p>
                    </div>

                    <div class=""footer"">
                        <p>© 2026 FutebolSort. Todos os direitos reservados.</p>
                        <p>Este é um email automático, por favor não responda.</p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }

    private string GetWelcomeEmailTemplate(string name)
    {
        return $@"
            <!DOCTYPE html>
            <html dir=""ltr"" lang=""pt-BR"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Bem-vindo ao FutebolSort</title>
                <style>
                    body {{
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', sans-serif;
                        line-height: 1.6;
                        color: #333;
                        background: linear-gradient(135deg, #f0fdf4 0%, #f0f9ff 100%);
                        margin: 0;
                        padding: 20px;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background: white;
                        border-radius: 12px;
                        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                    }}
                    .header {{
                        background: linear-gradient(135deg, #16a34a 0%, #0ea5e9 100%);
                        color: white;
                        padding: 30px 20px;
                        text-align: center;
                    }}
                    .header h1 {{
                        margin: 0;
                        font-size: 28px;
                        font-weight: 700;
                    }}
                    .content {{
                        padding: 30px 20px;
                    }}
                    .content p {{
                        margin: 0 0 15px 0;
                        font-size: 16px;
                    }}
                    .features {{
                        display: grid;
                        grid-template-columns: 1fr 1fr;
                        gap: 15px;
                        margin: 20px 0;
                    }}
                    .feature {{
                        background: #f3f4f6;
                        padding: 15px;
                        border-radius: 6px;
                        text-align: center;
                    }}
                    .feature p {{
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        background: #f9fafb;
                        padding: 20px;
                        text-align: center;
                        font-size: 12px;
                        color: #666;
                        border-top: 1px solid #e5e7eb;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>🏆 FutebolSort</h1>
                        <p style=""margin: 10px 0 0 0; font-size: 16px;"">Bem-vindo, {HtmlEncoder.Default.Encode(name)}!</p>
                    </div>

                    <div class=""content"">
                        <p>Obrigado por se registrar no FutebolSort! 🎉</p>
                        <p>Agora você tem acesso a todas as nossas funcionalidades para sorteio de times e partidas de futebol.</p>

                        <h3 style=""color: #16a34a; margin-top: 20px;"">Principais Funcionalidades:</h3>
                        <div class=""features"">
                            <div class=""feature"">
                                <p>⚡ <strong>Sorteios Rápidos</strong><br>Em menos de 2 minutos</p>
                            </div>
                            <div class=""feature"">
                                <p>📊 <strong>Estatísticas</strong><br>Acompanhe desempenhos</p>
                            </div>
                            <div class=""feature"">
                                <p>👥 <strong>Gerenciar Jogadores</strong><br>Organize seu time</p>
                            </div>
                            <div class=""feature"">
                                <p>🏆 <strong>Campeonatos</strong><br>Crie torneiros</p>
                            </div>
                        </div>

                        <p style=""color: #666; font-size: 14px; margin-top: 30px;"">
                            Qualquer dúvida, entre em contato conosco.<br>
                            <strong>Time FutebolSort</strong>
                        </p>
                    </div>

                    <div class=""footer"">
                        <p>© 2026 FutebolSort. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }
}
