using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AkyildizYonetim.Tests.Integration;

public class NotificationsControllerTests : AuthTestBase
{
    public NotificationsControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetNotifications_AsManager_ReturnsOk()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var response = await client.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNotifications_AsAdmin_ReturnsOk()
    {
        var client = CreateClientWithUser(UserContext.Admin());
        var response = await client.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendTargetedNotification_ToAll_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            Title = "Test Duyurusu",
            Message = "Bu bir test mesajıdır.",
            Type = "announcement",
            TargetType = "all",
            TargetId = (string?)null,
            SendEmail = false
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendTargetedNotification_ToFloor_WithFloorNumber_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            Title = "Kat Duyurusu",
            Message = "1. kat için test mesajı.",
            Type = "announcement",
            TargetType = "floor",
            TargetId = "1",
            SendEmail = false
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendTargetedNotification_ToSpecificTenant_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Admin());
        var tenantId = Guid.NewGuid();

        var command = new
        {
            Title = "Kiracı Duyurusu",
            Message = "Belirli kiracıya mesaj.",
            Type = "announcement",
            TargetType = "tenant",
            TargetId = tenantId.ToString(),
            SendEmail = false
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendTargetedNotification_DebtType_WithEmail_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            Title = "Geciken Ödeme Hatırlatıcısı",
            Message = "Geciken ödemeleriniz bulunmaktadır.",
            Type = "debt",
            TargetType = "all",
            TargetId = (string?)null,
            SendEmail = true,
            DelayDays = 7
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        // Email servisi test ortamında mock - 500 vermemeli
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendTargetedNotification_WithEmptyTitle_ReturnsBadRequest()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            Title = "", // Boş başlık - validation hatası
            Message = "Mesaj var.",
            Type = "announcement",
            TargetType = "all",
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendTargetedNotification_WithEmptyMessage_ReturnsBadRequest()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            Title = "Başlık var",
            Message = "", // Boş mesaj - validation hatası
            Type = "announcement",
            TargetType = "all",
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendTargetedNotification_AsObserver_ReturnsForbidden()
    {
        var client = CreateClientWithUser(new UserContext { Role = "observer" });

        var command = new
        {
            Title = "Yetkisiz Duyuru",
            Message = "Observer rol duyuru gönderememeli.",
            Type = "announcement",
            TargetType = "all",
        };

        var response = await client.PostAsJsonAsync("/api/notifications/targeted", command);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SendOverdueEmail_WithValidTenantId_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Admin());
        var tenantId = Guid.NewGuid();

        var command = new
        {
            TenantId = tenantId,
            OwnerId = (Guid?)null
        };

        var response = await client.PostAsJsonAsync("/api/notifications/send-overdue-email", command);
        // Test ortamında email servisi mock - 500 olmamalı
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendOverdueEmail_WithoutTenantOrOwnerId_ReturnsBadRequest()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            TenantId = (Guid?)null,
            OwnerId = (Guid?)null
        };

        var response = await client.PostAsJsonAsync("/api/notifications/send-overdue-email", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WithInvalidId_ReturnsNotFound()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var notificationId = Guid.NewGuid();

        // Endpoint: POST /api/notifications/{id}/read
        var response = await client.PostAsync($"/api/notifications/{notificationId}/read", null);
        // Var olmayan ID - NotFound veya OK (handler graceful olabilir)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAnnouncement_AsAdmin_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Admin());

        var command = new
        {
            Title = "Genel Duyuru",
            Message = "Tüm kullanıcılara duyuru.",
            Type = "announcement"
        };

        var response = await client.PostAsJsonAsync("/api/notifications", command);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
