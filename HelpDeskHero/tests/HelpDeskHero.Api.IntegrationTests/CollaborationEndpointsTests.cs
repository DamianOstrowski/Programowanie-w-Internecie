using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Shared.Contracts.Notifications;
using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskHero.Api.IntegrationTests;

public sealed class CollaborationEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CollaborationEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Comments_ShouldBeCreatable_ForExistingTicket()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        var createTicket = await client.PostAsJsonAsync("/api/tickets", new CreateTicketDto
        {
            Title = "Comment Ticket",
            Description = "Ticket for comments",
            Priority = "Medium"
        });
        createTicket.EnsureSuccessStatusCode();
        var ticket = await createTicket.Content.ReadFromJsonAsync<TicketDto>();
        ticket.Should().NotBeNull();

        var createComment = await client.PostAsJsonAsync(
            $"/api/tickets/{ticket!.Id}/comments",
            new CreateTicketCommentDto { Body = "Pierwszy komentarz", IsInternal = false });
        createComment.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetAsync($"/api/tickets/{ticket.Id}/comments");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await list.Content.ReadFromJsonAsync<List<TicketCommentDto>>();
        comments.Should().Contain(x => x.Body == "Pierwszy komentarz");
    }

    [Fact]
    public async Task Attachments_ShouldUpload_AndList()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        var createTicket = await client.PostAsJsonAsync("/api/tickets", new CreateTicketDto
        {
            Title = "Attachment Ticket",
            Description = "Ticket for attachments",
            Priority = "Low"
        });
        createTicket.EnsureSuccessStatusCode();
        var ticket = await createTicket.Content.ReadFromJsonAsync<TicketDto>();
        ticket.Should().NotBeNull();

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("hello attachment"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "note.txt");

        var upload = await client.PostAsync($"/api/tickets/{ticket!.Id}/attachments", content);
        upload.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetAsync($"/api/tickets/{ticket.Id}/attachments");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var attachments = await list.Content.ReadFromJsonAsync<List<TicketAttachmentDto>>();
        attachments.Should().Contain(x => x.OriginalFileName == "note.txt");
    }

    [Fact]
    public async Task Notifications_Mine_ShouldReturnCurrentUserItems()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.Single(x => x.UserName == "agent");
            db.UserNotifications.Add(new UserNotification
            {
                UserId = user.Id,
                Subject = "Test",
                Body = "Body",
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/notifications/mine");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<UserNotificationDto>>();
        items.Should().Contain(x => x.Subject == "Test");
    }
}
