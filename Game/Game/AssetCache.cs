using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Game.Config;

namespace Game.Services
{
    public static class AssetCache
    {
        private static readonly Dictionary<string, BitmapImage> _cache = new();

        public static BitmapImage GetImage(string relativePath)
        {
            if (!_cache.TryGetValue(relativePath, out var bitmap))
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"{GameConfig.AssetBasePath}{relativePath}");
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // Unlocks max rendering performance in WPF graphics pipeline
                _cache[relativePath] = bitmap;
            }
            return bitmap;
        }
    }
}