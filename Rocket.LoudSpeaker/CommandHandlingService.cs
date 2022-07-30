using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Rocket.LoudSpeaker
{
    public class CommandHandlingService
    {
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _discordClient;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandlingService(CommandService commandService, DiscordSocketClient discordClient, IServiceProvider serviceProvider)
        {
            _commandService = commandService;
            _discordClient = discordClient;
            _serviceProvider = serviceProvider;
            _commandService.CommandExecuted += CommandExecutedAsync;
            _discordClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        /// <summary>
        /// Executes when a message is posted with the prefix
        /// </summary>
        /// <param name="command"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // Prefix was used but no command exists
            if (!command.IsSpecified)
                return;

            // Successful path
            if (result.IsSuccess)
                return;

            // Boo Sad Path
            await context.Channel.SendMessageAsync($"error: {result}");
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            //if (message.Source != MessageSource.User) return;

            // This value holds the offset where the prefix ends
            var argPos = 0;
            // Perform prefix check. You may want to replace this with
            if (!message.HasCharPrefix('!', ref argPos) && !message.HasStringPrefix("- ", ref argPos)) return;
            // for a more traditional command format like !help.
            //if (!message.HasMentionPrefix(discordClient.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_discordClient, message);
            // Perform the execution of the command. In this method,
            // the command service will perform precondition and parsing check
            // then execute the command if one is matched.
            await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
            // Note that normally a result will be returned by this format, but here
            // we will handle the result in CommandExecutedAsync,
        }
    }
}
