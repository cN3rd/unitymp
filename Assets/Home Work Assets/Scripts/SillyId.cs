using System;
using System.Collections.Generic;
using System.Linq;

namespace HW1.Scripts
{
    public class SillyId
    {
        private static readonly Dictionary<string, string[]> Dictionaries = new()
        {
            ["adj"] = new[]
            {
                // Color adjectives
                "red", "blue", "green", "yellow", "orange", "purple", "pink", "black", "white", "gray",
                "crimson", "azure", "emerald", "golden", "violet", "magenta", "turquoise", "silver",

                // Size adjectives
                "tiny", "small", "large", "huge", "giant", "massive", "enormous", "mini", "mega",
                "colossal",

                // Personality adjectives
                "silly", "funny", "crazy", "wild", "calm", "brave", "shy", "bold", "clever", "wise",
                "sneaky", "fierce", "gentle", "proud", "humble", "cheerful", "grumpy", "sleepy",

                // Gaming adjectives
                "epic", "legendary", "elite", "pro", "supreme", "ultimate", "super", "hyper", "ultra",
                "shadow", "dark", "bright", "swift", "quick", "slow", "silent", "loud", "invisible",

                // Physical adjectives
                "hairy", "smooth", "rough", "soft", "hard", "sharp", "blunt", "shiny", "dull",
                "sparkly", "fluffy", "spiky", "bumpy", "slippery", "sticky", "elastic", "frozen",
                "burning",

                // Emotional adjectives
                "happy", "sad", "angry", "excited", "bored", "surprised", "confused", "relaxed",
                "nervous", "confident", "jealous", "curious", "mysterious", "magical", "enchanted",
                "cursed",

                // Weather adjectives
                "sunny", "cloudy", "rainy", "snowy", "stormy", "windy", "foggy", "misty", "frosty",
                "icy"
            },
            ["noun"] = new[]
            {
                // Animals
                "cat", "dog", "rabbit", "hamster", "bird", "fish", "turtle", "lizard", "snake", "frog",
                "lion", "tiger", "bear", "wolf", "fox", "deer", "elephant", "giraffe", "zebra", "hippo",
                "monkey", "gorilla", "panda", "koala", "kangaroo", "penguin", "owl", "eagle", "hawk",
                "dove", "shark", "whale", "dolphin", "octopus", "crab", "lobster", "jellyfish",
                "starfish", "dragon", "unicorn", "phoenix", "griffin", "pegasus", "centaur", "fairy",
                "goblin",

                // Gaming creatures
                "robot", "cyborg", "android", "ninja", "warrior", "wizard", "knight", "archer", "mage",
                "paladin", "rogue", "assassin", "hunter", "ranger", "barbarian", "monk", "bard",
                "pirate", "viking", "samurai", "gladiator", "spartans", "guardians", "champions",

                // Objects
                "sword", "shield", "bow", "staff", "wand", "hammer", "axe", "dagger", "spear", "mace",
                "helmet", "armor", "boots", "gloves", "cape", "crown", "ring", "amulet", "potion",
                "crystal", "gem", "stone", "rock", "mountain", "volcano", "river", "ocean", "forest",
                "castle", "tower", "bridge", "gate", "door", "window", "mirror", "book", "scroll",

                // Technology
                "computer", "laptop", "phone", "tablet", "camera", "headset", "keyboard", "mouse",
                "monitor", "speaker", "microphone", "drone", "satellite", "spaceship", "rocket",

                // Food items
                "pizza", "burger", "taco", "sandwich", "cookie", "cake", "donut", "apple", "banana",
                "orange", "grape", "strawberry", "chocolate", "candy", "ice cream", "coffee", "tea"
            }
        };

        private readonly bool _caps;

        private readonly List<WordOrder> _order;
        private readonly Random _random;
        private readonly string _spacer;

        // Default constructor - generates names like "HairyOrangeGeckos"
        public SillyId() : this(
            new List<WordOrder>
            {
                new() { Type = "adj" }, new() { Type = "adj" }, new() { Type = "noun" }
            })
        {
        }

        // Custom constructor
        public SillyId(List<WordOrder> order, string spacer = "", bool caps = true)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));
            _spacer = spacer ?? "";
            _caps = caps;
            _random = new Random();
        }

        public string Generate()
        {
            var words = new List<string>();

            foreach (WordOrder orderItem in _order)
            {
                string word = GetRandomWord(orderItem.Type, orderItem.Letter);
                words.Add(_caps ? Capitalize(word) : word);
            }

            return string.Join(_spacer, words);
        }

        private string GetRandomWord(string type, char? startingLetter)
        {
            if (!Dictionaries.ContainsKey(type))
                throw new ArgumentException($"Unknown word type: {type}");

            string[] availableWords = Dictionaries[type];

            if (startingLetter.HasValue)
            {
                string[] filteredWords = availableWords
                    .Where(word => word.Length > 0 &&
                                   char.ToLower(word[0]) == char.ToLower(startingLetter.Value))
                    .ToArray();

                if (filteredWords.Length == 0)
                {
                    // Fallback to any word if no words start with the specified letter
                    availableWords = Dictionaries[type];
                }
                else
                {
                    availableWords = filteredWords;
                }
            }

            return availableWords[_random.Next(availableWords.Length)];
        }

        private static string Capitalize(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            return char.ToUpper(word[0]) + word.Substring(1).ToLower();
        }

        // Convenience methods for common patterns
        public static string GenerateGamertag()
        {
            SillyId generator = new();
            return generator.Generate();
        }

        public static string GenerateWithDashes()
        {
            SillyId generator = new(
                new List<WordOrder>
                {
                    new() { Type = "adj" }, new() { Type = "adj" }, new() { Type = "noun" }
                },
                "-",
                false
            );

            return generator.Generate();
        }

        public static string GenerateWithUnderscores()
        {
            SillyId generator = new(
                new List<WordOrder> { new() { Type = "adj" }, new() { Type = "noun" } },
                "_"
            );

            return generator.Generate();
        }

        // Generate with specific starting letters (like the original example)
        public static string GenerateCustomPattern()
        {
            var order = new List<WordOrder>
            {
                new() { Type = "adj", Letter = 'n' }, // naked
                new() { Type = "adj", Letter = 'p' }, // purple  
                new() { Type = "noun", Letter = 'm' } // monkey
            };

            SillyId generator = new(order, "-", false);
            return generator.Generate(); // => "naked-purple-monkey"
        }

        public class WordOrder
        {
            public string Type { get; set; } // "adj" or "noun"
            public char? Letter { get; set; } // specific starting letter, or null for any
        }
    }

    // Usage examples class
    public static class GamertagGenerator
    {
        public static string GenerateRandomGamertag()
        {
            // Randomly choose between different formats
            Random random = new();
            int format = random.Next(4);

            return format switch
            {
                0 => SillyId.GenerateGamertag(), // "HairyOrangeGeckos"
                1 => SillyId.GenerateWithDashes(), // "silly-blue-robot"
                2 => SillyId.GenerateWithUnderscores(), // "Epic_Dragon"
                3 => GenerateWithNumbers(), // "SwiftWolf123"
                _ => SillyId.GenerateGamertag()
            };
        }

        private static string GenerateWithNumbers()
        {
            string baseName = SillyId.GenerateGamertag();
            int number = new Random().Next(10, 1000);
            return $"{baseName}{number}";
        }
    }
}
