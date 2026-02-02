using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;

namespace Resturant_Menu.Services
{
    public class TelegramSettings
    {
        public string BotToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
    }

    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly TelegramSettings _settings;
        private readonly IWebHostEnvironment _env;

        public TelegramService(HttpClient httpClient, IConfiguration configuration, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _settings = configuration.GetSection("Telegram").Get<TelegramSettings>() ?? new TelegramSettings();
            _env = env;
        }

        public async Task SendBookingNotificationAsync(string message, List<string>? photoPathsOrUrls = null)
        {
            if (string.IsNullOrWhiteSpace(_settings.BotToken) || string.IsNullOrWhiteSpace(_settings.ChatId))
                return; // not configured

            var sendPhotoUrl = $"https://api.telegram.org/bot{_settings.BotToken}/sendPhoto";
            var sendMessageUrl = $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";

            // Filter and prepare valid photos
            var validPhotos = new List<string>();
            if (photoPathsOrUrls != null && photoPathsOrUrls.Any())
            {
                foreach (var photo in photoPathsOrUrls)
                {
                    if (string.IsNullOrWhiteSpace(photo)) continue;

                    if (photo.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        validPhotos.Add(photo);
                    }
                    else
                    {
                        var relative = photo.TrimStart('/', '\\');
                        var filePath = Path.Combine(_env.WebRootPath ?? string.Empty, relative);
                        if (File.Exists(filePath))
                        {
                            validPhotos.Add(filePath);
                        }
                    }
                }
            }

            // Send each photo individually with caption on the first one
            if (validPhotos.Count > 0)
            {
                for (int i = 0; i < validPhotos.Count; i++)
                {
                    try
                    {
                        var caption = i == 0 ? message : string.Empty;
                        await SendSinglePhotoAsync(validPhotos[i], caption, sendPhotoUrl);
                    }
                    catch
                    {
                        // continue to next photo even if one fails
                    }
                }
                return;
            }

            // Fallback: send text message only if no valid photos
            try
            {
                var payload = new Dictionary<string, string>
                {
                    ["chat_id"] = _settings.ChatId,
                    ["text"] = message
                };

                using var content = new FormUrlEncodedContent(payload);
                await _httpClient.PostAsync(sendMessageUrl, content);
            }
            catch
            {
                // ignore
            }
        }

        private async Task SendSinglePhotoAsync(string photoPathOrUrl, string caption, string sendPhotoUrl)
        {
            if (photoPathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // URL-based
                var payload = new Dictionary<string, string>
                {
                    ["chat_id"] = _settings.ChatId,
                    ["photo"] = photoPathOrUrl,
                    ["caption"] = caption
                };

                using var content = new FormUrlEncodedContent(payload);
                await _httpClient.PostAsync(sendPhotoUrl, content);
            }
            else
            {
                // Local file upload
                using var fs = File.OpenRead(photoPathOrUrl);
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(_settings.ChatId), "chat_id");
                content.Add(new StringContent(caption), "caption");
                var streamContent = new StreamContent(fs);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "photo", Path.GetFileName(photoPathOrUrl));

                await _httpClient.PostAsync(sendPhotoUrl, content);
            }
        }
    }
}
