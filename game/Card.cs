using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace game
{
    /// <summary>
    /// Represents a single word card with optional embedding vector and team ownership.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Word printed on the card.
        /// </summary>
        public string Word { get; init; }

        /// <summary>
        /// Optional semantic vector associated with the word.
        /// </summary>
        public List<double>? Vector { get; init; }

        /// <summary>
        /// Team assignment for this card.
        /// </summary>
        public Team Team { get; init; }

        /// <summary>
        /// Initializes a new card instance.
        /// </summary>
        /// <param name="word">Word label; cannot be null.</param>
        /// <param name="vector">Optional embedding vector.</param>
        /// <param name="team">Owning team for the card.</param>
        public Card(string word, List<double>? vector, Team team)
        {
            Word = word ?? throw new ArgumentNullException(nameof(word));
            Vector = vector;
            Team = team;
        }

        /// <summary>
        /// Formats full card details including the vector when present.
        /// </summary>
        /// <returns>Human-readable description of the card.</returns>
        public string fullWordInfo()
        {
            StringBuilder sb = new StringBuilder();

            if (Vector != null)
            {
                foreach (double item in Vector)
                {
                    sb.AppendLine(item.ToString());
                }
            }

            return $"Word: {Word}\nVector: {sb.ToString()}\nTeam: {Team}";
        }

        /// <summary>
        /// Returns a short representation without the vector contents.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Vector != null)
            {
                foreach (double item in Vector)
                {
                    sb.AppendLine(item.ToString());
                }

            }

            return $"Word: {Word}\nTeam: {Team.ToString()}";
        }

        /// <summary>
        /// Serializes the card to JSON including the team enumeration.
        /// </summary>
        /// <returns>JSON string representing the card.</returns>
        public string toJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)

            });
        }

        /// <summary>
        /// Deserializes a JSON representation of a card.
        /// </summary>
        /// <param name="json">Serialized card JSON.</param>
        /// <returns>Hydrated <see cref="Card"/> instance.</returns>
        public static Card fromJson(string json)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                    WriteIndented = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    Converters = { new JsonStringEnumConverter() }
            };

            return System.Text.Json.JsonSerializer.Deserialize<Card>(json, options)!;
        }

        /// <summary>
        /// Determines equality based on word and team hash codes.
        /// </summary>
        public override bool Equals(object? obj)
        {
            Card? card = obj as Card;
            if (card == null)
                return false;

            return this.GetHashCode() == card.GetHashCode();

            
        }

                    /// <summary>
                    /// Combines word and team hashes to uniquely identify a card.
                    /// </summary>
        public override int GetHashCode()
        {
            return Word.GetHashCode() + Team.GetHashCode();
        }

        
    }
}
