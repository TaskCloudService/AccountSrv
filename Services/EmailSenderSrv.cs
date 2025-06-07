using Azure.Communication.Email;
using Azure;
using Presentation.Interfaces;

namespace Presentation.Services
{

    public sealed class AzureEmailSender : IEmailSender
    {
        private readonly EmailClient _client;
        private readonly string _from;

        public AzureEmailSender(IConfiguration cfg)
        {
            _client = new EmailClient(cfg["Email:ConnectionString"]!);
            _from = cfg["Email:From"]!;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        {
            var msg = new EmailMessage(
                _from,
                new EmailRecipients(new[] { new EmailAddress(to) }),
                new EmailContent(subject) { Html = htmlBody });

            await _client.SendAsync(WaitUntil.Completed, msg, ct);
        }
    }
}
