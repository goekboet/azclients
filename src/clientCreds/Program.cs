using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.StringExtensions;

namespace clientCreds
{
    class Program
    {
        static string esc(string s) => Uri.EscapeDataString(s);
        static string baseUrl(string tenant) => $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";

        static Task<HttpResponseMessage> Auth(
            HttpClient c,
            string baseurl,
            string query) =>
            c.PostAsync(
                baseurl,
                new StringContent(
                    query,
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"));

        static string queryV2(
            string clientId,
            string clientSecret,
            string scopes) =>
            string.Join("&", new[]
        {
            $"client_id={esc(clientId)}",
            $"client_secret={esc(clientSecret)}",
            $"grant_type=client_credentials",
            $"scope={esc(scopes)}"
        });
        
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine($"{string.Join(" ", args)}");
                Console.WriteLine(
@"
useage:
- tenantId
- clientId
- client_secret
- scope
");
                return 1;
            }

            var baseurl = baseUrl(args[1]);
            var query = queryV2(args[2], args[3], args[4]);
            var output = "";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                try
                {
                    using (var response = await Auth(client, baseurl, query))
                    {
                        var status = $"{response.StatusCode} {response.ReasonPhrase}";
                        var content = await response.Content.ReadAsStringAsync();
                        output = content.PrettyPrint();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Exception: {e.Message}");
                    return 1;
                }
            }

            Console.WriteLine(output);
            return 0;
        }
    }
}
