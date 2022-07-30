using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rocket.LoudSpeaker.Services
{
    public class FFMPEGAudioService : IAudioService
    {
        private readonly IMemoryCache _memoryCache;

        public FFMPEGAudioService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task PlayAudioAsync(ISocketAudioChannel channel, string filePath)
        {
            IAudioClient audioClient;
            var cacheResult = _memoryCache.Get(channel.Id);
            if (cacheResult == null)
            {
                audioClient = await channel.ConnectAsync();
                _memoryCache.Set(channel.Id, audioClient);
            }
            else
            {
                audioClient = cacheResult as IAudioClient;
            }

            using (var ffmpeg = CreateStream(filePath))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        private static Process CreateStream(string path)
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
