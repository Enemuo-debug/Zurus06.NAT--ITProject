using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace server.tools
{
    public class EmailCodeVerification
    {
        public readonly IConfiguration config;
        public EmailCodeVerification(IConfiguration _config)
        {
            config = _config;
        }
        private List<char> AllowedEmailLocalChars = new List<char>()
        {
            // Letters
            'a','b','c','d','e','f','g','h','i','j','k','l','m',
            'n','o','p','q','r','s','t','u','v','w','x','y','z',

            // Digits
            '0','1','2','3','4','5','6','7','8','9',

            // Special characters (commonly allowed)
            '.','_','-','+'
        };

        public string CreateCode(string emailAddress, bool fromAPI = false)
        {
            string user = string.Join("", emailAddress.Split("@"));
            string salt = "";
            Random random = new();

            for (int i = 0; i < 30; i++)
            {
                salt += AllowedEmailLocalChars[random.Next(AllowedEmailLocalChars.Count)];
            }

            string text = File.ReadAllText("cache.txt");

            // Split into entries
            List<string> parts = text.Split('@').ToList();

            // Loop through and remove the matching one
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (fromAPI && parts[i].Split(':')[0] == user)
                {
                    salt = parts[i].Split(':')[1];
                    break;
                }
                if (parts[i].Split(':')[0] == user)
                {
                    parts.RemoveAt(i);
                }
            }
            text = string.Join("@", parts);
            File.WriteAllText("cache.txt", text);
            if (!fromAPI) File.AppendAllText("cache.txt", $"{user}:{salt}@");

            user += salt;
            Console.WriteLine(user);

            BigInteger code = 1;

            foreach (char c in user)
            {
                if (AllowedEmailLocalChars.Contains(char.ToLower(c)))
                {
                    int idx = AllowedEmailLocalChars.IndexOf(char.ToLower(c));
                    code *= AllowedEmailLocalChars.Count - idx;
                    Console.Write(code + " ");
                    Console.WriteLine(idx);
                }
            }

            // Turn BigInteger into base-N string using AllowedEmailLocalChars
            string output = "";
            BigInteger n = code;
            int baseN = AllowedEmailLocalChars.Count;

            if (n == 0) return AllowedEmailLocalChars[0].ToString();

            while (n > 0)
            {
                int remainder = (int)(n % baseN);
                output = AllowedEmailLocalChars[remainder] + output;
                n /= baseN;
            }

            Console.WriteLine(output);
            return output;
        }

        public bool VerifyCode(string emailAddress, string code)
        {
            string user = string.Join("", emailAddress.Split("@"));
            string text = File.ReadAllText("cache.txt");
            List<string> parts = text.Split('@').ToList();

            for (int i = parts.Count - 1; i >= 0; i--)
            {
                var salt = parts[i].Split(':');
                if (salt[0] == user)
                {
                    user += salt[1];
                    parts.RemoveAt(i);
                    return code == CreateCode(emailAddress, true).Substring(0, int.Parse(config["Secrets:codeLength"] ?? "10"));
                }
            }
            return false;
        }
    }
}