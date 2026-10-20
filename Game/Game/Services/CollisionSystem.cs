using System;
using Game.Config;
using Game.Models;

namespace Game.Services
{
    public class CollisionSystem
    {
        private const double SpriteTopPadding = 48;
        private const double SpriteLeftPadding = 16;
        private const double SpriteRightPadding = 16;

        public bool IsColliding(double targetX, double targetY, LevelMap map, Action<int> onDoorHit)
        {
            double footLeft = targetX + SpriteLeftPadding;
            double footRight = targetX + GameConfig.TileSize - SpriteRightPadding;
            double footTop = targetY + SpriteTopPadding;
            double footBottom = targetY + GameConfig.TileSize;

            int startCol = (int)(footLeft / GameConfig.TileSize);
            int endCol = (int)(footRight / GameConfig.TileSize);
            int startRow = (int)(footTop / GameConfig.TileSize);
            int endRow = (int)(footBottom / GameConfig.TileSize);

            for (int r = startRow; r <= endRow; r++)
            {
                for (int c = startCol; c <= endCol; c++)
                {
                    if (r < 0 || r >= map.Height || c < 0 || c >= map.Width)
                        return true;

                    int tileId = map.Layout[r, c];

                    if (map.DoorQuestions.TryGetValue(tileId, out var door))
                    {
                        if (!door.IsUnlocked)
                        {
                            onDoorHit?.Invoke(tileId);
                            return true;
                        }
                    }

                    if (map.TileRegistry.TryGetValue(tileId, out var tile) && tile.IsSolid)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}