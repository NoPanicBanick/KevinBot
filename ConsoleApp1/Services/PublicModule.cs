using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace KevinSpacey.Services
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly IMemoryCache _memoryCache;

        public PublicModule(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [Command("KevinsHeart")]
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

            IAudioClient audioClient;
            var cacheResult = _memoryCache.Get(channel.Id);
            if(cacheResult == null)
            {
                audioClient = await channel.ConnectAsync();
                _memoryCache.Set(channel.Id, audioClient);
            }
            else
            {
                audioClient = cacheResult as IAudioClient;
            }

            var path = "C:\\Users\\ryley\\Downloads\\Luigishot.mp3";

            var cancellationTokenSource = new CancellationTokenSource();
            string outputPath = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(path);

            IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                ?.SetCodec(AudioCodec.mp3);

            //await FFmpeg.Conversions.New().Streams

            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
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
