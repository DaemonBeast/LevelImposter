﻿using System;
using System.Collections.Generic;
using System.Text;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class MapLoader
    {
        private static string? _lastMapID = null;
        private static LIMap? _currentMap = null;
        public static LIMap CurrentMap => _currentMap;

        /// <summary>
        /// Loads a map from <c>LIMap</c> model
        /// </summary>
        /// <param name="map">LevelImposter map data</param>
        public static void LoadMap(LIMap? map)
        {
            if (_lastMapID != map?.id)
                CleanAssets();
            _lastMapID = map?.id;
            _currentMap = map;
        }

        /// <summary>
        /// Loads a map based on ID from filesystem
        /// </summary>
        /// <param name="mapID">Map file name or ID without extension</param>
        /// <param name="callback">Callback on success</param>
        public static void LoadMap(string mapID, Action? callback)
        {
            MapFileAPI.Instance?.Get(mapID, (mapData) =>
            {
                LoadMap(mapData);
                if (callback != null)
                    callback();
            });
        }

        /// <summary>
        /// Unloads any map, if loaded
        /// </summary>
        public static void UnloadMap()
        {
            _currentMap = null;
        }

        /// <summary>
        /// Cleans all assets from cache and runs GC
        /// </summary>
        private static void CleanAssets()
        {
            LILogger.Msg("Clearing all created assets");
            SpriteLoader.Instance?.ClearAll();
            WAVLoader.Instance?.ClearAll();

            GC.Collect();
        }
    }
}
