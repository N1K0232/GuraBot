using GuraDiscordBot.Core;
using System.Text;

namespace GuraDiscordBot;

public class Program
{
    public async static Task Main()
    {
        string encodedAccessToken = Environment.GetEnvironmentVariable("AccessToken");
        string accessToken = Encoding.UTF8.GetString(Convert.FromBase64String(encodedAccessToken));
        int permissionsInteger = int.Parse(Environment.GetEnvironmentVariable("PermissionsInteger"));
        GuraBot guraBot = new(accessToken, permissionsInteger);
        await guraBot.RunAsync();
        Console.Write("Press a key to continue . . . ");
        Console.ReadLine();
    }
}