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

        // Multi-floor support
        public List<int[,]> Floors { get; set; } = new();
        public int CurrentFloorIndex { get; private set; } = 0;

        // The current layout dynamically returns the grid of the active floor
        public int[,] Layout => Floors[CurrentFloorIndex];

        public void RegisterDoor(int doorTileId, QuestionGenerator.Difficulty difficulty = QuestionGenerator.Difficulty.Easy)
        {
            if (!DoorQuestions.ContainsKey(doorTileId))
            {
                DoorQuestions[doorTileId] = DoorQuestion.CreateRandom(doorTileId, difficulty);
            }
        }

        public void ChangeFloor(bool goingUp)
        {
            if (goingUp && CurrentFloorIndex < Floors.Count - 1)
                CurrentFloorIndex++;
            else if (!goingUp && CurrentFloorIndex > 0)
                CurrentFloorIndex--;
        }

        public int GetTileAt(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return 99; // Return void if out of bounds
            return Layout[y, x]; // y is row (Height), x is column (Width)
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
                
                // Door Difficulty IDs
                { 6, new TileType { Id = 6, AssetName = "Door.png", IsSolid = true } },
                { 7, new TileType { Id = 7, AssetName = "Door.png", IsSolid = true } },
                { 8, new TileType { Id = 8, AssetName = "Door.png", IsSolid = true } },
                
                // Stairs
                { 11, new TileType { Id = 11, AssetName = "StairsUp.png", IsSolid = false } },
                { 12, new TileType { Id = 12, AssetName = "StairsDown.png", IsSolid = false } },

                { 99, new TileType { Id = 99, AssetName = "Void.png", IsSolid = true } },
                { 100, new TileType { Id = 100, AssetName = "Door.png", IsSolid = true } }
            };

            int width = 30;
            int height = 20;

            var map = new LevelMap
            {
                Width = width,
                Height = height,
                TileRegistry = registry
            };

            // Generate 3 floors
            map.Floors.Add(GenerateJapaneseSchoolFloor(0, width, height, 6)); // Begane grond (Easy doors)
            map.Floors.Add(GenerateJapaneseSchoolFloor(1, width, height, 7)); // Verdieping 1 (Medium doors)
            map.Floors.Add(GenerateJapaneseSchoolFloor(2, width, height, 8)); // Verdieping 2 (Hard doors)

            // Overwrite specific tiles for the main exit and final boss on the ground floor
            map.Floors[0][19, 14] = 100; // Boss door at the bottom of the main courtyard
            map.Floors[0][19, 15] = 100;

            // Register questions linked to the door IDs
            map.RegisterDoor(6, QuestionGenerator.Difficulty.Easy);
            map.RegisterDoor(7, QuestionGenerator.Difficulty.Medium);
            map.RegisterDoor(8, QuestionGenerator.Difficulty.Hard);
            map.RegisterDoor(100, QuestionGenerator.Difficulty.Hard); // Boss door

            return map;
        }

        private static int[,] GenerateJapaneseSchoolFloor(int floorIndex, int width, int height, int doorId)
        {
            int[,] grid = new int[height, width];

            // 1. Fill with void/back walls initially
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[y, x] = 99; // Default void
                }
            }

            // 2. Carve U-Shaped Hallways (Bottom, Left, Right)
            CarveSpace(grid, 2, 14, 26, 3, 0); // Bottom hallway
            CarveSpace(grid, 2, 2, 3, 12, 0);  // Left hallway
            CarveSpace(grid, 25, 2, 3, 12, 0); // Right hallway

            // 3. Add Classrooms (Shifted slightly per floor for variety)
            int shift = floorIndex * 2;

            // Left Wing Classrooms
            CarveRoom(grid, 6, 2 + shift, 6, 4, 0, 3);
            grid[3 + shift, 5] = doorId; // Door connecting to left hallway
            grid[5 + shift, 5] = doorId;

            // Right Wing Classrooms
            CarveRoom(grid, 18, 4 - (shift % 2), 6, 4, 0, 3);
            grid[5 - (shift % 2), 24] = doorId; // Door connecting to right hallway

            // Bottom Wing Classrooms
            CarveRoom(grid, 10 + shift, 9, 8, 4, 0, 3);
            grid[13, 12 + shift] = doorId; // Door connecting to bottom hallway
            grid[13, 15 + shift] = doorId;

            // 4. Place Stairs
            if (floorIndex < 2) // Stairs going up
            {
                grid[2, 3] = 11;
            }
            if (floorIndex > 0) // Stairs going down
            {
                grid[2, 26] = 12;
            }

            // 5. Apply wall borders around floors
            ApplyWalls(grid, width, height);

            return grid;
        }

        private static void CarveSpace(int[,] grid, int startX, int startY, int width, int height, int floorId)
        {
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    grid[y, x] = floorId;
                }
            }
        }

        private static void CarveRoom(int[,] grid, int startX, int startY, int width, int height, int floorId, int wallId)
        {
            // Carve floors
            CarveSpace(grid, startX, startY, width, height, floorId);

            // Add top backwalls for visual depth inside rooms
            for (int x = startX; x < startX + width; x++)
            {
                grid[startY - 1, x] = wallId;
            }
        }

        private static void ApplyWalls(int[,] grid, int width, int height)
        {
            // Simple algorithm to place walls around floor tiles where void exists
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[y, x] == 0) // If it's a floor
                    {
                        // Check above for backwall
                        if (y > 0 && grid[y - 1, x] == 99) grid[y - 1, x] = 3;

                        // Check below for frontwall
                        if (y < height - 1 && grid[y + 1, x] == 99) grid[y + 1, x] = 5;

                        // Check left for leftwall
                        if (x > 0 && grid[y, x - 1] == 99) grid[y, x - 1] = 1;

                        // Check right for rightwall
                        if (x < width - 1 && grid[y, x + 1] == 99) grid[y, x + 1] = 2;
                    }
                }
            }
        }
    }
}