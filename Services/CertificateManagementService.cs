using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Service to manage HTTPS certificates for proxy without requiring user configuration
    /// </summary>
    public class CertificateManagementService
    {
        private const string CERT_FOLDER = "Certificates";
        private const string CERT_FILE = "proxy-cert.pfx";
        private const string CERT_PASSWORD = "WebTrafficInspector2024";

        public CertificateManagementService()
        {
        }

        /// <summary>
        /// Gets or creates a self-signed certificate for the proxy
        /// </summary>
        public X509Certificate2 GetOrCreateCertificate()
        {
            try
            {
                var certPath = GetCertificatePath();

                // Try to load existing certificate
                if (File.Exists(certPath))
                {
                    try
                    {
                        var cert = new X509Certificate2(certPath, CERT_PASSWORD, X509KeyStorageFlags.Exportable);

                        // Check if certificate is still valid
                        if (cert.NotAfter > DateTime.Now.AddDays(30))
                        {
                            return cert;
                        }
                        else
                        {
                            // Certificate expiring soon, delete and create new one
                            File.Delete(certPath);
                        }
                    }
                    catch
                    {
                        // Certificate file corrupted, delete it
                        try { File.Delete(certPath); } catch { }
                    }
                }

                // Create new certificate
                return CreateSelfSignedCertificate(certPath);
            }
            catch
            {
                // Fallback: create in-memory certificate
                return CreateInMemoryCertificate();
            }
        }

        private X509Certificate2 CreateSelfSignedCertificate(string certPath)
        {
            try
            {
                // Ensure directory exists
                var certDir = Path.GetDirectoryName(certPath);
                if (!Directory.Exists(certDir))
                {
                    Directory.CreateDirectory(certDir);
                }

                // Create certificate using ECDsa (more modern than RSA)
                using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                {
                    var request = new CertificateRequest(
                        "CN=WebTrafficInspector Proxy",
                        ecdsa,
                        HashAlgorithmName.SHA256);

                    // Add extensions
                    request.CertificateExtensions.Add(
                        new X509BasicConstraintsExtension(false, false, 0, false));

                    request.CertificateExtensions.Add(
                        new X509KeyUsageExtension(
                            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                            false));

                    request.CertificateExtensions.Add(
                        new X509EnhancedKeyUsageExtension(
                            new OidCollection {
                                new Oid("1.3.6.1.5.5.7.3.1") // Server Authentication
                            },
                            false));

                    // Create certificate valid for 1 year
                    var certificate = request.CreateSelfSigned(
                        DateTimeOffset.Now.AddDays(-1),
                        DateTimeOffset.Now.AddYears(1));

                    // Export certificate with private key
                    var certBytes = certificate.Export(X509ContentType.Pfx, CERT_PASSWORD);
                    File.WriteAllBytes(certPath, certBytes);

                    // Return certificate that can be used
                    return new X509Certificate2(certPath, CERT_PASSWORD, X509KeyStorageFlags.Exportable);
                }
            }
            catch
            {
                // If file operation fails, return in-memory certificate
                return CreateInMemoryCertificate();
            }
        }

        private X509Certificate2 CreateInMemoryCertificate()
        {
            try
            {
                using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                {
                    var request = new CertificateRequest(
                        "CN=WebTrafficInspector",
                        ecdsa,
                        HashAlgorithmName.SHA256);

                    request.CertificateExtensions.Add(
                        new X509BasicConstraintsExtension(false, false, 0, false));

                    request.CertificateExtensions.Add(
                        new X509KeyUsageExtension(
                            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                            false));

                    var certificate = request.CreateSelfSigned(
                        DateTimeOffset.Now.AddDays(-1),
                        DateTimeOffset.Now.AddYears(1));

                    return certificate;
                }
            }
            catch
            {
                // Ultimate fallback - null certificate (proxy will work in HTTP-only mode)
                return null;
            }
        }

        private string GetCertificatePath()
        {
            // Try to use application data folder first
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var certDir = Path.Combine(appData, "WebTrafficInspector", CERT_FOLDER);
                return Path.Combine(certDir, CERT_FILE);
            }
            catch
            {
                // Fallback to temp folder
                var tempDir = Path.GetTempPath();
                var certDir = Path.Combine(tempDir, "WebTrafficInspector", CERT_FOLDER);
                return Path.Combine(certDir, CERT_FILE);
            }
        }

        /// <summary>
        /// Export certificate for user installation (optional)
        /// </summary>
        public bool ExportCertificateForInstallation(string outputPath)
        {
            try
            {
                var cert = GetOrCreateCertificate();
                if (cert == null) return false;

                var certBytes = cert.Export(X509ContentType.Cert);
                File.WriteAllBytes(outputPath, certBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if certificate is installed in trust store
        /// </summary>
        public bool IsCertificateInstalled()
        {
            try
            {
                var cert = GetOrCreateCertificate();
                if (cert == null) return false;

                using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        cert.Thumbprint,
                        false);
                    return certs.Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Install certificate in trust store (requires admin privileges)
        /// </summary>
        public bool InstallCertificate()
        {
            try
            {
                var cert = GetOrCreateCertificate();
                if (cert == null) return false;

                using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Uninstall certificate from trust store
        /// </summary>
        public bool UninstallCertificate()
        {
            try
            {
                var cert = GetOrCreateCertificate();
                if (cert == null) return false;

                using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    var certs = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        cert.Thumbprint,
                        false);

                    if (certs.Count > 0)
                    {
                        store.Remove(certs[0]);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
