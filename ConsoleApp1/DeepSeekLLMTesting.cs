using AI;

namespace Program
{
    public class DeepSeekLLMTesting
    {
        public static async Task Test()
        {

            string baseDir = Directory.GetCurrentDirectory();
            var parentDirInfo = Directory.GetParent(baseDir);
            if (parentDirInfo == null)
                throw new Exception("Cannot locate parent directory for apiKey.txt.");
            string apiKeyPath = Path.Combine(parentDirInfo.FullName, "apiKey.txt");
            string apiKey = File.ReadAllText(apiKeyPath).Trim();

            DeepSeekLLM llm = new(apiKey);

            string response = await llm.SendRequestAsync("Your nice man", "Hello how are u, and how much is 2+2?");

            System.Console.WriteLine(response);
            

        }
    }
}