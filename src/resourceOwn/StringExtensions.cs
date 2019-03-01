using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace resourceOwn.StringExtensions
{
    public static class StringExtensions
    {
        public static string PrettyPrint(this string json) => JToken.Parse(json).ToString();
        
        public static string Decode(this string s) => 
            PrettyPrint(
                Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        FromUrlBase64(s))));

        public static string DecodeToken(this string token) =>
            string.Join(Environment.NewLine, token.Split('.').Take(2).Select(Decode));

        public static string FromUrlBase64(this string s)
        {
            var n = s.Length % 4;
            var padding = new String('=', n);
            
            return $"{s.Replace('-', '+').Replace('_', '/')}{padding}";
        }
    }
}