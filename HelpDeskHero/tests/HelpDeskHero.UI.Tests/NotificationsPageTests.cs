using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Bunit;
using HelpDeskHero.Shared.Contracts.Notifications;
using HelpDeskHero.UI.Pages.Notifications;
using HelpDeskHero.UI.Services.Api;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskHero.UI.Tests;

public sealed class NotificationsPageTests : TestContext
{
    [Fact]
    public void NotificationsPage_ShouldRenderNotificationSubject()
    {
        var handler = new FakeNotificationHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/")
        };

        Services.AddSingleton(new NotificationApiClient(httpClient));

        var cut = RenderComponent<NotificationsPage>();

        cut.Markup.Should().Contain("System alert");
    }

    private sealed class FakeNotificationHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/notifications/mine", StringComparison.OrdinalIgnoreCase) == true)
            {
                var payload = JsonSerializer.Serialize(new List<UserNotificationDto>
                {
                    new()
                    {
                        Id = 1,
                        Subject = "System alert",
                        Body = "Sample notification",
                        IsRead = false,
                        CreatedAtUtc = DateTime.UtcNow
                    }
                });

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
