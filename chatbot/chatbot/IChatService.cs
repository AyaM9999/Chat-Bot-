using chatbot.ChatDTO;
using System.Threading.Tasks;
namespace chatbot
{
    

    namespace Sanayii.Core.Interfaces
    {
        public interface IChatService
        {
            Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request);
        }
    }

}
