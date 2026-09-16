using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MainServer.DTOs.Common;
using MainServer.DTOs.Payments;
using MainServer.Exceptions;

namespace MainServer.Integrations.Payments;

public class PaymentHttpGateway(HttpClient httpClient) : IPaymentHttpGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<PaymentResponse> GetPaymentByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/payments/{id}", cancellationToken);
        return await ReadRequiredAsync(response, $"Payment with id {id} was not found.", cancellationToken);
    }

    public async Task<PaginationResponse<PaymentResponse>> GetPaymentsByOrderIdAsync(
        int orderId,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.GetNormalizedPage();
        var pageSize = request.GetNormalizedPageSize();
        var url = $"api/payments/order/{orderId}?page={page}&pageSize={pageSize}";

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PaginationResponse<PaymentResponse>>>(
            JsonOptions,
            cancellationToken);

        return payload?.Data
            ?? new PaginationResponse<PaymentResponse>([], page, pageSize, 0, 0);
    }

    private static async Task<PaymentResponse> ReadRequiredAsync(
        HttpResponseMessage response,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(notFoundMessage);
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentResponse>>(
            JsonOptions,
            cancellationToken);

        return payload?.Data ?? throw new NotFoundException(notFoundMessage);
    }
}
