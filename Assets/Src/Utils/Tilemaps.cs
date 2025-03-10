﻿using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Innerclash.Core;
using Innerclash.Entities;
using Innerclash.World;

namespace Innerclash.Utils {
    public static class Tilemaps {
        public static Tilemap Tilemap => GameController.Instance.tilemap;

        public static Vector3Int CellPos(Vector2 rawPos) => Tilemap.WorldToCell(rawPos);

        public static ScriptedTile GetTile(Vector2 rawPos) => Tilemap.GetTile<ScriptedTile>(CellPos(rawPos));

        public static void ApplyTile(Vector2 rawPos, PhysicsTrait entity) {
            var pos = CellPos(rawPos);
            var tile = Tilemap.GetTile<ScriptedTile>(pos);

            if(tile != null) tile.Apply(entity, pos);
        }

        public static void WithTile(Vector2 rawPos, Action<ScriptedTile, Vector3Int> action) {
            var pos = CellPos(rawPos);
            var tile = Tilemap.GetTile<ScriptedTile>(pos);

            if(tile != null) action(tile, pos);
        }

        public static void RemoveTile(Vector2 rawPos) {
            var pos = CellPos(rawPos);
            var map = Tilemap;
            var tile = map.GetTile<ScriptedTile>(pos);
            
            if(tile != null) {
                map.SetTile(pos, null);
                if(tile.itemDrop.item != null) {
                    tile.itemDrop.item.Create(new Vector2(pos.x + 0.5f, pos.y + 0.5f), tile.itemDrop.amount);
                }

                foreach(var offset in Masks.mask8) {
                    map.RefreshTile(pos + (Vector3Int)offset);
                }
            }
        }
    }
}
