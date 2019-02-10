﻿using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AutoTrash
{
	internal class AutoTrashPlayer : ModPlayer
	{
		public bool AutoTrashEnabled = false;
		public List<Item> AutoTrashItems;
		public Item LastAutoTrashItem;

		public bool NoValue;
		internal bool NoValueBelongs(Item item) => item.value == 0;

		public override void Initialize()
		{
			AutoTrashItems = new List<Item>();
			LastAutoTrashItem = new Item();
			LastAutoTrashItem.SetDefaults(0, true);
			AutoTrashEnabled = false;
			NoValue = false;
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				["AutoTrashItems"] = AutoTrashItems,
				["AutoTrashEnabled"] = AutoTrashEnabled,
				[nameof(NoValue)] = NoValue,
			};
		}

		public override void Load(TagCompound tag)
		{
			AutoTrashItems = tag.Get<List<Item>>("AutoTrashItems");
			AutoTrashEnabled = tag.GetBool("AutoTrashEnabled");
			NoValue = tag.GetBool(nameof(NoValue));

			AutoTrash.instance.autoTrashListUI?.UpdateNeeded();
		}

		internal static bool IsModItem(Item item)
		{
			return item.type >= ItemID.Count;
		}

		internal bool ShouldItemBeTrashed(Item item)
		{
			return AutoTrashItems.Where(x => x.type == item.type).Any() || (NoValue && NoValueBelongs(item));
		}

		public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
		{
			if (context == Terraria.UI.ItemSlot.Context.InventoryItem || context == Terraria.UI.ItemSlot.Context.InventoryCoin || context == Terraria.UI.ItemSlot.Context.InventoryAmmo)
			{
				if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl))
				{
					if (AutoTrashEnabled && !AutoTrashItems.Any(x => x.type == inventory[slot].type))
					{
						Main.PlaySound(7, -1, -1, 1, 1f, 0f);

						Item newItem = new Item();
						newItem.SetDefaults(inventory[slot].type);
						AutoTrashItems.Add(newItem);

						LastAutoTrashItem = inventory[slot].Clone();

						inventory[slot].SetDefaults();

						AutoTrash.instance.autoTrashListUI.UpdateNeeded();
						return true;
					}
				}
			}
			return false; // do default behavior.
		}

		public static int caughtFish;
		public override void PreUpdate()
		{
			// Fishing uses player.GetItem bypassing AutoTrash.
			if (Main.myPlayer == player.whoAmI)
			{
				if(caughtFish > 0 && AutoTrashItems.Where(x => x.type == caughtFish).Any()) // type vs Item filter?
				{
					for (int i = 0; i < 59; i++)
					{
						if (!player.inventory[i].IsAir && player.inventory[i].type == caughtFish)
						{
							// TODO: Analyze performance impact? do every 60 frames only?
							LastAutoTrashItem = player.inventory[i].Clone();
							player.inventory[i].TurnToAir();
							break;
						}
					}
				}
				caughtFish = 0;
			}
		}
	}
}
