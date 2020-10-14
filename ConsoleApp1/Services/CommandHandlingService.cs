using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace KevinSpacey.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService commandService;
        private readonly DiscordSocketClient discordClient;
        private readonly IServiceProvider serviceProvider;

        public CommandHandlingService(CommandService commandService, DiscordSocketClient discordClient, IServiceProvider serviceProvider)
        {
            this.commandService = commandService;
            this.discordClient = discordClient;
            this.serviceProvider = serviceProvider;
            this.commandService.CommandExecuted += CommandExecutedAsync;
            this.discordClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            // This value holds the offset where the prefix ends
            var argPos = 0;
            // Perform prefix check. You may want to replace this with
            if (!message.HasCharPrefix('!', ref argPos) && !message.HasStringPrefix("- ", ref argPos)) return;
            // for a more traditional command format like !help.
            //if (!message.HasMentionPrefix(discordClient.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(discordClient, message);
            // Perform the execution of the command. In this method,
            // the command service will perform precondition and parsing check
            // then execute the command if one is matched.
            await commandService.ExecuteAsync(context, argPos, serviceProvider);
            // Note that normally a result will be returned by this format, but here
            // we will handle the result in CommandExecutedAsync,
        }
    }
}
