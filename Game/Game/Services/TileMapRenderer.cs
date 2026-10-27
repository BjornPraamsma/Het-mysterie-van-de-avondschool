
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Game.Config;
using Game.Models;

namespace Game.Services
{
    public class TileMapRenderer
    {
        private readonly Canvas _canvas;

        public TileMapRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Render(LevelMap map, Image playerImage)
        {
            _canvas.Children.Clear();

            // Bewaar boekenkasten om ze later te tekenen
            List<(int row, int col, TileType type)> bookshelves = new();

            // STAP 1: Teken alle gewone tegels
            for (int r = 0; r < map.Height; r++)
            {
                for (int c = 0; c < map.Width; c++)
                {
                    int tileId = map.Layout[r, c];

                    if (!map.TileRegistry.TryGetValue(tileId, out var tileType))
                        continue;

                    bool isBookshelf =
                        tileId == 20 ||
                        tileId == 21 ||
                        tileId == 22;

                    // Boekenkasten bewaren voor later
                    if (isBookshelf)
                    {
                        bookshelves.Add((r, c, tileType));

                        // Teken vloer onder de boekenkast
                        DrawTile("Floor.png", r, c,
                            GameConfig.TileSize,
                            GameConfig.TileSize);

                        continue;
                    }

                    // Gewone tegel tekenen
                    DrawTile(tileType.AssetName, r, c,
                        GameConfig.TileSize,
                        GameConfig.TileSize);
                }
            }

            // STAP 2: Teken alle boekenkasten
            foreach (var bookshelf in bookshelves)
            {
                double width = GameConfig.TileSize * 1.5;
                double height = GameConfig.TileSize * 1.5;

                Image tile = new Image
                {
                    Width = width,
                    Height = height,
                    Source = AssetCache.GetImage(
                        bookshelf.type.AssetName),
                    Stretch = Stretch.Fill
                };

                // Horizontaal centreren
                double x = bookshelf.col * GameConfig.TileSize
                    - (width - GameConfig.TileSize) / 2;

                // Bovenkant op tegelpositie houden
                double y = bookshelf.row * GameConfig.TileSize -65;

                Canvas.SetLeft(tile, x);
                Canvas.SetTop(tile, y);

                _canvas.Children.Add(tile);
            }

            // STAP 3: Teken de speler als laatste
            if (!_canvas.Children.Contains(playerImage))
            {
                _canvas.Children.Add(playerImage);
            }
        }

        // Hulpfunctie om gewone tegels te tekenen
        private void DrawTile(
            string assetName,
            int row,
            int col,
            double width,
            double height)
        {
            Image tile = new Image
            {
                Width = width,
                Height = height,
                Source = AssetCache.GetImage(assetName),
                Stretch = Stretch.Fill
            };

            Canvas.SetLeft(tile, col * GameConfig.TileSize);
            Canvas.SetTop(tile, row * GameConfig.TileSize);

            _canvas.Children.Add(tile);
        }
    }
}
