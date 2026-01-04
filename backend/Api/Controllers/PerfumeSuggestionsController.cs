using Core.DTOs;
using Core.Enums;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/perfume-suggestions")]
    [Authorize]
    public sealed class PerfumeSuggestionsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PerfumeSuggestionsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PerfumeSuggestionRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Brand) || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest();

            var webhookUrl = _configuration["Discord:WebhookUrl"];
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return StatusCode(500);

            var fields = new List<DiscordEmbedField>
            {
                new() { Name = "Brand", Value = dto.Brand, Inline = true },
                new() { Name = "Name", Value = dto.Name, Inline = true }
            };

            var safeGroups = SanitizeTokens(dto.Groups).ToList();

            if (safeGroups.Count > 0)
            {
                fields.Add(new DiscordEmbedField
                {
                    Name = "Olfactory Groups",
                    Value = string.Join(", ", safeGroups),
                    Inline = false
                });
            }

            foreach (var ng in dto.NoteGroups)
            {
                var safeNotes = SanitizeTokens(ng.Notes).ToList();
                if (safeNotes.Count == 0) continue;

                fields.Add(new DiscordEmbedField
                {
                    Name = ng.Type switch
                    {
                        NoteTypeEnum.Top => "Top Notes",
                        NoteTypeEnum.Middle => "Heart Notes",
                        NoteTypeEnum.Base => "Base Notes",
                        _ => "Notes"
                    },
                    Value = string.Join(", ", safeNotes),
                    Inline = false
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.Comment))
            {
                fields.Add(new DiscordEmbedField
                {
                    Name = "Comment",
                    Value = dto.Comment,
                    Inline = false
                });
            }

            var embed = new DiscordEmbed
            {
                Title = "New Perfume Suggestion",
                Color = 0xD3A54A,
                Fields = fields,
                Image = string.IsNullOrWhiteSpace(dto.ImageBase64)
                    ? null
                    : new DiscordEmbedImage { Url = "attachment://image.jpg" },
                Footer = new DiscordEmbedFooter
                {
                    Text = $"UserId: {User.GetUserId()}"
                },
                Timestamp = DateTimeOffset.UtcNow.ToString("O")
            };

            var payload = new DiscordWebhookPayload
            {
                Username = "FragranceLog",
                Embeds = [embed]
            };

            using var content = new MultipartFormDataContent();

            var payloadJson = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }
            );

            content.Add(
                new StringContent(payloadJson, Encoding.UTF8),
                "payload_json"
            );

            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                var base64 = dto.ImageBase64;
                var comma = base64.IndexOf(',');
                if (comma >= 0)
                    base64 = base64[(comma + 1)..];

                var bytes = Convert.FromBase64String(base64);

                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue("image/jpeg");

                content.Add(fileContent, "file", "image.jpg");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(webhookUrl, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode(502);

            return Accepted();
        }

        private static bool IsValidToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value.Length < 2) return false;
            if (!char.IsLetterOrDigit(value[0])) return false;
            return true;
        }

        private static IEnumerable<string> SanitizeTokens(IEnumerable<string> values)
        {
            return values
                .Where(IsValidToken)
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(20);
        }
    }

}
