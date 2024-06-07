using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutoTrash
{
	internal class AutoTrashSystem : ModSystem
	{
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryLayerIndex != -1) {
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Slot",
					delegate {
						ModContent.GetInstance<AutoTrashGlobalItem>().DrawUpdateAutoTrash();
						return true;
					},
					InterfaceScaleType.UI)
				);

				layers.Insert(inventoryLayerIndex + 2, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Cursor",
					delegate {
						if (Main.cursorOverride == 6 && (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl)) && Main.keyState.PressingShift()) {
							var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();
							if (autoTrashPlayer.AutoTrashEnabled)
								Main.cursorOverride = 5;
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}

			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (MouseTextIndex != -1) {
				layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash List",
					delegate {
						if (AutoTrashListUI.visible) {

							AutoTrash.autoTrashUserInterface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			if (AutoTrashListUI.visible) {
				AutoTrash.autoTrashUserInterface.Update(gameTime);
			}
		}
	}
}
