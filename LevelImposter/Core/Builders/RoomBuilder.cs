﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class RoomBuilder : Builder
    {
        private int roomId = 0;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;

            SystemTypes systemType = (SystemTypes)roomId;
            roomId++;

            PlainShipRoom shipRoom = obj.AddComponent<PlainShipRoom>();
            shipRoom.RoomId = systemType;
            shipRoom.roomArea = obj.GetComponent<Collider2D>();
            shipRoom.roomArea.isTrigger = true;

            ShipStatus.Instance.AllRooms = LIShipStatus.AddToArr(ShipStatus.Instance.AllRooms, shipRoom);
            ShipStatus.Instance.FastRooms.Add(systemType, shipRoom);

            // Collider
            PolygonCollider2D polyCollider = obj.GetComponent<PolygonCollider2D>();
            if (polyCollider != null)
                polyCollider.isTrigger = true;
        }

        public void PostBuild()
        {
            roomId = 0;
        }
    }
}
