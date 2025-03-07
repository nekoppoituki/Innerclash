﻿using UnityEngine;

using static Innerclash.Misc.Item;

namespace Innerclash.Entities {
    /// <summary> An entity trait that holds an inventory </summary>
    public class InventoryTrait : MonoBehaviour {
        public float maxMass = 1000f;

        public ItemInventory Inventory { get; private set; }
        public float LastUpdated { get; private set; }

        void Start() {
            Inventory = new ItemInventory() {
                offset = 10
            };
        }

        public int Accept(ItemStack stack) {
            return Mathf.Min(Mathf.FloorToInt((maxMass - Inventory.TotalMass) / stack.item.mass), stack.amount);
        }

        public int Add(ItemStack stack) {
            return Add(stack, null);
        }

        public int Add(ItemStack stack, int? idx) {
            LastUpdated = Time.time;

            int res = Accept(stack);
            Inventory.Add(new ItemStack(stack.item, res), idx);

            return res;
        }
    }
}
