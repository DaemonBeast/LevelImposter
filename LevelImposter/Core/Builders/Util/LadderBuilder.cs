using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class LadderBuilder : IElemBuilder
    {
        public const float LADDER_Y_OFFSET = -0.4f;
        public static readonly Dictionary<string, float> DEFAULT_LADDER_HEIGHTS = new()
        {
            { "util-ladder1", 3.0f },
            { "util-ladder2", 1.5f }
        };

        private static List<Ladder> _allLadders = new();

        private byte _ladderID = 0;

        public LadderBuilder()
        {
            _allLadders.Clear();
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-ladder"))
                return;
            
            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var topPrefab = prefab.transform.FindChild("LadderTop").GetComponent<Ladder>();
            var bottomPrefab = prefab.transform.FindChild("LadderBottom").GetComponent<Ladder>();

            // Default Sprite
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Console
            float ladderHeight = elem.properties.ladderHeight == null ?
                DEFAULT_LADDER_HEIGHTS[elem.type] : (float)elem.properties.ladderHeight;
            
            GameObject topObj = new("LadderTop");
            topObj.transform.SetParent(obj.transform);
            topObj.transform.localPosition = new Vector3(0, ladderHeight + LADDER_Y_OFFSET, 0);
            topObj.AddComponent<BoxCollider2D>().isTrigger = true;
            GameObject bottomObj = new("LadderBottom");
            bottomObj.transform.SetParent(obj.transform);
            bottomObj.transform.localPosition = new Vector3(0, -ladderHeight + LADDER_Y_OFFSET, 0);
            bottomObj.AddComponent<BoxCollider2D>().isTrigger = true;

            Ladder topConsole = topObj.AddComponent<Ladder>();
            Ladder bottomConsole = bottomObj.AddComponent<Ladder>();
            topConsole.Id = _ladderID++;
            topConsole.IsTop = true;
            topConsole.Destination = bottomConsole;
            topConsole.UseSound = topPrefab.UseSound;
            topConsole.Image = spriteRenderer;
            _allLadders.Add(topConsole);

            bottomConsole.Id = _ladderID++;
            bottomConsole.IsTop = false;
            bottomConsole.Destination = topConsole;
            bottomConsole.UseSound = bottomPrefab.UseSound;
            bottomConsole.Image = spriteRenderer;
            _allLadders.Add(bottomConsole);
        }

        public void PostBuild()
        {
            _allLadders.RemoveAll(ladder => ladder == null);
        }

        /// <summary>
        /// Trys the find the ladder of specified id
        /// </summary>
        /// <param name="id">ID of the ladder</param>
        /// <param name="ladder">Cooresponding ladder, if found</param>
        /// <returns>TRUE if found</returns>
        public static bool TryGetLadder(byte id, out Ladder? ladder)
        {
            foreach (Ladder l in _allLadders)
            {
                if (l.Id == id)
                {
                    ladder = l;
                    return true;
                }
            }
            ladder = null;
            return false;
        }
    }
}