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

            using var md5 = MD5.Create();
            using var stream = File.OpenRead(file);
            var hash = md5.ComputeHash(stream);
            var md5str = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            string nameFromHasheous = LookupHash(md5str);

            MessageBox.Show(
                $"MD5: {md5str}\n\nHasheous API result: {nameFromHasheous}",
                "Hash Result",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1
            );
        }

        private static string LookupHash(string md5str)
        {
            using var client = new HttpClient();
            try
            {
                var response = client.GetAsync($"https://hasheous.org/api/v1/Lookup/ByHash/md5/{md5str}").Result;
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