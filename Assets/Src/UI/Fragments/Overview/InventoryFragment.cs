﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Innerclash.Core;
using Innerclash.Entities;
using Innerclash.Utils;

using static Innerclash.Misc.Item;

namespace Innerclash.UI.Fragments.Overview {
    public class InventoryFragment : MonoBehaviour, ISlotButtonHandler {
        public GameObject slotPrefab;

        [Header("Attributes")]
        public InventoryTrait trait;
        public RectTransform slotParent;
        public Slider massBar;
        public Image massImage;
        public HotbarIndexer hotbarIndexer;

        readonly List<SlotButton> slots = new List<SlotButton>();
        readonly List<int> strips = new List<int>();
        float lastUpdated;
        bool forceUpdate;
        bool needsStripping;

        void Start() {
            foreach(var slot in hotbarIndexer.slots) {
                slot.left.handler = this;
                slots.Add(slot.left);

                slot.right.handler = this;
                slots.Add(slot.right);
            }
        }

        void Update() {
            if(lastUpdated < trait.LastUpdated || forceUpdate) {
                forceUpdate = false;
                lastUpdated = trait.LastUpdated;

                int highest = Mathf.Max(trait.Inventory.Highest + 1, slots.Count - 1, trait.Inventory.offset);
                while(slots.Count <= highest) {
                    var n = Instantiate(slotPrefab, slotParent).GetComponent<SlotButton>();
                    n.handler = this;

                    slots.Add(n);
                }

                for(int i = 0; i < highest; i++) {
                    var slot = slots[i];
                    if(trait.Inventory.contents.ContainsKey(i)) {
                        slot.Set(trait.Inventory.contents[i]);
                    } else {
                        slot.ResetIcon();
                    }
                }

                if(needsStripping) {
                    strips.Clear();

                    var content = trait.Inventory.contents;
                    for(int i = slots.Count - 2; i >= trait.Inventory.offset; i--) {
                        if(!content.ContainsKey(i) && !content.ContainsKey(i + 1)) {
                            strips.Add(i + 1);
                        } else if(content.ContainsKey(i)) {
                            break;
                        }
                    }

                    foreach(int strip in strips) {
                        var slot = slots[strip];
                        slots.RemoveAt(strip);

                        Destroy(slot.gameObject);
                    }

                    needsStripping = false;
                }

                massBar.value = Mathf.Min(trait.Inventory.TotalMass / trait.maxMass, 1f);
                massImage.color = Color.Lerp(Color.green, Color.red, massBar.value);
            }
        }

        public void Interact(int idx) {
            if(idx < 0 || idx >= slots.Count) return;

            var inst = GameController.Instance;
            var inv = trait.Inventory;
            var content = inv.contents;

            if(inst.HoldingStack) { // If is already holding a stack, either put or swap
                var source = inst.CurrentStack;
                if(content.ContainsKey(idx)) { // If already has a stack, either merge or swap
                    var stack = content[idx];
                    if(stack.item == source.item && !stack.Full) { // If the item is the same, transfer it
                        ItemStack.Transfer(ref source, ref stack, source.amount);

                        content[idx] = stack;
                        inst.CurrentStack = source;

                        if(source.Empty) needsStripping = true;
                    } else { // Otherwise, simply swap
                        content[idx] = source;
                        inst.CurrentStack = stack;
                    }
                } else { // Otherwise, simply put
                    int accepted = trait.Add(source, idx);
                    source.amount -= accepted;

                    inst.CurrentStack = source;

                    needsStripping = true;
                }
                
                forceUpdate = true;
            } else if(content.ContainsKey(idx)) { // Otherwise try to take an existing stack from inventory
                inst.CurrentStack = content[idx];
                content.Remove(idx);

                inst.FromInventory = true;
                forceUpdate = true;
            }
        }

        public void Handle(SlotButton button) {
            Interact(Structs.IndexOf(slots, button));
        }

        public void Trash() {
            GameController.Instance.CurrentStack = new ItemStack();

            forceUpdate = true;
            needsStripping = true;
        }

        public void Drop() {
            if(Drop(GameController.Instance.CurrentStack)) {
                GameController.Instance.CurrentStack = new ItemStack();

                forceUpdate = true;
                needsStripping = true;
            }
        }

        public bool Drop(ItemStack stack) {
            if(stack.Empty) return false;

            var item = stack.item.Create(trait.transform.position, stack.amount);
            return item.FromEntity = true;
        }

        public void OnVisibilityChange(bool visible) {
            var inst = GameController.Instance;
            if(!visible && inst.HoldingStack) {
                var source = inst.CurrentStack;

                int accepted = trait.Add(source);
                source.amount -= accepted;

                Drop(source);
                inst.CurrentStack = new ItemStack();

                forceUpdate = true;
                needsStripping = true;
            }
        }
    }
}
