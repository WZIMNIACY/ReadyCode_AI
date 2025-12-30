using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using game;
using hints;
using AI;


namespace testowanie
{
    internal static class HintTesting
    {
        public static async Task Test()
        {
            //### Hint Generating ###//
            string vectorsPath = Path.Combine(Directory.GetCurrentDirectory(), "WordVectorBase.txt");
            string data = File.ReadAllText(vectorsPath);

            //Creating llm connection
            string baseDir = Directory.GetCurrentDirectory();
            var parentDirInfo = Directory.GetParent(baseDir);
            if (parentDirInfo == null)
                throw new Exception("Cannot locate parent directory for apiKey.txt.");
            string apiKeyPath = Path.Combine(parentDirInfo.FullName, "apiKey.txt");
            string apiKey = File.ReadAllText(apiKeyPath).Trim();

            //LLM llm = new(apiKey);
            DeepSeekLLM llm = new(apiKey);

            //Creating deck
            Dictionary<string, List<double>>? words = JsonSerializer.Deserialize<Dictionary<string, List<double>>>(data, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            Deck? deck;

            if (words != null)
                deck = Deck.CreateFromDictionary(words);
            else
                throw new Exception("Dictionary deserialization failed.");



            Hint hint = Hint.Create(deck, llm, Team.Red);

            System.Console.WriteLine(hint.toJson());




        }
    }
}