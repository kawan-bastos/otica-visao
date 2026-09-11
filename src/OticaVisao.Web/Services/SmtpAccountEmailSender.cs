using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using OticaVisao.Web.Configuration;

namespace OticaVisao.Web.Services;

public sealed class SmtpAccountEmailSender(IOptions<AccountEmailOptions> options) : IAccountEmailSender
{
    private readonly AccountEmailOptions settings = options.Value;

    public async Task SendPasswordResetCodeAsync(
        string destinationAddress,
        string recipientName,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.Host)
            || string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            throw new InvalidOperationException(
                "O serviço de e-mail não foi configurado. Preencha a seção Email da configuração.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = "Código para redefinir sua senha",
            IsBodyHtml = true,
            Body = $"""
                <div style="font-family:Arial,sans-serif;color:#082b61;line-height:1.6">
                  <p>Olá, {HtmlEncoder.Default.Encode(recipientName)}.</p>
                  <p>Use o código abaixo para criar uma nova senha na Ótica Visão:</p>
                  <p style="font-size:30px;font-weight:800;letter-spacing:8px;margin:24px 0">{code}</p>
                  <p>O código é válido por 10 minutos. Se você não pediu a alteração, ignore este e-mail.</p>
                </div>
                """
        };
        message.To.Add(destinationAddress);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.UseSsl,
            Credentials = string.IsNullOrWhiteSpace(settings.UserName)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(settings.UserName, settings.Password)
        };
        await client.SendMailAsync(message, cancellationToken);
    }
}
