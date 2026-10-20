using System.Collections.Generic;
using Game.Services;

namespace Game.Models
{
    public class TileType
    {
        public int Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public bool IsSolid { get; set; }
    }

    public class LevelMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<int, TileType> TileRegistry { get; set; } = new();
        public Dictionary<int, DoorQuestion> DoorQuestions { get; set; } = new();
        public int[,] Layout { get; set; } = new int[0, 0];

        // Register a door dynamically by tile ID
        public void RegisterDoor(int doorTileId, QuestionGenerator.Difficulty difficulty = QuestionGenerator.Difficulty.Easy)
        {
            if (!DoorQuestions.ContainsKey(doorTileId))
            {
                DoorQuestions[doorTileId] = DoorQuestion.CreateRandom(doorTileId, difficulty);
            }
        }

        public static LevelMap GetSchoolLevel()
        {
            var registry = new Dictionary<int, TileType>
            {
                { 0, new TileType { Id = 0, AssetName = "Floor.png", IsSolid = false } },
                { 1, new TileType { Id = 1, AssetName = "LeftWall.png", IsSolid = true } },
                { 2, new TileType { Id = 2, AssetName = "RightWall.png", IsSolid = true } },
                { 3, new TileType { Id = 3, AssetName = "BackWall1.png", IsSolid = true } },
                { 4, new TileType { Id = 4, AssetName = "BackWall2.png", IsSolid = true } },
                { 5, new TileType { Id = 5, AssetName = "FrontWall.png", IsSolid = true } },
                { 6, new TileType { Id = 6, AssetName = "Door.png", IsSolid = true } },
                { 7, new TileType { Id = 7, AssetName = "Door.png", IsSolid = true } },
                { 100, new TileType { Id = 100, AssetName = "Door.png", IsSolid = true } } 
            };

            int[,] layout = new int[,]
            {
                { 3, 4, 3, 4, 3, 1, 0, 0, 0, 2, 3, 4, 3, 4, 3, 4 },
                { 1, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 2 },
                { 1, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 2 },
                { 1, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 2 },
                { 5, 5, 6, 5, 5, 5, 0, 0, 0, 5, 5, 5, 7, 5, 5, 5 }, 
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
                { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 }
            };

            var map = new LevelMap
            {
                Width = layout.GetLength(1),
                Height = layout.GetLength(0),
                TileRegistry = registry,
                Layout = layout
            };

            // Dynamically register 20+ doors with automatic math questions
            map.RegisterDoor(6, QuestionGenerator.Difficulty.Easy);
            map.RegisterDoor(7, QuestionGenerator.Difficulty.Medium);

            return map;
        }
    }
}