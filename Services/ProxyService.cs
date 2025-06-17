using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using WebTrafficInspector.Models;
using System.Linq;
using Titanium.Web.Proxy.Network;
using System.Collections.Concurrent;

namespace WebTrafficInspector.Services
{
    public class ProxyService
    {
        private ProxyServer _proxyServer;
        private ExplicitProxyEndPoint _explicitEndPoint;
        private int _requestCounter = 0;
        private readonly ConcurrentDictionary<string, TrafficEntry> _pendingRequests = new();

        public event Action<TrafficEntry> TrafficCaptured;
        public int ProxyPort { get; private set; } = 8080;

        public async Task StartProxyAsync()
        {
            try
            {
                _proxyServer = new ProxyServer();

                // Configure certificate handling
                try
                {
                    _proxyServer.CertificateManager.CertificateEngine = CertificateEngine.BouncyCastle;
                    _proxyServer.CertificateManager.TrustRootCertificate(true);
                }
                catch (Exception certEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Certificate setup failed: {certEx.Message}");
                    // Continue without HTTPS interception
                }

                // Create explicit proxy endpoint
                _explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, ProxyPort, true);

                // Enable SSL decryption for HTTPS traffic
                _explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnect;

                _proxyServer.AddEndPoint(_explicitEndPoint);

                // Register event handlers for all traffic
                _proxyServer.BeforeRequest += OnRequest;
                _proxyServer.BeforeResponse += OnResponse;
                _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;

                _proxyServer.Start();

                System.Diagnostics.Debug.WriteLine($"Proxy started on port {ProxyPort}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to start proxy server: {ex.Message}", ex);
            }
        }

        private async Task OnBeforeTunnelConnect(object sender, TunnelConnectSessionEventArgs e)
        {
            // Decrypt all HTTPS traffic to capture it
            e.DecryptSsl = true;
        }

        public void StopProxy()
        {
            try
            {
                _proxyServer?.Stop();
                _proxyServer?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping proxy: {ex.Message}");
            }
        }

        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            try
            {
                // Create unique session identifier using request details and thread-safe counter
                var sessionId = $"{e.HttpClient.Request.Method}_{e.HttpClient.Request.Host}_{e.HttpClient.Request.RequestUri.PathAndQuery}_{DateTime.Now.Ticks}_{System.Threading.Interlocked.Increment(ref _requestCounter)}";

                var entry = new TrafficEntry
                {
                    Id = _requestCounter,
                    Timestamp = DateTime.Now,
                    Method = e.HttpClient.Request.Method,
                    Host = e.HttpClient.Request.Host,
                    Path = e.HttpClient.Request.RequestUri.PathAndQuery,
                    Status = 0, // Will be updated in response
                    Length = 0  // Will be updated in response
                };

                // Build raw request
                var requestBuilder = new StringBuilder();
                requestBuilder.AppendLine($"{e.HttpClient.Request.Method} {e.HttpClient.Request.RequestUri.PathAndQuery} HTTP/{e.HttpClient.Request.HttpVersion}");
                requestBuilder.AppendLine($"Host: {e.HttpClient.Request.Host}");

                // Add all headers
                foreach (var header in e.HttpClient.Request.Headers)
                {
                    requestBuilder.AppendLine($"{header.Name}: {header.Value}");
                }

                // Add request body if present
                if (e.HttpClient.Request.HasBody)
                {
                    try
                    {
                        var bodyString = await e.GetRequestBodyAsString();
                        if (!string.IsNullOrEmpty(bodyString))
                        {
                            requestBuilder.AppendLine();
                            requestBuilder.AppendLine(bodyString);
                        }
                    }
                    catch
                    {
                        requestBuilder.AppendLine();
                        requestBuilder.AppendLine("[Binary or unreadable body content]");
                    }
                }

                entry.RawRequest = requestBuilder.ToString();

                // Store the entry for response matching
                _pendingRequests[sessionId] = entry;

                // Store session ID in the session for response matching
                e.UserData = sessionId;

                System.Diagnostics.Debug.WriteLine($"Captured request: {entry.Method} {entry.Host}{entry.Path}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnRequest: {ex.Message}");
            }
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            try
            {
                TrafficEntry entry = null;
                string sessionId = e.UserData as string;

                // Try to match with pending request
                if (!string.IsNullOrEmpty(sessionId) && _pendingRequests.TryRemove(sessionId, out entry))
                {
                    // Update the existing entry with response data
                    entry.Status = e.HttpClient.Response.StatusCode;
                    entry.Length = e.HttpClient.Response.ContentLength;
                }
                else
                {
                    // Create new entry if no matching request found
                    entry = new TrafficEntry
                    {
                        Id = System.Threading.Interlocked.Increment(ref _requestCounter),
                        Timestamp = DateTime.Now,
                        Method = e.HttpClient.Request.Method,
                        Host = e.HttpClient.Request.Host,
                        Path = e.HttpClient.Request.RequestUri.PathAndQuery,
                        Status = e.HttpClient.Response.StatusCode,
                        Length = e.HttpClient.Response.ContentLength,
                        RawRequest = "[Request not captured]"
                    };
                }

                // Build raw response
                var responseBuilder = new StringBuilder();
                responseBuilder.AppendLine($"HTTP/{e.HttpClient.Response.HttpVersion} {e.HttpClient.Response.StatusCode} {e.HttpClient.Response.StatusDescription}");

                // Add all response headers
                foreach (var header in e.HttpClient.Response.Headers)
                {
                    responseBuilder.AppendLine($"{header.Name}: {header.Value}");
                }

                // Add response body if present
                if (e.HttpClient.Response.HasBody)
                {
                    try
                    {
                        var bodyString = await e.GetResponseBodyAsString();
                        if (!string.IsNullOrEmpty(bodyString))
                        {
                            responseBuilder.AppendLine();
                            responseBuilder.AppendLine(bodyString);
                        }
                    }
                    catch
                    {
                        responseBuilder.AppendLine();
                        responseBuilder.AppendLine("[Binary or unreadable body content]");
                    }
                }

                entry.RawResponse = responseBuilder.ToString();

                // Notify UI on main thread
                TrafficCaptured?.Invoke(entry);

                System.Diagnostics.Debug.WriteLine($"Captured response: {entry.Status} for {entry.Host}{entry.Path}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnResponse: {ex.Message}");
            }
        }

        private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            // Accept all certificates for debugging purposes
            e.IsValid = true;
            return Task.CompletedTask;
        }
    }
}
