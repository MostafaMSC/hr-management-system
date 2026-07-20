using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            return SendEmailAsync(to, subject, body, false, CancellationToken.None, null);
        }

        public Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default, IEnumerable<EmailAttachment>? attachments = null)
        {
            _logger.LogInformation("Dummy EmailService: Sending email to {To} with Subject {Subject}. Attachments count: {Count}",
                to, subject, attachments?.Count() ?? 0);

            // Dummy implementation for now
            return Task.CompletedTask;
        }
    }
}
