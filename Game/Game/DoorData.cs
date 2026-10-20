using Game.Services;

namespace Game.Models
{
    public class DoorQuestion
    {
        public int DoorTileId { get; set; }
        public string Question { get; set; } = string.Empty;
        public int CorrectAnswer { get; set; }
        public bool IsUnlocked { get; set; }

        public static DoorQuestion CreateRandom(int doorTileId, QuestionGenerator.Difficulty difficulty)
        {
            var generated = QuestionGenerator.Generate(difficulty);
            return new DoorQuestion
            {
                DoorTileId = doorTileId,
                Question = generated.Text,
                CorrectAnswer = generated.Answer,
                IsUnlocked = false
            };
        }
    }
}