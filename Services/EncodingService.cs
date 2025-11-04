using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Comprehensive encoding, decoding, and cryptographic utilities
    /// </summary>
    public class EncodingService
    {
        // URL Encoding/Decoding
        public string UrlEncode(string input)
        {
            return HttpUtility.UrlEncode(input);
        }

        public string UrlDecode(string input)
        {
            return HttpUtility.UrlDecode(input);
        }

        public string UrlEncodeAll(string input)
        {
            return string.Join("", input.Select(c => $"%{((int)c):X2}"));
        }

        // Base64 Encoding/Decoding
        public string Base64Encode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public string Base64Decode(string input)
        {
            try
            {
                var bytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "Invalid Base64 string";
            }
        }

        // HTML Encoding/Decoding
        public string HtmlEncode(string input)
        {
            return HttpUtility.HtmlEncode(input);
        }

        public string HtmlDecode(string input)
        {
            return HttpUtility.HtmlDecode(input);
        }

        // Hex Encoding/Decoding
        public string HexEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public string HexDecode(string input)
        {
            try
            {
                var bytes = Enumerable.Range(0, input.Length / 2)
                    .Select(x => Convert.ToByte(input.Substring(x * 2, 2), 16))
                    .ToArray();
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "Invalid hex string";
            }
        }

        // Unicode Escape
        public string UnicodeEscape(string input)
        {
            return string.Join("", input.Select(c => $"\\u{((int)c):X4}"));
        }

        public string UnicodeUnescape(string input)
        {
            return Regex.Unescape(input);
        }

        // JWT Decoder (Basic - doesn't verify signature)
        public JwtInfo DecodeJwt(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return new JwtInfo { Error = "Invalid JWT format" };

                var header = Base64UrlDecode(parts[0]);
                var payload = Base64UrlDecode(parts[1]);

                return new JwtInfo
                {
                    Header = header,
                    Payload = payload,
                    Signature = parts[2],
                    IsValid = true
                };
            }
            catch (Exception ex)
            {
                return new JwtInfo { Error = ex.Message };
            }
        }

        private string Base64UrlDecode(string input)
        {
            var base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Base64Decode(base64);
        }

        // Hash Functions
        public string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string SHA1Hash(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha1.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string SHA256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string SHA512Hash(string input)
        {
            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha512.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // ROT13
        public string ROT13(string input)
        {
            return string.Join("", input.Select(c =>
            {
                if (c >= 'a' && c <= 'z')
                    return (char)((c - 'a' + 13) % 26 + 'a');
                if (c >= 'A' && c <= 'Z')
                    return (char)((c - 'A' + 13) % 26 + 'A');
                return c;
            }));
        }

        // Binary
        public string ToBinary(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return string.Join(" ", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }

        public string FromBinary(string input)
        {
            try
            {
                var bytes = input.Split(' ')
                    .Select(b => Convert.ToByte(b, 2))
                    .ToArray();
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "Invalid binary string";
            }
        }

        // Smart Auto-Decoder (tries to detect encoding)
        public DecodingResult AutoDecode(string input)
        {
            var results = new List<DecodingAttempt>();

            // Try URL decode
            try
            {
                var urlDecoded = UrlDecode(input);
                if (urlDecoded != input)
                {
                    results.Add(new DecodingAttempt
                    {
                        Method = "URL Decode",
                        Result = urlDecoded,
                        Confidence = CalculateUrlEncodeConfidence(input)
                    });
                }
            }
            catch { }

            // Try Base64 decode
            try
            {
                var base64Decoded = Base64Decode(input);
                if (!base64Decoded.Contains("Invalid"))
                {
                    results.Add(new DecodingAttempt
                    {
                        Method = "Base64 Decode",
                        Result = base64Decoded,
                        Confidence = CalculateBase64Confidence(input)
                    });
                }
            }
            catch { }

            // Try Hex decode
            if (input.Length % 2 == 0 && Regex.IsMatch(input, "^[0-9A-Fa-f]+$"))
            {
                try
                {
                    var hexDecoded = HexDecode(input);
                    if (!hexDecoded.Contains("Invalid"))
                    {
                        results.Add(new DecodingAttempt
                        {
                            Method = "Hex Decode",
                            Result = hexDecoded,
                            Confidence = 90
                        });
                    }
                }
                catch { }
            }

            // Try JWT decode
            if (input.Split('.').Length == 3)
            {
                var jwt = DecodeJwt(input);
                if (jwt.IsValid)
                {
                    results.Add(new DecodingAttempt
                    {
                        Method = "JWT Decode",
                        Result = $"Header: {jwt.Header}\n\nPayload: {jwt.Payload}",
                        Confidence = 95
                    });
                }
            }

            // Try HTML decode
            try
            {
                var htmlDecoded = HtmlDecode(input);
                if (htmlDecoded != input)
                {
                    results.Add(new DecodingAttempt
                    {
                        Method = "HTML Decode",
                        Result = htmlDecoded,
                        Confidence = CalculateHtmlEncodeConfidence(input)
                    });
                }
            }
            catch { }

            return new DecodingResult
            {
                OriginalInput = input,
                Attempts = results.OrderByDescending(a => a.Confidence).ToList(),
                BestGuess = results.OrderByDescending(a => a.Confidence).FirstOrDefault()
            };
        }

        private int CalculateUrlEncodeConfidence(string input)
        {
            int score = 0;
            if (input.Contains("%")) score += 30;
            if (input.Contains("+")) score += 20;
            if (Regex.IsMatch(input, "%[0-9A-Fa-f]{2}")) score += 50;
            return Math.Min(score, 100);
        }

        private int CalculateBase64Confidence(string input)
        {
            int score = 0;
            if (Regex.IsMatch(input, "^[A-Za-z0-9+/]*={0,2}$")) score += 50;
            if (input.Length % 4 == 0) score += 30;
            if (input.EndsWith("==") || input.EndsWith("=")) score += 20;
            return Math.Min(score, 100);
        }

        private int CalculateHtmlEncodeConfidence(string input)
        {
            int score = 0;
            if (input.Contains("&")) score += 30;
            if (input.Contains("&lt;") || input.Contains("&gt;")) score += 40;
            if (input.Contains("&quot;") || input.Contains("&amp;")) score += 30;
            return Math.Min(score, 100);
        }

        // Extract and decode all encoded strings from text
        public List<ExtractedEncoding> ExtractEncodedStrings(string text)
        {
            var results = new List<ExtractedEncoding>();

            // Extract Base64 strings
            var base64Pattern = @"\b[A-Za-z0-9+/]{20,}={0,2}\b";
            foreach (Match match in Regex.Matches(text, base64Pattern))
            {
                try
                {
                    var decoded = Base64Decode(match.Value);
                    if (!decoded.Contains("Invalid"))
                    {
                        results.Add(new ExtractedEncoding
                        {
                            Type = "Base64",
                            Original = match.Value,
                            Decoded = decoded,
                            Position = match.Index
                        });
                    }
                }
                catch { }
            }

            // Extract URL-encoded strings
            var urlPattern = @"%[0-9A-Fa-f]{2}";
            if (Regex.IsMatch(text, urlPattern))
            {
                try
                {
                    var decoded = UrlDecode(text);
                    if (decoded != text)
                    {
                        results.Add(new ExtractedEncoding
                        {
                            Type = "URL Encoded",
                            Original = text,
                            Decoded = decoded,
                            Position = 0
                        });
                    }
                }
                catch { }
            }

            // Extract JWTs
            var jwtPattern = @"\beyJ[A-Za-z0-9_-]*\.eyJ[A-Za-z0-9_-]*\.[A-Za-z0-9_-]*\b";
            foreach (Match match in Regex.Matches(text, jwtPattern))
            {
                var jwt = DecodeJwt(match.Value);
                if (jwt.IsValid)
                {
                    results.Add(new ExtractedEncoding
                    {
                        Type = "JWT",
                        Original = match.Value,
                        Decoded = $"Header: {jwt.Header}\nPayload: {jwt.Payload}",
                        Position = match.Index
                    });
                }
            }

            return results;
        }
    }

    public class JwtInfo
    {
        public string Header { get; set; }
        public string Payload { get; set; }
        public string Signature { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
    }

    public class DecodingResult
    {
        public string OriginalInput { get; set; }
        public List<DecodingAttempt> Attempts { get; set; }
        public DecodingAttempt BestGuess { get; set; }
    }

    public class DecodingAttempt
    {
        public string Method { get; set; }
        public string Result { get; set; }
        public int Confidence { get; set; }
    }

    public class ExtractedEncoding
    {
        public string Type { get; set; }
        public string Original { get; set; }
        public string Decoded { get; set; }
        public int Position { get; set; }
    }
}
