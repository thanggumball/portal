using StudentPortal.Common.DTOs.Mail;
using StudentPortal.Service.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace StudentPortal.Service.Implementations;

public class MailServiceClient : IMailServiceClient
{
    private readonly HttpClient _httpClient;

    public MailServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateAccountAsync(
        CreateMailAccountRequest request,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/mail-account",
            request,
            ct);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "Mail account already exists.");
        }

        var message = await response.Content.ReadAsStringAsync(ct);

        throw new HttpRequestException(
            $"MailService account creation failed. " +
            $"Status: {(int)response.StatusCode}. " +
            $"Response: {message}");
    }
}
