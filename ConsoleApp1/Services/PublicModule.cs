using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace KevinSpacey.Services
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {

        [Command("KevinsHeart")]
        public Task PingAsync()
            => ReplyAsync("My Heart is Stilling Beating!");


        [Command("play", RunMode = RunMode.Async)]
        [Alias("stop", "skip")]
        public async Task CleanUpBotCommands()
        {
            await Context.Message.AddReactionAsync(new Emoji("🔥"));
            await Task.Delay(1 * 60 * 1000);
            await Context.Message.DeleteAsync();
        }     
    }
}
