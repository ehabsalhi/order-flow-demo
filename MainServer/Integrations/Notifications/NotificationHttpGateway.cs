using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MainServer.DTOs.Common;
using MainServer.DTOs.Notifications;

namespace MainServer.Integrations.Notifications;

public class NotificationHttpGateway(HttpClient httpClient) : INotificationHttpGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<PaginationResponse<NotificationResponse>> GetAsync(
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.GetNormalizedPage();
        var pageSize = request.GetNormalizedPageSize();
        var query = $"page={page}&pageSize={pageSize}";

        if (request.OrderId.HasValue)
        {
            query += $"&orderId={request.OrderId.Value}";
        }

        if (request.Status.HasValue)
        {
            query += $"&status={request.Status.Value}";
        }

        var response = await httpClient.GetAsync($"api/notifications?{query}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<ApiResponse<PaginationResponse<NotificationResponse>>>(
                JsonOptions,
                cancellationToken);

        return payload?.Data
            ?? new PaginationResponse<NotificationResponse>([], page, pageSize, 0, 0);
    }
}
