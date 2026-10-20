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
                
                // Deuren voor de lokalen
                { 6, new TileType { Id = 6, AssetName = "Door.png", IsSolid = true } },
                { 7, new TileType { Id = 7, AssetName = "Door.png", IsSolid = true } },
                { 8, new TileType { Id = 8, AssetName = "Door.png", IsSolid = true } },
                
                // Opvulling voor ruimtes 'buiten' de map (maak hiervoor een compleet zwart plaatje, of gebruik FrontWall.png)
                { 99, new TileType { Id = 99, AssetName = "Void.png", IsSolid = true } }, 
                
                // De Hoofduitgang (Het doel van de game!)
                { 100, new TileType { Id = 100, AssetName = "Door.png", IsSolid = true } } 
            };

            // Layout 26 breed x 13 hoog
            int[,] layout = new int[,]
            {
                // Lokaal 1 (Links)       Lokaal 2 (Midden)       Lokaal 3 (Rechts)       Dode Ruimte
                { 3, 3, 3, 4, 4, 3, 3,    3, 3, 3, 3, 4, 3, 3,    3, 3, 4, 4, 3, 3, 3,    99,99,99,99, 99}, // 0
                { 1, 0, 0, 0, 0, 0, 2,    1, 0, 0, 0, 0, 0, 2,    1, 0, 0, 0, 0, 0, 2,    99,99,99,99, 99}, // 1
                { 1, 0, 0, 0, 0, 0, 2,    1, 0, 0, 0, 0, 0, 2,    1, 0, 0, 0, 0, 0, 2,    99,99,99,99, 99}, // 2
                { 1, 0, 0, 0, 0, 0, 2,    1, 0, 0, 0, 0, 0, 2,    1, 0, 0, 0, 0, 0, 2,    99,99,99,99, 99}, // 3
                { 3, 3, 3, 6, 3, 3, 3,    3, 3, 7, 3, 3, 3, 3,    3, 3, 8, 3, 3, 3, 3,    3, 4, 3, 3,  3}, // 4 (Deuren naar de gang)
                
                // De Grote Horizontale Gang
                { 1, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0,  2 }, // 5 
                { 1, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0,  2 }, // 6 
                { 1, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0,    0, 0, 0, 0,  2 }, // 7 
                { 5, 5, 5, 5, 5, 5, 5,    5, 5, 5, 5, 5, 5, 5,    5, 5, 5, 5, 5, 5, 5,    1, 0, 0, 0,  2 }, // 8 (Gang buigt af naar beneden)
                
                // Verticale gang richting de ontsnapping
                { 99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   1, 0, 0, 0,  2 }, // 9 
                { 99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   1, 0, 0, 0,  2 }, // 10
                { 99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   1, 0, 0, 0,  2 }, // 11
                { 99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   99,99,99,99,99,99,99,   5, 5,100,5,  5 }  // 12 (HOOFDDEUR 100)
            };

            var map = new LevelMap
            {
                Width = layout.GetLength(1),
                Height = layout.GetLength(0),
                TileRegistry = registry,
                Layout = layout
            };

            // Registreer de vragen: hoe verder je komt, hoe moeilijker
            map.RegisterDoor(6, QuestionGenerator.Difficulty.Easy);
            map.RegisterDoor(7, QuestionGenerator.Difficulty.Medium);
            map.RegisterDoor(8, QuestionGenerator.Difficulty.Medium);
            
            // De hoofddeur heeft een moeilijke 'boss' vraag nodig!
            map.RegisterDoor(100, QuestionGenerator.Difficulty.Hard);

            return map;
        }
    }
}