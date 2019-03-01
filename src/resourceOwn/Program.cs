using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Common.StringExtensions;

namespace resourceOwn
{
    class Program
    {
        static string baseUrl(string tenant) => $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";

        static string esc(string s) => Uri.EscapeDataString(s); 
        static string queryV2(
            string clientId,
            string clientSecret,
            string username, 
            string password,
            string scopes) =>
            string.Join("&", new[]
        {
            $"client_id={esc(clientId)}",
            $"client_secret={esc(clientSecret)}",
            $"username={esc(username)}",
            $"grant_type=password",
            $"password={esc(password)}",
            $"scope={esc(scopes)}"
        });

        static String queryV1(
            string resourceId,
            string clientId,
            string username, 
            string password,
            string scopes) =>
            string.Join("&", new[]
        {
            $"resource={esc(resourceId)}",
            $"client_id={esc(clientId)}",
            $"username={esc(username)}",
            $"grant_type=password",
            $"password={esc(password)}",
            $"scope={esc(scopes)}"
        });

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

        static bool ValidArgs(string[] args)
        {
            bool hasFirst() => args.Length > 1;
            bool rightCount() {
                switch (args[1])
                {
                    case "V1":
                    case "V2":
                        return args.Length == 7 + 1;
                    default:
                        return false;
                }
            }

            return hasFirst() && rightCount();
        }
        static async Task<int> Main(string[] args)
        {
            if (!ValidArgs(args))
            {
                Console.WriteLine($"{string.Join(" ", args)}");
                Console.Error.WriteLine(
@"usage: resourceOwn version [Args]
version must be either V1 or V2. If it is V1 six arguments must follow:
- tenantId
- resourceId
- clientId
- userName
- passWord
- scope (space separated)
If it is V2 six arguments must follow:
- tenantId
- clientId
- clientSecret
- userName
- password
- scope");
                return 1;
            }

            string query;
            if (args[1] == "V1")
            {
                query = queryV1(args[3],args[4], args[5], args[6], args[7]);
            }
            else
            {
                query = queryV2(args[3],args[4], args[5], args[6], args[7]);
            }

            var baseurl = baseUrl(args[2]); 
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

            Console.Write(output);
            return 0;
        }
    }
}
