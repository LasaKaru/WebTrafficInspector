using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Service for intercepting, analyzing, and manipulating WebSocket traffic
    /// </summary>
    public class WebSocketInterceptorService
    {
        private ObservableCollection<WebSocketMessage> _messages = new ObservableCollection<WebSocketMessage>();
        private Dictionary<string, WebSocketConnection> _connections = new Dictionary<string, WebSocketConnection>();
        private List<WebSocketInterceptRule> _interceptRules = new List<WebSocketInterceptRule>();
        private bool _isInterceptEnabled = false;

        public event EventHandler<WebSocketMessageEventArgs> MessageReceived;
        public event EventHandler<WebSocketMessageEventArgs> MessageSent;
        public event EventHandler<WebSocketConnectionEventArgs> ConnectionEstablished;
        public event EventHandler<WebSocketConnectionEventArgs> ConnectionClosed;

        public bool IsInterceptEnabled
        {
            get => _isInterceptEnabled;
            set => _isInterceptEnabled = value;
        }

        public ObservableCollection<WebSocketMessage> Messages => _messages;
        public IReadOnlyDictionary<string, WebSocketConnection> Connections => _connections;

        public WebSocketInterceptorService()
        {
        }

        public void RegisterConnection(string connectionId, string url)
        {
            var connection = new WebSocketConnection
            {
                Id = connectionId,
                Url = url,
                EstablishedAt = DateTime.Now,
                State = WebSocketState.Open,
                MessageCount = 0
            };

            _connections[connectionId] = connection;
            OnConnectionEstablished(new WebSocketConnectionEventArgs { Connection = connection });
        }

        public void UnregisterConnection(string connectionId, WebSocketCloseStatus? closeStatus = null, string closeDescription = null)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                connection.State = WebSocketState.Closed;
                connection.ClosedAt = DateTime.Now;
                connection.CloseStatus = closeStatus;
                connection.CloseDescription = closeDescription;

                OnConnectionClosed(new WebSocketConnectionEventArgs { Connection = connection });
            }
        }

        public WebSocketMessage InterceptMessage(string connectionId, string data, WebSocketMessageDirection direction, WebSocketMessageType messageType)
        {
            var message = new WebSocketMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConnectionId = connectionId,
                Direction = direction,
                Type = messageType,
                Data = data,
                OriginalData = data,
                Timestamp = DateTime.Now,
                Length = Encoding.UTF8.GetByteCount(data),
                IsModified = false
            };

            if (_connections.TryGetValue(connectionId, out var connection))
            {
                connection.MessageCount++;
                message.Url = connection.Url;
            }

            // Apply intercept rules
            if (_isInterceptEnabled)
            {
                foreach (var rule in _interceptRules.Where(r => r.IsEnabled))
                {
                    if (ShouldApplyRule(rule, message))
                    {
                        message = ApplyRule(rule, message);
                    }
                }
            }

            _messages.Add(message);

            if (direction == WebSocketMessageDirection.Incoming)
            {
                OnMessageReceived(new WebSocketMessageEventArgs { Message = message });
            }
            else
            {
                OnMessageSent(new WebSocketMessageEventArgs { Message = message });
            }

            return message;
        }

        private bool ShouldApplyRule(WebSocketInterceptRule rule, WebSocketMessage message)
        {
            if (!string.IsNullOrEmpty(rule.UrlPattern))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(message.Url ?? "", rule.UrlPattern))
                    return false;
            }

            if (rule.Direction.HasValue && rule.Direction != message.Direction)
                return false;

            if (rule.MessageType.HasValue && rule.MessageType != message.Type)
                return false;

            if (!string.IsNullOrEmpty(rule.DataPattern))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(message.Data, rule.DataPattern))
                    return false;
            }

            return true;
        }

        private WebSocketMessage ApplyRule(WebSocketInterceptRule rule, WebSocketMessage message)
        {
            var modifiedData = message.Data;

            switch (rule.Action)
            {
                case WebSocketInterceptAction.Drop:
                    message.IsDropped = true;
                    break;

                case WebSocketInterceptAction.Delay:
                    message.DelayMs = rule.DelayMs;
                    Thread.Sleep(rule.DelayMs);
                    break;

                case WebSocketInterceptAction.ModifyData:
                    if (!string.IsNullOrEmpty(rule.FindPattern) && !string.IsNullOrEmpty(rule.ReplaceWith))
                    {
                        modifiedData = System.Text.RegularExpressions.Regex.Replace(
                            modifiedData, rule.FindPattern, rule.ReplaceWith);
                        message.Data = modifiedData;
                        message.IsModified = true;
                    }
                    break;

                case WebSocketInterceptAction.InjectData:
                    if (!string.IsNullOrEmpty(rule.InjectData))
                    {
                        modifiedData = rule.InjectData;
                        message.Data = modifiedData;
                        message.IsModified = true;
                    }
                    break;

                case WebSocketInterceptAction.Log:
                    message.IsLogged = true;
                    break;

                case WebSocketInterceptAction.Alert:
                    message.IsAlerted = true;
                    break;
            }

            return message;
        }

        public void AddInterceptRule(WebSocketInterceptRule rule)
        {
            _interceptRules.Add(rule);
        }

        public void RemoveInterceptRule(WebSocketInterceptRule rule)
        {
            _interceptRules.Remove(rule);
        }

        public List<WebSocketInterceptRule> GetInterceptRules()
        {
            return new List<WebSocketInterceptRule>(_interceptRules);
        }

        public WebSocketAnalysisReport AnalyzeMessages(string connectionId = null)
        {
            var messagesToAnalyze = string.IsNullOrEmpty(connectionId)
                ? _messages.ToList()
                : _messages.Where(m => m.ConnectionId == connectionId).ToList();

            var report = new WebSocketAnalysisReport
            {
                TotalMessages = messagesToAnalyze.Count,
                IncomingMessages = messagesToAnalyze.Count(m => m.Direction == WebSocketMessageDirection.Incoming),
                OutgoingMessages = messagesToAnalyze.Count(m => m.Direction == WebSocketMessageDirection.Outgoing),
                TextMessages = messagesToAnalyze.Count(m => m.Type == WebSocketMessageType.Text),
                BinaryMessages = messagesToAnalyze.Count(m => m.Type == WebSocketMessageType.Binary),
                ModifiedMessages = messagesToAnalyze.Count(m => m.IsModified),
                DroppedMessages = messagesToAnalyze.Count(m => m.IsDropped),
                TotalDataSize = messagesToAnalyze.Sum(m => m.Length),
                AverageMessageSize = messagesToAnalyze.Any() ? messagesToAnalyze.Average(m => m.Length) : 0,
                ConnectionCount = _connections.Count,
                ActiveConnections = _connections.Count(c => c.Value.State == WebSocketState.Open),
                MessagesByConnection = messagesToAnalyze
                    .GroupBy(m => m.ConnectionId)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Detect potential issues
            report.Issues = new List<string>();

            if (report.DroppedMessages > 0)
            {
                report.Issues.Add($"{report.DroppedMessages} messages were dropped by intercept rules");
            }

            if (report.ModifiedMessages > 0)
            {
                report.Issues.Add($"{report.ModifiedMessages} messages were modified by intercept rules");
            }

            // Check for suspicious patterns
            var suspiciousPatterns = new[]
            {
                @"<script[^>]*>",
                @"javascript:",
                @"eval\(",
                @"document\.cookie",
                @"alert\(",
                @"\.execute\(",
                @"DROP TABLE",
                @"SELECT.*FROM.*WHERE"
            };

            foreach (var message in messagesToAnalyze)
            {
                foreach (var pattern in suspiciousPatterns)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(message.Data, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        report.Issues.Add($"Suspicious pattern detected in message {message.Id}: {pattern}");
                        break;
                    }
                }
            }

            return report;
        }

        public string GenerateReport(WebSocketAnalysisReport analysis)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("          WebSocket Traffic Analysis Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine();

            sb.AppendLine("OVERVIEW:");
            sb.AppendLine("─────────────────────────────────────────────────────────");
            sb.AppendLine($"Total Messages: {analysis.TotalMessages}");
            sb.AppendLine($"  Incoming: {analysis.IncomingMessages}");
            sb.AppendLine($"  Outgoing: {analysis.OutgoingMessages}");
            sb.AppendLine($"  Text: {analysis.TextMessages}");
            sb.AppendLine($"  Binary: {analysis.BinaryMessages}");
            sb.AppendLine();
            sb.AppendLine($"Modified Messages: {analysis.ModifiedMessages}");
            sb.AppendLine($"Dropped Messages: {analysis.DroppedMessages}");
            sb.AppendLine();
            sb.AppendLine($"Total Data Size: {FormatBytes(analysis.TotalDataSize)}");
            sb.AppendLine($"Average Message Size: {FormatBytes((long)analysis.AverageMessageSize)}");
            sb.AppendLine();

            sb.AppendLine("CONNECTIONS:");
            sb.AppendLine("─────────────────────────────────────────────────────────");
            sb.AppendLine($"Total Connections: {analysis.ConnectionCount}");
            sb.AppendLine($"Active Connections: {analysis.ActiveConnections}");
            sb.AppendLine();

            if (analysis.MessagesByConnection.Any())
            {
                sb.AppendLine("Messages per Connection:");
                foreach (var kvp in analysis.MessagesByConnection.OrderByDescending(x => x.Value).Take(10))
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value} messages");
                }
                sb.AppendLine();
            }

            if (analysis.Issues.Any())
            {
                sb.AppendLine("ISSUES DETECTED:");
                sb.AppendLine("─────────────────────────────────────────────────────────");
                foreach (var issue in analysis.Issues)
                {
                    sb.AppendLine($"  • {issue}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public void ClearMessages()
        {
            _messages.Clear();
        }

        public void ClearConnections()
        {
            _connections.Clear();
        }

        protected virtual void OnMessageReceived(WebSocketMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnMessageSent(WebSocketMessageEventArgs e)
        {
            MessageSent?.Invoke(this, e);
        }

        protected virtual void OnConnectionEstablished(WebSocketConnectionEventArgs e)
        {
            ConnectionEstablished?.Invoke(this, e);
        }

        protected virtual void OnConnectionClosed(WebSocketConnectionEventArgs e)
        {
            ConnectionClosed?.Invoke(this, e);
        }
    }

    #region Models

    public class WebSocketMessage
    {
        public string Id { get; set; }
        public string ConnectionId { get; set; }
        public string Url { get; set; }
        public WebSocketMessageDirection Direction { get; set; }
        public WebSocketMessageType Type { get; set; }
        public string Data { get; set; }
        public string OriginalData { get; set; }
        public DateTime Timestamp { get; set; }
        public int Length { get; set; }
        public bool IsModified { get; set; }
        public bool IsDropped { get; set; }
        public bool IsLogged { get; set; }
        public bool IsAlerted { get; set; }
        public int DelayMs { get; set; }
    }

    public class WebSocketConnection
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public DateTime EstablishedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public WebSocketState State { get; set; }
        public int MessageCount { get; set; }
        public WebSocketCloseStatus? CloseStatus { get; set; }
        public string CloseDescription { get; set; }
    }

    public class WebSocketInterceptRule
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string UrlPattern { get; set; }
        public WebSocketMessageDirection? Direction { get; set; }
        public WebSocketMessageType? MessageType { get; set; }
        public string DataPattern { get; set; }
        public WebSocketInterceptAction Action { get; set; }
        public int DelayMs { get; set; }
        public string FindPattern { get; set; }
        public string ReplaceWith { get; set; }
        public string InjectData { get; set; }
    }

    public class WebSocketAnalysisReport
    {
        public int TotalMessages { get; set; }
        public int IncomingMessages { get; set; }
        public int OutgoingMessages { get; set; }
        public int TextMessages { get; set; }
        public int BinaryMessages { get; set; }
        public int ModifiedMessages { get; set; }
        public int DroppedMessages { get; set; }
        public long TotalDataSize { get; set; }
        public double AverageMessageSize { get; set; }
        public int ConnectionCount { get; set; }
        public int ActiveConnections { get; set; }
        public Dictionary<string, int> MessagesByConnection { get; set; }
        public List<string> Issues { get; set; }
    }

    public enum WebSocketMessageDirection
    {
        Incoming,
        Outgoing
    }

    public enum WebSocketMessageType
    {
        Text,
        Binary,
        Close,
        Ping,
        Pong
    }

    public enum WebSocketInterceptAction
    {
        Drop,
        Delay,
        ModifyData,
        InjectData,
        Log,
        Alert
    }

    public class WebSocketMessageEventArgs : EventArgs
    {
        public WebSocketMessage Message { get; set; }
    }

    public class WebSocketConnectionEventArgs : EventArgs
    {
        public WebSocketConnection Connection { get; set; }
    }

    #endregion
}
