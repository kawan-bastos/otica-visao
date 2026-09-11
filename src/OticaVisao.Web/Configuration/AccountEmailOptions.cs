namespace OticaVisao.Web.Configuration;

public sealed class AccountEmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Ótica Visão de Piabetá";
    public bool UseSsl { get; set; } = true;
}
