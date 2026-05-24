using System.Net.Http.Json;

namespace HelpDeskHero.Api.Infrastructure.Notifications;

public sealed class WebhookNotificationSender : INotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookNotificationSender> _logger;

    public WebhookNotificationSender(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WebhookNotificationSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Webhook;

    public async Task SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        var url = _configuration["Notifications:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogInformation("Webhook URL not configured, skipping notification.");
            return;
        }

        using var response = await _httpClient.PostAsJsonAsync(url, new
        {
            subject = message.Subject,
            body = message.Body,
            userId = message.UserId
        }, ct);

        response.EnsureSuccessStatusCode();
    }
}
