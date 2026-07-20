namespace HR.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default, IEnumerable<HR.Application.Common.Models.EmailAttachment>? attachments = null);
}
