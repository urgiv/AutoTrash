﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutoTrash
{
	class AutoTrashListUI : UIState
	{
		public static bool visible = false;
		public UIPanel mainPanel;
		public UIGrid autoTrashGrid;
		public FixedUIScrollbar autoTrashGridScrollbar;
		float spacing = 8f;
		UICheckbox NoValueCheckbox;

		public override void OnInitialize() {
			int checkboxesHeight = 30;

			mainPanel = new UIPanel();
			mainPanel.SetPadding(0);
			mainPanel.Left.Set(250f, 0f);
			mainPanel.Top.Set(310f, 0f);
			mainPanel.Width.Set(200f, 0f);
			mainPanel.Height.Set(300f + checkboxesHeight, 0f);
			mainPanel.BackgroundColor = new Color(73, 94, 171);
			mainPanel.OnLeftMouseDown += DragStart;
			mainPanel.OnLeftMouseUp += DragEnd;

			Asset<Texture2D> closeTexture = AutoTrash.instance.Assets.Request<Texture2D>("closeButton");
			UIImageButton closeButton = new UIImageButton(closeTexture);
			closeButton.Left.Set(-23, 1f);
			closeButton.Top.Set(8, 0f);
			closeButton.Width.Set(15, 0f);
			closeButton.Height.Set(14, 0f);
			closeButton.OnLeftClick += new MouseEvent(CloseButtonClicked);
			mainPanel.Append(closeButton);

			UIText label = new UIText(Language.GetTextValue("Mods.AutoTrash.ClickToRemove"));
			label.Left.Set(8, 0f);
			label.Top.Set(8, 0f);
			mainPanel.Append(label);

			autoTrashGrid = new UIGrid(4);
			autoTrashGrid.Top.Pixels = 32f + spacing;
			autoTrashGrid.Left.Pixels = spacing;
			autoTrashGrid.Width.Set(-25f, 1f);
			autoTrashGrid.Height.Set(-55f - checkboxesHeight, 1f);
			autoTrashGrid.ListPadding = 12f;
			mainPanel.Append(autoTrashGrid);

			autoTrashGridScrollbar = new FixedUIScrollbar();
			autoTrashGridScrollbar.SetView(100f, 1000f);
			autoTrashGridScrollbar.Top.Pixels = 32f + spacing;
			autoTrashGridScrollbar.Height.Set(-50f - spacing - checkboxesHeight, 1f);
			autoTrashGridScrollbar.HAlign = 1f;
			mainPanel.Append(autoTrashGridScrollbar);
			autoTrashGrid.SetScrollbar(autoTrashGridScrollbar);

			NoValueCheckbox = new UICheckbox(Language.GetTextValue("Mods.AutoTrash.NoValue"), Language.GetTextValue("Mods.AutoTrash.TrashAllItemsWithNoValue"));
			NoValueCheckbox.Top.Set(300, 0f);
			NoValueCheckbox.Left.Set(12, 0f);
			NoValueCheckbox.OnSelectedChanged += (a, b) => Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>().NoValue = NoValueCheckbox.Selected;
			mainPanel.Append(NoValueCheckbox);

			Append(mainPanel);
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuClose);
			visible = false;
		}

		Vector2 offset;
		public bool dragging = false;
		private void DragStart(UIMouseEvent evt, UIElement listeningElement) {
			if (evt.Target != autoTrashGridScrollbar) {
				offset = new Vector2(evt.MousePosition.X - mainPanel.Left.Pixels, evt.MousePosition.Y - mainPanel.Top.Pixels);
				dragging = true;
			}
		}

		private void DragEnd(UIMouseEvent evt, UIElement listeningElement) {
			if (dragging) {
				Vector2 end = evt.MousePosition;
				dragging = false;

				mainPanel.Left.Set(end.X - offset.X, 0f);
				mainPanel.Top.Set(end.Y - offset.Y, 0f);

				Recalculate();
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			UpdateCheckboxes();
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (mainPanel.ContainsPoint(MousePosition)) {
				Main.LocalPlayer.mouseInterface = true;
				Terraria.GameInput.PlayerInput.LockVanillaMouseScroll("AutoTrash/AutoTrashListUI");
			}
			if (dragging) {
				mainPanel.Left.Set(MousePosition.X - offset.X, 0f);
				mainPanel.Top.Set(MousePosition.Y - offset.Y, 0f);
				Recalculate();
			}
		}


		internal void UpdateNeeded() {
			updateneeded = true;
		}

		private bool updateneeded;
		internal void UpdateCheckboxes() {
			if (!updateneeded) { return; }
			updateneeded = false;
			autoTrashGrid.Clear();

			var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();

			foreach (var item in autoTrashPlayer.AutoTrashItems) {
				if (item.ModItem is not Terraria.ModLoader.Default.UnloadedItem) {
					ItemSlot box = new ItemSlot(item.type);

					autoTrashGrid._items.Add(box);
					autoTrashGrid._innerList.Append(box);
				}
			}
			autoTrashGrid.UpdateOrder();
			autoTrashGrid._innerList.Recalculate();

			NoValueCheckbox.Selected = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>().NoValue;
		}
	}

	public class FixedUIScrollbar : UIScrollbar
	{
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = AutoTrash.autoTrashUserInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void LeftMouseDown(UIMouseEvent evt) {
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = AutoTrash.autoTrashUserInterface;
			base.LeftMouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
}
