using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using resourceOwn.StringExtensions;

namespace resourceOwn
{
    class Program
    {
        const string tokenEndpoint = "https://login.microsoftonline.com/c4407ff9-b5f0-4ce7-9696-53c8758fcc25/oauth2/v2.0/token";

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
            string query) =>
            c.PostAsync(
                tokenEndpoint,
                new StringContent(
                    query,
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"));

        static bool ValidArgs(string[] args)
        {
            bool hasFirst() => args.Length > 1;
            bool rightCount() {
                switch (args[0])
                {
                    case "V1":
                    case "V2":
                        return args.Length == 5 + 1;
                    default:
                        return false;
                }
            }

            return hasFirst() && rightCount();
        }
        static async Task<int> Main(string[] args)
        {
            //Console.WriteLine(string.Join(" ", args));

            if (!ValidArgs(args))
            {
                Console.Error.WriteLine(
@"usage: resourceOwn version [Args]
version must be either V1 or V2. If it is V1 five arguments must follow:
- resourceId
- clientId
- userName
- passWord
- scope (space separated)
If it is V2 five arguments must follow:
- clientId
- clientSecret
- userName
- password
- scope");
                return 1;
            }

            string query;
            if (args[0] == "V1")
            {
                query = queryV1(args[1],args[2], args[3], args[4], args[5]);
            }
            else
            {
                query = queryV2(args[1],args[2], args[3], args[4], args[5]);
            } 
            var output = "";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                try
                {
                    using (var response = await Auth(client, query))
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
