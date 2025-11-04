using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class ReconnaissanceService
    {
        private HttpClient _httpClient;
        private List<string> _commonSubdomains;
        private List<string> _commonDirectories;
        private List<int> _commonPorts;

        public ReconnaissanceService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            LoadDefaultWordlists();
        }

        public void LoadDefaultWordlists()
        {
            _commonSubdomains = new List<string>
            {
                "www", "mail", "ftp", "localhost", "webmail", "smtp", "pop", "ns1", "webdisk",
                "ns2", "cpanel", "whm", "autodiscover", "autoconfig", "m", "imap", "test", "ns",
                "blog", "pop3", "dev", "www2", "admin", "forum", "news", "vpn", "ns3", "mail2",
                "new", "mysql", "old", "lists", "support", "mobile", "mx", "static", "docs", "beta",
                "shop", "sql", "secure", "demo", "cp", "calendar", "wiki", "web", "media", "email",
                "images", "img", "www1", "intranet", "portal", "video", "sip", "dns2", "api", "cdn",
                "stats", "dns1", "ns4", "www3", "dns", "search", "staging", "server", "mx1", "chat",
                "wap", "my", "svn", "mail1", "sites", "proxy", "ads", "host", "crm", "cms", "backup",
                "mx2", "lyncdiscover", "info", "apps", "download", "remote", "db", "forums", "store",
                "relay", "files", "newsletter", "app", "live", "owa", "en", "start", "sms", "office"
            };

            _commonDirectories = new List<string>
            {
                "/admin", "/administrator", "/login", "/wp-admin", "/wp-login.php", "/user/login",
                "/admin/login", "/dashboard", "/cpanel", "/control", "/controlpanel", "/admin/index.php",
                "/admin/login.php", "/admin/admin.php", "/admin_area", "/admincontrol", "/admin/cp.php",
                "/moderator", "/webadmin", "/adminarea", "/bb-admin", "/adminLogin", "/admin_login",
                "/panel-administracion", "/instadmin", "/memberadmin", "/administratorlogin", "/adm",
                "/admin/account.php", "/admin/index.html", "/admin/login.html", "/admin/admin.html",
                "/admin_area/admin.php", "/admin_area/login.php", "/siteadmin/login.php", "/admin.php",
                "/admin.html", "/login.php", "/login.html", "/modelsearch/login.php", "/moderator.php",
                "/moderator/login.php", "/moderator/admin.php", "/administrator.php", "/admincp/index.asp",
                "/admincp/login.asp", "/admincp/index.html", "/admin/account.html", "/adminpanel.html",
                "/webadmin.html", "/pages/admin/admin-login.php", "/admin/admin-login.php", "/admin-login.php",
                "/bb-admin/index.php", "/bb-admin/login.php", "/acceso.php", "/bb-admin/admin.php",
                "/admin/home.php", "/admin/controlpanel.html", "/admin.html", "/panel-administracion/login.html",
                "/api", "/api/v1", "/api/v2", "/api/docs", "/swagger", "/graphql", "/rest", "/services",
                "/.git", "/.env", "/config", "/backup", "/backups", "/db", "/database", "/uploads",
                "/files", "/assets", "/static", "/public", "/private", "/temp", "/tmp", "/logs",
                "/debug", "/test", "/dev", "/.svn", "/.hg", "/phpinfo.php", "/info.php", "/server-status",
                "/robots.txt", "/sitemap.xml", "/.htaccess", "/.htpasswd", "/web.config", "/wp-config.php"
            };

            _commonPorts = new List<int>
            {
                21, 22, 23, 25, 53, 80, 110, 111, 135, 139, 143, 443, 445, 993, 995,
                1723, 3306, 3389, 5900, 8080, 8443, 8888, 9090
            };
        }

        public async Task<SubdomainEnumerationReport> EnumerateSubdomains(string domain, ReconOptions options = null)
        {
            options = options ?? new ReconOptions();

            var report = new SubdomainEnumerationReport
            {
                Domain = domain,
                StartTime = DateTime.Now,
                DiscoveredSubdomains = new List<SubdomainInfo>()
            };

            var subdomains = options.CustomWordlist ?? _commonSubdomains;

            foreach (var subdomain in subdomains)
            {
                var fullDomain = $"{subdomain}.{domain}";

                try
                {
                    var addresses = await Dns.GetHostAddressesAsync(fullDomain);

                    if (addresses.Length > 0)
                    {
                        var info = new SubdomainInfo
                        {
                            Subdomain = fullDomain,
                            IPAddresses = addresses.Select(a => a.ToString()).ToList(),
                            IsActive = true,
                            DiscoveredAt = DateTime.Now
                        };

                        // Try to determine if HTTP/HTTPS is available
                        info.SupportsHTTP = await CheckHTTPAvailability(fullDomain, false);
                        info.SupportsHTTPS = await CheckHTTPAvailability(fullDomain, true);

                        report.DiscoveredSubdomains.Add(info);
                    }
                }
                catch
                {
                    // Subdomain doesn't exist or DNS resolution failed
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            report.EndTime = DateTime.Now;
            report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
            report.TotalTested = subdomains.Count;
            report.TotalFound = report.DiscoveredSubdomains.Count;

            return report;
        }

        public async Task<DirectoryEnumerationReport> EnumerateDirectories(string baseUrl, ReconOptions options = null)
        {
            options = options ?? new ReconOptions();

            var report = new DirectoryEnumerationReport
            {
                BaseUrl = baseUrl,
                StartTime = DateTime.Now,
                DiscoveredPaths = new List<DirectoryInfo>()
            };

            var directories = options.CustomWordlist ?? _commonDirectories;

            foreach (var dir in directories)
            {
                var url = baseUrl.TrimEnd('/') + dir;

                try
                {
                    var response = await _httpClient.GetAsync(url);

                    var info = new DirectoryInfo
                    {
                        Path = dir,
                        FullUrl = url,
                        StatusCode = (int)response.StatusCode,
                        ContentLength = response.Content.Headers.ContentLength ?? 0,
                        IsAccessible = response.IsSuccessStatusCode,
                        DiscoveredAt = DateTime.Now
                    };

                    // Only add if not 404
                    if (response.StatusCode != HttpStatusCode.NotFound)
                    {
                        report.DiscoveredPaths.Add(info);
                    }

                    report.TotalTested++;
                }
                catch
                {
                    // Ignore errors
                    report.TotalTested++;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            report.EndTime = DateTime.Now;
            report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
            report.TotalFound = report.DiscoveredPaths.Count;

            return report;
        }

        public async Task<PortScanReport> ScanPorts(string host, ReconOptions options = null)
        {
            options = options ?? new ReconOptions();

            var report = new PortScanReport
            {
                Host = host,
                StartTime = DateTime.Now,
                OpenPorts = new List<PortInfo>()
            };

            var ports = options.PortRange ?? _commonPorts;

            foreach (var port in ports)
            {
                var isOpen = await IsPortOpen(host, port, options.PortTimeout);

                if (isOpen)
                {
                    var info = new PortInfo
                    {
                        Port = port,
                        IsOpen = true,
                        Service = GetServiceName(port),
                        DiscoveredAt = DateTime.Now
                    };

                    report.OpenPorts.Add(info);
                }

                report.TotalTested++;
            }

            report.EndTime = DateTime.Now;
            report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
            report.TotalFound = report.OpenPorts.Count;

            return report;
        }

        public async Task<WebTechnologyReport> FingerprintTechnologies(string url)
        {
            var report = new WebTechnologyReport
            {
                Url = url,
                DetectedTechnologies = new List<TechnologyInfo>(),
                Timestamp = DateTime.Now
            };

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

                // Detect technologies from headers
                DetectFromHeaders(headers, report);

                // Detect technologies from content
                DetectFromContent(content, report);

                // Detect from meta tags
                DetectFromMetaTags(content, report);
            }
            catch
            {
                // Ignore errors
            }

            return report;
        }

        public async Task<RobotsTxtAnalysis> AnalyzeRobotsTxt(string baseUrl)
        {
            var analysis = new RobotsTxtAnalysis
            {
                BaseUrl = baseUrl,
                DisallowedPaths = new List<string>(),
                Sitemaps = new List<string>(),
                Exists = false
            };

            try
            {
                var robotsUrl = baseUrl.TrimEnd('/') + "/robots.txt";
                var response = await _httpClient.GetAsync(robotsUrl);

                if (response.IsSuccessStatusCode)
                {
                    analysis.Exists = true;
                    var content = await response.Content.ReadAsStringAsync();
                    analysis.RawContent = content;

                    var lines = content.Split('\n');
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();

                        if (trimmed.StartsWith("Disallow:", StringComparison.OrdinalIgnoreCase))
                        {
                            var path = trimmed.Substring(9).Trim();
                            if (!string.IsNullOrEmpty(path))
                            {
                                analysis.DisallowedPaths.Add(path);
                            }
                        }
                        else if (trimmed.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase))
                        {
                            var sitemap = trimmed.Substring(8).Trim();
                            if (!string.IsNullOrEmpty(sitemap))
                            {
                                analysis.Sitemaps.Add(sitemap);
                            }
                        }
                    }
                }
            }
            catch
            {
                // robots.txt doesn't exist or error occurred
            }

            return analysis;
        }

        public async Task<EmailAddressDiscovery> DiscoverEmailAddresses(string url)
        {
            var discovery = new EmailAddressDiscovery
            {
                Url = url,
                EmailAddresses = new List<string>(),
                Timestamp = DateTime.Now
            };

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                var matches = Regex.Matches(content, emailPattern);

                foreach (Match match in matches)
                {
                    if (!discovery.EmailAddresses.Contains(match.Value))
                    {
                        discovery.EmailAddresses.Add(match.Value);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return discovery;
        }

        private async Task<bool> CheckHTTPAvailability(string domain, bool https)
        {
            try
            {
                var protocol = https ? "https://" : "http://";
                var response = await _httpClient.GetAsync(protocol + domain);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsPortOpen(string host, int port, int timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout));

                    if (success)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetServiceName(int port)
        {
            return port switch
            {
                21 => "FTP",
                22 => "SSH",
                23 => "Telnet",
                25 => "SMTP",
                53 => "DNS",
                80 => "HTTP",
                110 => "POP3",
                143 => "IMAP",
                443 => "HTTPS",
                445 => "SMB",
                3306 => "MySQL",
                3389 => "RDP",
                5900 => "VNC",
                8080 => "HTTP-Alt",
                8443 => "HTTPS-Alt",
                _ => "Unknown"
            };
        }

        private void DetectFromHeaders(Dictionary<string, string> headers, WebTechnologyReport report)
        {
            foreach (var header in headers)
            {
                if (header.Key.Equals("Server", StringComparison.OrdinalIgnoreCase))
                {
                    report.DetectedTechnologies.Add(new TechnologyInfo
                    {
                        Name = "Server",
                        Version = header.Value,
                        Category = "Web Server",
                        Confidence = 100
                    });
                }
                else if (header.Key.Equals("X-Powered-By", StringComparison.OrdinalIgnoreCase))
                {
                    report.DetectedTechnologies.Add(new TechnologyInfo
                    {
                        Name = "Framework",
                        Version = header.Value,
                        Category = "Framework",
                        Confidence = 100
                    });
                }
            }
        }

        private void DetectFromContent(string content, WebTechnologyReport report)
        {
            var patterns = new Dictionary<string, (string name, string category)>
            {
                ["wp-content"] = ("WordPress", "CMS"),
                ["Drupal"] = ("Drupal", "CMS"),
                ["joomla"] = ("Joomla", "CMS"),
                ["react"] = ("React", "Frontend Framework"),
                ["angular"] = ("Angular", "Frontend Framework"),
                ["vue"] = ("Vue.js", "Frontend Framework"),
                ["jquery"] = ("jQuery", "JavaScript Library"),
                ["bootstrap"] = ("Bootstrap", "CSS Framework"),
                ["laravel"] = ("Laravel", "PHP Framework"),
                ["django"] = ("Django", "Python Framework"),
                ["flask"] = ("Flask", "Python Framework")
            };

            foreach (var pattern in patterns)
            {
                if (content.Contains(pattern.Key, StringComparison.OrdinalIgnoreCase))
                {
                    report.DetectedTechnologies.Add(new TechnologyInfo
                    {
                        Name = pattern.Value.name,
                        Category = pattern.Value.category,
                        Confidence = 75
                    });
                }
            }
        }

        private void DetectFromMetaTags(string content, WebTechnologyReport report)
        {
            var generatorMatch = Regex.Match(content, @"<meta\s+name=[""']generator[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            if (generatorMatch.Success)
            {
                report.DetectedTechnologies.Add(new TechnologyInfo
                {
                    Name = "Generator",
                    Version = generatorMatch.Groups[1].Value,
                    Category = "CMS/Framework",
                    Confidence = 90
                });
            }
        }

        public string GenerateReconReport(SubdomainEnumerationReport subdomains, DirectoryEnumerationReport directories, PortScanReport ports)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("           RECONNAISSANCE REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine();

            if (subdomains != null)
            {
                sb.AppendLine($"SUBDOMAIN ENUMERATION ({subdomains.Domain}):");
                sb.AppendLine($"  Tested: {subdomains.TotalTested}");
                sb.AppendLine($"  Found: {subdomains.TotalFound}");
                sb.AppendLine();

                foreach (var sub in subdomains.DiscoveredSubdomains.Take(20))
                {
                    var ips = string.Join(", ", sub.IPAddresses);
                    var protocols = new List<string>();
                    if (sub.SupportsHTTP) protocols.Add("HTTP");
                    if (sub.SupportsHTTPS) protocols.Add("HTTPS");
                    var protoStr = protocols.Any() ? $" [{string.Join(", ", protocols)}]" : "";

                    sb.AppendLine($"  {sub.Subdomain} -> {ips}{protoStr}");
                }
                sb.AppendLine();
            }

            if (directories != null)
            {
                sb.AppendLine($"DIRECTORY ENUMERATION:");
                sb.AppendLine($"  Base URL: {directories.BaseUrl}");
                sb.AppendLine($"  Tested: {directories.TotalTested}");
                sb.AppendLine($"  Found: {directories.TotalFound}");
                sb.AppendLine();

                var grouped = directories.DiscoveredPaths.GroupBy(d => d.StatusCode);
                foreach (var group in grouped)
                {
                    sb.AppendLine($"  Status {group.Key}:");
                    foreach (var dir in group.Take(15))
                    {
                        sb.AppendLine($"    {dir.Path} ({dir.ContentLength} bytes)");
                    }
                }
                sb.AppendLine();
            }

            if (ports != null)
            {
                sb.AppendLine($"PORT SCAN RESULTS:");
                sb.AppendLine($"  Host: {ports.Host}");
                sb.AppendLine($"  Tested: {ports.TotalTested}");
                sb.AppendLine($"  Open: {ports.TotalFound}");
                sb.AppendLine();

                foreach (var port in ports.OpenPorts)
                {
                    sb.AppendLine($"  Port {port.Port}/TCP - {port.Service} [OPEN]");
                }
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    public class ReconOptions
    {
        public List<string> CustomWordlist { get; set; }
        public List<int> PortRange { get; set; }
        public int DelayBetweenRequests { get; set; } = 100;
        public int PortTimeout { get; set; } = 1000;
    }

    public class SubdomainEnumerationReport
    {
        public string Domain { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalTested { get; set; }
        public int TotalFound { get; set; }
        public List<SubdomainInfo> DiscoveredSubdomains { get; set; }
    }

    public class SubdomainInfo
    {
        public string Subdomain { get; set; }
        public List<string> IPAddresses { get; set; }
        public bool IsActive { get; set; }
        public bool SupportsHTTP { get; set; }
        public bool SupportsHTTPS { get; set; }
        public DateTime DiscoveredAt { get; set; }
    }

    public class DirectoryEnumerationReport
    {
        public string BaseUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalTested { get; set; }
        public int TotalFound { get; set; }
        public List<DirectoryInfo> DiscoveredPaths { get; set; }
    }

    public class DirectoryInfo
    {
        public string Path { get; set; }
        public string FullUrl { get; set; }
        public int StatusCode { get; set; }
        public long ContentLength { get; set; }
        public bool IsAccessible { get; set; }
        public DateTime DiscoveredAt { get; set; }
    }

    public class PortScanReport
    {
        public string Host { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalTested { get; set; }
        public int TotalFound { get; set; }
        public List<PortInfo> OpenPorts { get; set; }
    }

    public class PortInfo
    {
        public int Port { get; set; }
        public bool IsOpen { get; set; }
        public string Service { get; set; }
        public DateTime DiscoveredAt { get; set; }
    }

    public class WebTechnologyReport
    {
        public string Url { get; set; }
        public List<TechnologyInfo> DetectedTechnologies { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TechnologyInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Category { get; set; }
        public int Confidence { get; set; }
    }

    public class RobotsTxtAnalysis
    {
        public string BaseUrl { get; set; }
        public bool Exists { get; set; }
        public string RawContent { get; set; }
        public List<string> DisallowedPaths { get; set; }
        public List<string> Sitemaps { get; set; }
    }

    public class EmailAddressDiscovery
    {
        public string Url { get; set; }
        public List<string> EmailAddresses { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
