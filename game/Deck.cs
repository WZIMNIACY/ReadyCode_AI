using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
// Removed dependency on the `hints` project to avoid circular project references.


namespace game
{

    /// <summary>
    /// Team assignments available for cards within a deck.
    /// </summary>
    public enum Team
    {
        /// <summary>Blue team card.</summary>
        Blue,
        /// <summary>Red team card.</summary>
        Red,
        /// <summary>Neutral card that scores no team.</summary>
        Neutral,
        /// <summary>Assassin card that ends the game.</summary>
        Assassin
    }

    /// <summary>
    /// Represents a deck of 25 cards used for gameplay.
    /// </summary>
    public sealed class Deck
    {
        /// <summary>
        /// All cards contained in this deck instance.
        /// </summary>
        public List<Card> Cards { get; init; } = new List<Card>();

        /// <summary>
        /// Team that starts the round and has the extra card.
        /// </summary>
        public Team StartingTeam { get; init; }

        private Deck()
        {
        }

        /// <summary>
        /// Creates a deck with predefined cards and starting team.
        /// </summary>
        /// <param name="cards">Collection of cards to include.</param>
        /// <param name="startingTeam">Team that begins play.</param>
        public Deck(List<Card> cards, Team startingTeam)
        {
            Cards = cards;
            StartingTeam = startingTeam;
        }

        /// <summary>
        /// Loads the embedded <c>WordVectorBase.txt</c> resource and converts it to a dictionary.
        /// </summary>
        /// <returns>Dictionary mapping words to vector embeddings.</returns>
        /// <exception cref="InvalidOperationException">Resource is missing or cannot be deserialized.</exception>
        public static Dictionary<string, List<double>> CreateDictionary()
        {
            var assembly = typeof(Deck).Assembly;
            const string resourceName = "game.WordVectorBase.txt";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
            if (stream == null)
            {
                throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
            }

            using (var reader = new StreamReader(stream))
            {
                string jsonContent = reader.ReadToEnd();
                Dictionary<string, List<double>> dictionary;

                try
                {
                    dictionary = JsonSerializer.Deserialize<Dictionary<string, List<double>>>(jsonContent) 
                        ?? throw new InvalidOperationException("Deserialized dictionary is null.");
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException("Failed to deserialize JSON content into a dictionary.", ex);
                }

                return dictionary;
            }
            }
        }
        

        /// <summary>
        /// Builds a randomized deck from a dictionary of word embeddings.
        /// </summary>
        /// <param name="dictionary">Word vectors to sample from; must contain at least 25 entries.</param>
        /// <param name="startingTeam">Team that takes the first turn.</param>
        /// <param name="rng">Optional random number generator for reproducibility.</param>
        /// <returns>New deck with shuffled cards and assignments.</returns>
        /// <exception cref="ArgumentNullException">Dictionary is null.</exception>
        /// <exception cref="ArgumentException">Dictionary contains fewer than 25 entries.</exception>
        public static Deck CreateFromDictionary(Dictionary<string, List<double>> dictionary, Team startingTeam, Random? rng = null)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.Count < 25)
            {
                throw new ArgumentException("Dictionary must contain at least 25 entries.", nameof(dictionary));
            }

            rng ??= new Random();

            // Take 25 unique random words
            var keys = dictionary.Keys.ToList();
            Shuffle(keys, rng);
            var selectedKeys = keys.Take(25).ToList();

            // Determine starting team: randomly Blue or Red
            var otherTeam = startingTeam == Team.Blue ? Team.Red : Team.Blue;

            const int totalCards = 25;
            const int assassinCount = 1;
            int startingTeamCount = 9;
            int otherTeamCount = 8;
            int neutralCount = totalCards - (startingTeamCount + otherTeamCount + assassinCount);

            // Build assignments and shuffle them
            var assignments = new List<Team>(totalCards);
            assignments.AddRange(Enumerable.Repeat(startingTeam, startingTeamCount));
            assignments.AddRange(Enumerable.Repeat(otherTeam, otherTeamCount));
            assignments.AddRange(Enumerable.Repeat(Team.Assassin, assassinCount));
            assignments.AddRange(Enumerable.Repeat(Team.Neutral, neutralCount));
            if (assignments.Count != totalCards)
            {
                throw new InvalidOperationException("Team assignment counts do not add up to 25.");
            }

            Shuffle(assignments, rng);

            // Create cards
            var cards = new List<Card>(totalCards);
            for (int i = 0; i < totalCards; i++)
            {
                var word = selectedKeys[i];
                // Copy vector to avoid external mutation
                var vector = new List<double>(dictionary[word]);
                var team = assignments[i];
                cards.Add(new Card(word, vector, team));
            }

            // Optionally shuffle final deck (cards already in random order due to assignments shuffle but shuffle again)
            Shuffle(cards, rng);

            return new Deck
            {
                Cards = cards,
                StartingTeam = startingTeam
            };
        }

        /// <summary>
        /// Serializes the deck to JSON including card contents.
        /// </summary>
        /// <returns>Compact JSON string.</returns>
        public string ToJson()
        {
            var options = CreateJsonOptions();
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Deserializes a deck from its JSON representation.
        /// </summary>
        /// <param name="json">Serialized deck.</param>
        /// <returns>Hydrated <see cref="Deck"/>.</returns>
        /// <exception cref="InvalidOperationException">Input is null/whitespace or cannot be parsed.</exception>
        public static Deck FromJson(string json)
        {


            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Input jsonFormat is null or whitespace.");

            Deck? deck;

            try
            {
                deck = JsonSerializer.Deserialize<Deck>(json);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize Deck from JSON.", ex);
            }

            if (deck == null)
                throw new InvalidOperationException("Deck did not deserialized properly!");
            else
                return deck;
        }

        /// <summary>
        /// Creates serializer options for deck JSON conversions.
        /// </summary>
        /// <returns>Configured <see cref="JsonSerializerOptions"/>.</returns>
        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return options;
        }

        // Fisher-Yates shuffle for IList<T>
        /// <summary>
        /// Performs an in-place Fisher–Yates shuffle on the provided list.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to shuffle.</param>
        /// <param name="rng">Random number generator.</param>
        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                if (j == i) continue;
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Renders the deck contents with card numbering.
        /// </summary>
        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            int i = 1;

            foreach (Card item in Cards)
            {
                sb.AppendLine("Card " + i + ":\n" + item + "\n");
                i++;
            }

            return sb.ToString();

        }
    }
}
