using System;

namespace Game.Services
{
    public class GeneratedQuestion
    {
        public string Text { get; set; } = string.Empty;
        public int Answer { get; set; }
    }

    public static class QuestionGenerator
    {
        private static readonly Random _random = new();

        public enum Difficulty { Easy, Medium, Hard }

        public static GeneratedQuestion Generate(Difficulty difficulty)
        {
            int a, b;
            switch (difficulty)
            {
                case Difficulty.Easy: // Addition & Subtraction (e.g. 14 + 27)
                    a = _random.Next(5, 25);
                    b = _random.Next(5, 25);
                    return _random.Next(2) == 0
                        ? new GeneratedQuestion { Text = $"Wat is {a} + {b}?", Answer = a + b }
                        : new GeneratedQuestion { Text = $"Wat is {a + b} - {a}?", Answer = b };

                case Difficulty.Medium: // Multiplication & Division (e.g. 8 × 7)
                    a = _random.Next(3, 11);
                    b = _random.Next(3, 11);
                    return _random.Next(2) == 0
                        ? new GeneratedQuestion { Text = $"Wat is {a} × {b}?", Answer = a * b }
                        : new GeneratedQuestion { Text = $"Wat is {a * b} ÷ {a}?", Answer = b };

                case Difficulty.Hard: // Mixed Operations (e.g. (12 + 8) × 3)
                    a = _random.Next(10, 30);
                    b = _random.Next(5, 20);
                    int c = _random.Next(2, 5);
                    return new GeneratedQuestion { Text = $"Wat is ({a} + {b}) × {c}?", Answer = (a + b) * c };

                default:
                    return new GeneratedQuestion { Text = "Wat is 1 + 1?", Answer = 2 };
            }
        }
    }
}