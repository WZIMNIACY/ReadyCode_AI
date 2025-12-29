using AI;
using FileOperations; 

namespace testowanie
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
            string apiKey = FileOp.Read(apiKeyPath).Trim();

            DeepSeekLLM llm = new(apiKey);

            string response = await llm.SendRequestAsync("Your nice man", "Hello how are u?");

            System.Console.WriteLine(response);
            

        }
    }
}