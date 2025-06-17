using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class SessionService
    {
        private const string SessionFileExtension = ".wtis";
        private readonly JsonSerializerOptions _jsonOptions;

        public SessionService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<bool> SaveSessionAsync(string filePath, List<TrafficEntry> trafficEntries, string sessionName = null, string description = null)
        {
            try
            {
                var sessionData = new SessionData
                {
                    SessionName = sessionName ?? Path.GetFileNameWithoutExtension(filePath),
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    Description = description ?? "",
                    TrafficEntries = new List<TrafficEntry>(trafficEntries)
                };

                // Generate metadata
                sessionData.Metadata = GenerateMetadata(trafficEntries);

                // Ensure file has correct extension
                if (!filePath.EndsWith(SessionFileExtension))
                {
                    filePath += SessionFileExtension;
                }

                var jsonString = JsonSerializer.Serialize(sessionData, _jsonOptions);
                await File.WriteAllTextAsync(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save session: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<SessionData> LoadSessionAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Session file not found: {filePath}");
                }

                var jsonString = await File.ReadAllTextAsync(filePath);
                var sessionData = JsonSerializer.Deserialize<SessionData>(jsonString, _jsonOptions);

                if (sessionData == null)
                {
                    throw new InvalidOperationException("Failed to deserialize session data");
                }

                // Update last modified
                sessionData.LastModified = DateTime.Now;

                return sessionData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load session: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public List<string> GetRecentSessions(int maxCount = 10)
        {
            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var sessionsPath = Path.Combine(documentsPath, "WebTrafficInspector", "Sessions");

                if (!Directory.Exists(sessionsPath))
                    return new List<string>();

                return Directory.GetFiles(sessionsPath, $"*{SessionFileExtension}")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .Take(maxCount)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public string GetDefaultSessionsPath()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var sessionsPath = Path.Combine(documentsPath, "WebTrafficInspector", "Sessions");

            if (!Directory.Exists(sessionsPath))
            {
                Directory.CreateDirectory(sessionsPath);
            }

            return sessionsPath;
        }

        private SessionMetadata GenerateMetadata(List<TrafficEntry> entries)
        {
            var metadata = new SessionMetadata
            {
                TotalRequests = entries.Count,
                HttpRequests = entries.Count(e => e.Host != null && !e.Host.Contains("443")),
                HttpsRequests = entries.Count(e => e.Host != null && e.Host.Contains("443"))
            };

            // Unique hosts
            metadata.UniqueHosts = entries
                .Where(e => !string.IsNullOrEmpty(e.Host))
                .Select(e => e.Host)
                .Distinct()
                .ToList();

            // Method counts
            metadata.MethodCounts = entries
                .Where(e => !string.IsNullOrEmpty(e.Method))
                .GroupBy(e => e.Method)
                .ToDictionary(g => g.Key, g => g.Count());

            // Status counts
            metadata.StatusCounts = entries
                .Where(e => e.Status > 0)
                .GroupBy(e => e.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return metadata;
        }
    }
}
