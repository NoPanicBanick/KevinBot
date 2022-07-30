using Discord.WebSocket;
using System.Threading.Tasks;

namespace Rocket.LoudSpeaker.Services
{
    public interface IAudioService
    {
        public Task PlayAudioAsync(ISocketAudioChannel channel, string filePath);
    }
}