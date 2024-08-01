using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("CupboardSwitcher", "RustFlash", "1.2.0")]
    public class CupboardSwitcher : RustPlugin
    {
        private const string PermissionUse = "cupboardswitcher.use";
        private const string UiName = "CupboardSwitcherUI";

        private readonly string[] cupboardShortnames = 
        {
            "cupboard.tool",
            "cupboard.tool.retro",
            "cupboard.tool.shockbyte"
        };

        void Init()
        {
            permission.RegisterPermission(PermissionUse, this);
        }

        void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, UiName);
            }
        }

        [ChatCommand("cupboard")]
        private void CupboardCommand(BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, PermissionUse))
            {
                player.ChatMessage("You do not have permission to use this command.");
                return;
            }

            ShowCupboardUI(player);
        }

        private void ShowCupboardUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UiName);

            var elements = new CuiElementContainer();

            elements.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0.4 0.4", AnchorMax = "0.6 0.6" },
                CursorEnabled = true
            }, "Overlay", UiName);

            elements.Add(new CuiButton
            {
                Button = { Command = $"cupboardswitcherclose", Color = "0.7 0.2 0.2 1" },
                RectTransform = { AnchorMin = "0.9 0.9", AnchorMax = "1 1" },
                Text = { Text = "X", FontSize = 14, Align = TextAnchor.MiddleCenter }
            }, UiName);

            AddButton(elements, UiName, "0.1 0.7", "0.9 0.9", "Normal", "cupboard.tool");
            AddButton(elements, UiName, "0.1 0.4", "0.9 0.6", "Retro", "cupboard.tool.retro");
            AddButton(elements, UiName, "0.1 0.1", "0.9 0.3", "Shockbyte", "cupboard.tool.shockbyte");

            CuiHelper.AddUi(player, elements);
        }

        private void AddButton(CuiElementContainer elements, string parent, string anchorMin, string anchorMax, string text, string command)
        {
            elements.Add(new CuiButton
            {
                Button = { Command = $"cupboardswitch {command}", Color = "0.7 0.7 0.7 1" },
                RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
                Text = { Text = text, FontSize = 14, Align = TextAnchor.MiddleCenter }
            }, parent);
        }

        [ConsoleCommand("cupboardswitch")]
        private void CupboardSwitch(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null || !permission.UserHasPermission(player.UserIDString, PermissionUse)) return;

            var newCupboardShortname = arg.GetString(0);
            if (string.IsNullOrEmpty(newCupboardShortname)) return;

            var inventory = player.inventory;
            var oldCupboard = FindCupboardInInventory(inventory);

            if (oldCupboard == null)
            {
                player.ChatMessage("You do not have a cupboard to switch in your inventory.");
                return;
            }

            oldCupboard.Remove();
            var newCupboard = ItemManager.CreateByName(newCupboardShortname, 1);
            player.GiveItem(newCupboard);
            player.ChatMessage($"Your cupboard has been switched to {newCupboardShortname}.");

            CuiHelper.DestroyUi(player, UiName);
        }

        [ConsoleCommand("cupboardswitcherclose")]
        private void CloseCupboardSwitcher(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            CuiHelper.DestroyUi(player, UiName);
        }

        private Item FindCupboardInInventory(PlayerInventory inventory)
        {
            List<Item> allItems = new List<Item>();
            allItems.AddRange(inventory.containerMain.itemList);
            allItems.AddRange(inventory.containerBelt.itemList);

            return allItems.Find(item => cupboardShortnames.Contains(item.info.shortname));
        }
    }
}