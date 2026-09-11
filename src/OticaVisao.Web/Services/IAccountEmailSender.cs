namespace OticaVisao.Web.Services;

public interface IAccountEmailSender
{
    Task SendPasswordResetCodeAsync(
        string destinationAddress,
        string recipientName,
        string code,
        CancellationToken cancellationToken = default);
}
