using Discord.Commands;
using Discord.WebSocket;
using Rocket.LoudSpeaker.Services;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rocket.LoudSpeaker
{
    /// <summary>
    /// Controller level of sorts for Text Commands
    /// </summary>
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly IAudioService _audioService;

        public PublicModule(IAudioService audioService)
        {
            _audioService = audioService;
        }

        /// <summary>
        /// Simple ping
        /// </summary>
        /// <returns></returns>
        [Command("HeartBeat")]
        public Task PingAsync()
            => ReplyAsync("My Heart is Stilling Beating!");

        [Command("play", true, RunMode = RunMode.Async)]
        [Alias("stop", "skip")]
        public async Task CleanUpBotCommands()
        {
            ISocketAudioChannel channel;
            if (Context.User is SocketWebhookUser socketUser)
            {
                channel = Context.Guild.VoiceChannels.Where(x => x.Name == "R-n-R").First();
            }
            else
            {
                SocketGuildUser user = Context.User as SocketGuildUser;
                if (user.VoiceState is null)
                {
                    await Context.Channel.SendMessageAsync("Cannot join a target channel if you don't give me a target dipshit");
                    return;
                }
                channel = user.VoiceChannel;
            }

            var path = "C:\\Users\\ryley\\Downloads\\LuigiThemeClipped.mp3";
            await _audioService.PlayAudioAsync(channel, path);
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }
    }
}
