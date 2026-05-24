using System.Net;
using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Common;
using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.Api.IntegrationTests;

public sealed class TicketsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTickets_WithoutAuth_ShouldReturnUnauthorized()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/tickets");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTickets_WithAuth_ShouldReturnSuccess()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        var response = await client.GetAsync("/api/tickets");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResultDto<TicketDto>>();
        body.Should().NotBeNull();
        body!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTicket_ShouldReturnCreated()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        var dto = new CreateTicketDto
        {
            Title = "Integration test ticket",
            Description = "Created by integration test",
            Priority = "High"
        };

        var response = await client.PostAsJsonAsync("/api/tickets", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<TicketDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be(dto.Title);
        created.AssignedToUserId.Should().NotBeNullOrWhiteSpace();
        created.DueFirstResponseAtUtc.Should().NotBeNull();
        created.DueResolveAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task SoftDelete_And_Restore_ShouldWork()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        var create = await client.PostAsJsonAsync(
            "/api/tickets",
            new CreateTicketDto { Title = "Del", Description = "Desc long enough", Priority = "Low" });
        create.EnsureSuccessStatusCode();
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();
        ticket.Should().NotBeNull();

        var del = await client.DeleteAsync($"/api/tickets/{ticket!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await client.GetAsync("/api/tickets/deleted");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var deleted = await list.Content.ReadFromJsonAsync<List<TicketDto>>();
        deleted.Should().Contain(x => x.Id == ticket.Id);

        var restore = await client.PostAsync($"/api/tickets/{ticket.Id}/restore", null);
        restore.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExportCsv_ShouldReturnCsv()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("agent", "Agent1234");
        var response = await client.GetAsync("/api/tickets/export");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }
}
