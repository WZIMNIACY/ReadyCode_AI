using AI;
using FileOperations; 

namespace testowanie
{
    public class LocalLLMTesting
    {
        public static async Task Test()
        {

            LocalLLM localLLM = new();

            string response = await localLLM.SendRequestAsync("Your nice man", "Hello how are u?");

            System.Console.WriteLine(response);
            

        }
    }
}