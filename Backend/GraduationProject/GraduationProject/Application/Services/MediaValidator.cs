namespace GraduationProject.Application.Services
{
    using System;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using GraduationProject.Application.Services.Interfaces;

    public enum MediaType
    {
        Unknown,
        Video,
        Audio,
        Invalid
    }

    public class MediaValidator : IMediaValidator
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _videoExtensions;
        private readonly Dictionary<string, string> _audioExtensions;

        public MediaValidator(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Initialize common video extensions
            _videoExtensions = new Dictionary<string, string>
        {
            { ".mp4", "video/mp4" },
            { ".avi", "video/x-msvideo" },
            { ".mov", "video/quicktime" },
            { ".wmv", "video/x-ms-wmv" },
            { ".mkv", "video/x-matroska" },
            { ".webm", "video/webm" },
            { ".flv", "video/x-flv" },
            { ".m4v", "video/mp4" },
            { ".3gp", "video/3gpp" }
        };

            // Initialize common audio extensions
            _audioExtensions = new Dictionary<string, string>
        {
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/x-wav" },
            { ".ogg", "audio/ogg" },
            { ".flac", "audio/flac" },
            { ".aac", "audio/aac" },
            { ".m4a", "audio/mp4" },
            { ".wma", "audio/x-ms-wma" }
        };
        }

        public async Task<bool> ValidateAsync(string url)
        {
            var mediaType = await GetMediaTypeAsync(url);
            return mediaType == MediaType.Video || mediaType == MediaType.Audio;
        }

        public async Task<MediaType> GetMediaTypeAsync(string url)
        {
            // Basic URL validation first
            if (!IsValidUrlFormat(url))
                return MediaType.Invalid;

            try
            {
                // Check for known media hosting platforms
                var knownPlatformType = GetKnownPlatformMediaType(url);
                if (knownPlatformType != MediaType.Unknown)
                    return knownPlatformType;

                // For cloud storage platforms, we need to make a request
                var cloudStorageType = await GetCloudStorageMediaTypeAsync(url);
                if (cloudStorageType != MediaType.Unknown)
                    return cloudStorageType;

                // For other URLs, try to check content directly
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return MediaType.Invalid;

                return GetMediaTypeFromContentHeaders(response);
            }
            catch
            {
                return MediaType.Invalid;
            }
        }

        private bool IsValidUrlFormat(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Check if URL is valid format
            Uri? uriResult;
            bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return isValidUrl;
        }

        private MediaType GetKnownPlatformMediaType(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host.ToLower();

            // YouTube validation (supports both video and audio)
            if (host.Contains("youtube.com") || host.Contains("youtu.be"))
            {
                string youtubePattern = @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/\s]{11})";
                return Regex.IsMatch(url, youtubePattern) ? MediaType.Video : MediaType.Unknown;
            }

            // Vimeo validation (primarily video)
            if (host.Contains("vimeo.com"))
            {
                string vimeoPattern = @"vimeo\.com\/(?:channels\/(?:\w+\/)?|groups\/[^\/]+\/videos\/|)(\d+)";
                return Regex.IsMatch(url, vimeoPattern) ? MediaType.Video : MediaType.Unknown;
            }

            // Dailymotion validation (video)
            if (host.Contains("dailymotion.com") || host.Contains("dai.ly"))
            {
                string dailymotionPattern = @"(?:dailymotion\.com\/(?:video\/|embed\/video\/)|dai\.ly\/)([a-zA-Z0-9]+)";
                return Regex.IsMatch(url, dailymotionPattern) ? MediaType.Video : MediaType.Unknown;
            }

            // Facebook video validation
            if (host.Contains("facebook.com") || host.Contains("fb.watch"))
            {
                string facebookPattern = @"(?:facebook\.com\/(?:watch\/\?v=|video\.php\?v=)|fb\.watch\/)(\d+)";
                return Regex.IsMatch(url, facebookPattern) ? MediaType.Video : MediaType.Unknown;
            }

            // Twitch validation (video)
            if (host.Contains("twitch.tv"))
            {
                string twitchPattern = @"twitch\.tv\/(?:videos\/(\d+)|[^\/]+\/clip\/([^\/]+)|([^\/]+))";
                return Regex.IsMatch(url, twitchPattern) ? MediaType.Video : MediaType.Unknown;
            }

            // SoundCloud validation (audio)
            if (host.Contains("soundcloud.com"))
            {
                string soundcloudPattern = @"soundcloud\.com\/([^\/]+)\/([^\/]+)";
                return Regex.IsMatch(url, soundcloudPattern) ? MediaType.Audio : MediaType.Unknown;
            }

            // Spotify (audio)
            if (host.Contains("spotify.com") || host.Contains("open.spotify.com"))
            {
                string spotifyPattern = @"open\.spotify\.com\/(track|album|playlist|episode|show)\/([a-zA-Z0-9]+)";
                return Regex.IsMatch(url, spotifyPattern) ? MediaType.Audio : MediaType.Unknown;
            }

            // Mixcloud (audio)
            if (host.Contains("mixcloud.com"))
            {
                string mixcloudPattern = @"mixcloud\.com\/([^\/]+)\/([^\/]+)";
                return Regex.IsMatch(url, mixcloudPattern) ? MediaType.Audio : MediaType.Unknown;
            }

            // Cloudinary (check based on URL structure)
            if (host.Contains("cloudinary.com"))
            {
                if (url.Contains("/video/") || url.Contains("/video/upload/"))
                    return MediaType.Video;
                if (url.Contains("/audio/") || url.Contains("/audio/upload/"))
                    return MediaType.Audio;

                // For other Cloudinary URLs, we'll need to check content type in GetCloudStorageMediaTypeAsync
            }

            return MediaType.Unknown;
        }

        private async Task<MediaType> GetCloudStorageMediaTypeAsync(string url)
        {
            if(!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return MediaType.Invalid;
            string host = uri.Host.ToLower();
            string path = uri.PathAndQuery.ToLower();

            // --- Google Drive ---
            if (host.Contains("drive.google.com"))
            {
                // For Google Drive, we need to make a request to check the content type
                // Direct download links have format: https://drive.google.com/uc?export=download&id=FILE_ID
                // Viewing links have format: https://drive.google.com/file/d/FILE_ID/view

                string? fileId = null;

                // Extract the file ID from different possible URL formats
                if (path.Contains("/file/d/"))
                {
                    var match = Regex.Match(url, @"\/file\/d\/([^\/]+)");
                    if (match.Success)
                        fileId = match.Groups[1].Value;
                }
                else if (path.Contains("id="))
                {
                    var match = Regex.Match(url, @"[?&]id=([^&]+)");
                    if (match.Success)
                        fileId = match.Groups[1].Value;
                }

                if (!string.IsNullOrEmpty(fileId))
                {
                    // Construct a direct download URL
                    string directUrl = $"https://drive.google.com/uc?export=download&id={fileId}";

                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Head, directUrl);
                        var response = await _httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                            return GetMediaTypeFromContentHeaders(response);
                    }
                    catch
                    {
                        // Fall through to return Unknown if the request fails
                    }
                }
            }

            // --- Dropbox ---
            if (host.Contains("dropbox.com") || host.Contains("dl.dropboxusercontent.com"))
            {
                string modifiedUrl = url;

                // Convert standard sharing link to direct download if needed
                if (url.Contains("dropbox.com/s/") && !url.Contains("?dl=1"))
                {
                    modifiedUrl = url.TrimEnd('/') + "?dl=1";
                }

                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Head, modifiedUrl);
                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                        return GetMediaTypeFromContentHeaders(response);
                }
                catch
                {
                    // Fall through to Unknown if the request fails
                }
            }

            // --- Cloudinary (additional checks) ---
            if (host.Contains("cloudinary.com") || host.Contains("res.cloudinary.com"))
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Head, url);
                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                        return GetMediaTypeFromContentHeaders(response);
                }
                catch
                {
                    // Fall through to Unknown if the request fails
                }
            }

            return MediaType.Unknown;
        }

        private MediaType GetMediaTypeFromContentHeaders(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentType == null)
                return MediaType.Unknown;

            string? contentType = response.Content.Headers.ContentType.MediaType?.ToLower();
            string? url = response.RequestMessage?.RequestUri?.ToString().ToLower();

            if(string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(url))
                return MediaType.Unknown;

            // Check content type
            if (contentType.StartsWith("video/"))
                return MediaType.Video;

            if (contentType.StartsWith("audio/"))
                return MediaType.Audio;

            // Check for application/octet-stream with specific file extensions
            if (contentType == "application/octet-stream" ||
                contentType == "application/x-unknown-content-type" ||
                contentType == "application/force-download" ||
                contentType == "binary/octet-stream")
            {
                // Check URL for file extensions
                foreach (var ext in _videoExtensions.Keys)
                {
                    if (url.EndsWith(ext))
                        return MediaType.Video;
                }

                foreach (var ext in _audioExtensions.Keys)
                {
                    if (url.EndsWith(ext))
                        return MediaType.Audio;
                }

                // If we have a Content-Disposition header, check the filename
                if (response.Content.Headers.ContentDisposition != null &&
                    !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
                {
                    string fileName = response.Content.Headers.ContentDisposition.FileName.ToLower();

                    foreach (var ext in _videoExtensions.Keys)
                    {
                        if (fileName.EndsWith(ext))
                            return MediaType.Video;
                    }

                    foreach (var ext in _audioExtensions.Keys)
                    {
                        if (fileName.EndsWith(ext))
                            return MediaType.Audio;
                    }
                }
            }

            return MediaType.Unknown;
        }
    }
}
