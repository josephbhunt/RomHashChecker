using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Xml.Linq;

namespace RomHashChecker
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string? file = args.Length > 0 ? args[0] : null;
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                MessageBox.Show("No file selected.");
                return;
            }

            // Compute MD5
            string md5str;
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(file))
            {
                var hash = md5.ComputeHash(stream);
                md5str = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            // Compute SHA1
            string sha1str;
            using (var sha1 = SHA1.Create())
            using (var stream = File.OpenRead(file))
            {
                var hash = sha1.ComputeHash(stream);
                sha1str = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            // Compute SHA256
            string sha256str;
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(file))
            {
                var hash = sha256.ComputeHash(stream);
                sha256str = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            // Query Hasheous API for each hash
            string md5Result = LookupHash("md5", md5str);

            MessageBox.Show(
                $"MD5: {md5str}\n\n" +
                $"SHA1: {sha1str}\n\n" +
                $"SHA256: {sha256str}\n\n" +
                $"Hasheous API result : {md5Result}",
                "Hash Results",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1
            );
        }

        private static string LookupHash(string hashType, string hashValue)
        {
            using var client = new HttpClient();
            try
            {
                var response = client.GetAsync($"https://hasheous.org/api/v1/Lookup/ByHash/{hashType}/{hashValue}").Result;
                if (!response.IsSuccessStatusCode)
                    return $"API Error: {response.StatusCode}";

                var json = response.Content.ReadAsStringAsync().Result;
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("name", out var nameProperty))
                {
                    return nameProperty.ToString();
                }
                else
                {
                    return "Name property not found!";
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}