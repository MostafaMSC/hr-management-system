public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;   // ðŸ”´ Ù…Ù‡Ù…
    public string FromName { get; set; } = string.Empty;

    public bool EnableSsl { get; set; }     // ðŸ”´ Ù…Ù‡Ù…
}
