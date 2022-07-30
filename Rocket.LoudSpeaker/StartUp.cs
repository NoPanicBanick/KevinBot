using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KevinSpacey.Services;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace KevinSpacey
{
    //[StartUp]
    public class StartUp
    {
        public IServiceProvider services { get; set; }
        public void Configure()
        {
            var serviceBuilder = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json")
                .AddEnvironmentVariables();
            var configReader = configBuilder.Build();

            var config = configBuilder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions()
            {
                Client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                Vault = configReader["keyvaulturl"], 
                Manager = new DefaultKeyVaultSecretManager()
            });
            ConfigureServices(serviceBuilder, config.Build());
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var discordSocketConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            };

            services.AddSingleton(config);
            services.AddSingleton(new DiscordSocketClient(discordSocketConfig));
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddMemoryCache();

            // Add service provider for discord commands
            var servs = services.BuildServiceProvider();
            services.AddSingleton<IServiceProvider>(servs);
            services = ConfigureLogger(services, config);
            this.services = services.BuildServiceProvider();
        }

        private IServiceCollection ConfigureLogger(IServiceCollection services, IConfiguration config)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsights(config["APPINSIGHTS_INSTRUMENTATIONKEY"], TelemetryConverter.Events)
                .WriteTo.Console().CreateLogger();
            return services.AddSingleton<ILogger>(logger);
        }
    }
}
