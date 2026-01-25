using System.Threading.Tasks;

namespace AI
{
    /// <summary>
    /// Abstraction for language model clients capable of chat completion.
    /// </summary>
    public interface ILLM
    {
        /// <summary>
        /// Sends a request to the language model and returns generated content.
        /// </summary>
        /// <param name="systemPrompt">System-level instructions.</param>
        /// <param name="userPrompt">User message content.</param>
        /// <param name="maxTokens">Maximum tokens to generate.</param>
        /// <returns>Response text from the model.</returns>
        Task<string> SendRequestAsync(string systemPrompt, string userPrompt, uint maxTokens = 256);
    }
}
