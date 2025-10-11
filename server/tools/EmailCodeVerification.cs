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
            bool canRequest = await ctx.Users.AnyAsync(u => u.Email == emailAddress);
            string code = string.Empty;
            Random random = new Random();
            if (!canRequest)
            {
                var existingRequest = await ctx.Requests.FirstOrDefaultAsync(u => u.EmailAddress == emailAddress);
                for (int i = 0; i < length; i++)
                {
                    code += AllowedEmailLocalChars[random.Next(AllowedEmailLocalChars.Count)];
                }
                if (existingRequest != null)
                {
                    existingRequest.Code = code;
                    existingRequest.CreatedOn = DateTime.Now;
                }
                else
                {
                    await ctx.Requests.AddAsync(new NATRequests
                    {
                        EmailAddress = emailAddress,
                        Code = code
                    });
                }
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
                Console.WriteLine($"{code}, {request.Code}");
            }
            else
            {
                Console.WriteLine($"{code}, request not found.");
            }
            return output;
        }
        public string GenerateBody(string code)
        {
            string output = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <title>Zurus06.NAT Email Verification</title>
            </head>
            <body style='
                margin:0;
                padding:0;
                font-family:Arial, Helvetica, sans-serif;
                background-color:#ffffff;
                color:#000000;
            '>
                <div style='
                    max-width:600px;
                    margin:40px auto;
                    border:1px solid #000000;
                    padding:30px;
                    text-align:center;
                '>
                    <h2 style='margin-bottom:10px;'>Zurus06.NAT</h2>
                    <p style='font-size:15px; margin-bottom:25px;'>Use the code below to log in to your account:</p>

                    <div style='
                        display:inline-block;
                        border:2px solid #000000;
                        padding:12px 25px;
                        font-size:24px;
                        font-weight:bold;
                        letter-spacing:6px;
                        background-color:#f9f9f9;
                        margin-bottom:25px;
                    '>{code}</div>

                    <p style='font-size:14px; margin-top:10px;'>This code will expire shortly. Please use it soon.</p>

                    <hr style='margin:30px 0; border:0; border-top:1px solid #000000;'>

                    <p style='font-size:13px; color:#333;'>Need help? Contact our 
                        <a href='#' style='color:#000; text-decoration:underline;'>support team</a>.
                    </p>
                    <p style='font-size:12px; color:#555;'>&copy; {DateTime.Now.Year} Zurus06.NAT. All rights reserved.</p>
                </div>
            </body>
            </html>";
            return output;
        }
    }
}