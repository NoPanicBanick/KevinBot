using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KevinSpacey;
using KevinSpacey.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var startUp = new StartUp();
            startUp.Configure();

            var config = startUp.services.GetRequiredService<IConfiguration>();
            var client = startUp.services.GetRequiredService<DiscordSocketClient>();
            var logger = startUp.services.GetRequiredService<ILogger>();
            logger.Information("Kevin has started up");
            client.Log += LogAsync;

            startUp.services.GetRequiredService<CommandService>().Log += LogAsync;

            // Tokens should be considered secret data and never hard-coded.
            // We can read from the environment variable to avoid hardcoding.
            await client.LoginAsync(TokenType.Bot, config["Discord:Token"]);
            await client.StartAsync();

            // Here we initialize the logic required to register our commands.
            await startUp.services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }
    }
}
