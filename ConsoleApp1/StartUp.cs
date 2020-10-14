using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KevinSpacey.Services;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
                .AddJsonFile("C:\\Users\\ryley\\source\\repos\\KevinSpacey\\ConsoleApp1\\local.settings.json")
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
            services.AddSingleton(config);
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();

            // Add service provider for discord commands
            var servs = services.BuildServiceProvider();
            services.AddSingleton<IServiceProvider>(servs);
            this.services = services.BuildServiceProvider();
        }
    }
}
