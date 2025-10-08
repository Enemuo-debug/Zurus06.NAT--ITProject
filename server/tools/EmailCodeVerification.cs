using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.data;
using server.NATModels;

namespace server.tools
{
    public class EmailCodeVerification
    {
        public readonly IConfiguration config;
        private readonly ApplicationDbContext ctx;
        public EmailCodeVerification(IConfiguration _config, ApplicationDbContext context)
        {
            config = _config;
            ctx = context;
        }
        private List<char> AllowedEmailLocalChars = new List<char>()
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m',
            'n','o','p','q','r','s','t','u','v','w','x','y','z',
        };

        public async Task<string?> CreateCode(string emailAddress, int length = 5)
        {
            bool canRequest = ctx.Users.Any(u => u.Email == emailAddress) || ctx.Requests.Any(u => u.EmailAddress == emailAddress);
            string code = string.Empty;
            Console.WriteLine(canRequest);
            Random random = new Random();
            if (!canRequest)
            {
                for (int i = 0; i < length; i++)
                {
                    code += AllowedEmailLocalChars[random.Next(AllowedEmailLocalChars.Count)];
                }
                await ctx.Requests.AddAsync(new NATRequests
                {
                    EmailAddress = emailAddress,
                    Code = code
                });
                await ctx.SaveChangesAsync();
            }
            return code;
        }

        public async Task<bool> VerifyCode(string emailAddress, string code)
        {
            bool output = false;
            var request = await ctx.Requests.FirstOrDefaultAsync(r => r.EmailAddress == emailAddress && r.Code == code);
            if (request != null)
            {
                output = DateTime.Now < request.CreatedOn.AddMinutes(30);
                ctx.Requests.Remove(request);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"{code}, {request.Code}");
            }
            else
            {
                Console.WriteLine($"{code}, request not found.");
            }
            return output;
        }

    }
}