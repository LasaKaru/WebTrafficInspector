using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced service for replaying and modifying HTTP requests
    /// </summary>
    public class RequestReplayerService
    {
        private readonly HttpClient _httpClient;

        public RequestReplayerService()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Replay a traffic entry with optional modifications
        /// </summary>
        public async Task<ReplayResult> ReplayRequest(TrafficEntry entry, ReplayOptions options = null)
        {
            options = options ?? new ReplayOptions();

            try
            {
                var request = BuildHttpRequestMessage(entry, options);
                var startTime = DateTime.Now;

                var response = await _httpClient.SendAsync(request);

                var endTime = DateTime.Now;
                var responseBody = await response.Content.ReadAsStringAsync();

                return new ReplayResult
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    ResponseHeaders = response.Headers.ToString(),
                    ResponseBody = responseBody,
                    Duration = endTime - startTime,
                    Timestamp = startTime,
                    OriginalEntry = entry
                };
            }
            catch (Exception ex)
            {
                return new ReplayResult
                {
                    Success = false,
                    Error = ex.Message,
                    OriginalEntry = entry
                };
            }
        }

        /// <summary>
        /// Replay request multiple times for testing
        /// </summary>
        public async Task<List<ReplayResult>> BatchReplay(TrafficEntry entry, int count, ReplayOptions options = null)
        {
            var results = new List<ReplayResult>();

            for (int i = 0; i < count; i++)
            {
                var result = await ReplayRequest(entry, options);
                result.ReplayNumber = i + 1;
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Replay with different payloads (for fuzzing)
        /// </summary>
        public async Task<List<ReplayResult>> FuzzReplay(TrafficEntry entry, List<string> payloads, string targetParameter)
        {
            var results = new List<ReplayResult>();

            foreach (var payload in payloads)
            {
                var options = new ReplayOptions();
                options.ParameterReplacements[targetParameter] = payload;

                var result = await ReplayRequest(entry, options);
                result.FuzzPayload = payload;
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Compare original vs replayed response
        /// </summary>
        public ComparisonResult CompareResponses(TrafficEntry original, ReplayResult replayed)
        {
            return new ComparisonResult
            {
                OriginalStatus = original.Status,
                ReplayedStatus = replayed.StatusCode,
                StatusChanged = original.Status != replayed.StatusCode,
                OriginalLength = original.Length,
                ReplayedLength = replayed.ResponseBody?.Length ?? 0,
                LengthDifference = Math.Abs(original.Length - (replayed.ResponseBody?.Length ?? 0)),
                TimeDifference = replayed.Duration
            };
        }

        private HttpRequestMessage BuildHttpRequestMessage(TrafficEntry entry, ReplayOptions options)
        {
            var method = new HttpMethod(entry.Method);
            var url = $"http://{entry.Host}{entry.Path}";

            // Apply URL modifications
            if (options.ModifiedUrl != null)
            {
                url = options.ModifiedUrl;
            }

            var request = new HttpRequestMessage(method, url);

            // Parse and add headers from original request
            if (!string.IsNullOrEmpty(entry.RawRequest))
            {
                var lines = entry.RawRequest.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                bool isBody = false;
                StringBuilder bodyBuilder = new StringBuilder();

                foreach (var line in lines.Skip(1)) // Skip first line (method line)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        isBody = true;
                        continue;
                    }

                    if (!isBody)
                    {
                        var headerParts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                        if (headerParts.Length == 2)
                        {
                            var headerName = headerParts[0];
                            var headerValue = headerParts[1];

                            // Skip headers that HttpClient sets automatically
                            if (headerName.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                                headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                                continue;

                            try
                            {
                                if (headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Will be set with content
                                }
                                else
                                {
                                    request.Headers.TryAddWithoutValidation(headerName, headerValue);
                                }
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        bodyBuilder.AppendLine(line);
                    }
                }

                // Add body if present
                if (bodyBuilder.Length > 0)
                {
                    var body = bodyBuilder.ToString();

                    // Apply parameter replacements
                    foreach (var replacement in options.ParameterReplacements)
                    {
                        body = body.Replace(replacement.Key, replacement.Value);
                    }

                    request.Content = new StringContent(body, Encoding.UTF8);

                    // Set content type from headers
                    var contentTypeLine = lines.FirstOrDefault(l => l.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase));
                    if (contentTypeLine != null)
                    {
                        var contentType = contentTypeLine.Split(new[] { ": " }, 2)[1];
                        request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
                    }
                }
            }

            // Apply custom headers
            foreach (var header in options.CustomHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return request;
        }
    }

    /// <summary>
    /// Options for replaying requests
    /// </summary>
    public class ReplayOptions
    {
        public string ModifiedUrl { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ParameterReplacements { get; set; } = new Dictionary<string, string>();
        public bool FollowRedirects { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Result of a replayed request
    /// </summary>
    public class ReplayResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string ResponseHeaders { get; set; }
        public string ResponseBody { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Timestamp { get; set; }
        public string Error { get; set; }
        public TrafficEntry OriginalEntry { get; set; }
        public int ReplayNumber { get; set; }
        public string FuzzPayload { get; set; }
    }

    /// <summary>
    /// Comparison between original and replayed response
    /// </summary>
    public class ComparisonResult
    {
        public int OriginalStatus { get; set; }
        public int ReplayedStatus { get; set; }
        public bool StatusChanged { get; set; }
        public long OriginalLength { get; set; }
        public long ReplayedLength { get; set; }
        public long LengthDifference { get; set; }
        public TimeSpan TimeDifference { get; set; }
    }
}
