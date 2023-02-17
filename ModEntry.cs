using StardewModdingAPI;
using StardewValley;

namespace QToDrop {
    public class ModEntry : Mod {
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig Config;
        
        private bool IsDropItemKeybindPressedFirstSequence = false;
        private bool IsDropItemKeybindPressedSecondSequence = false;
        private bool IsCTRLPressed = false;
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            
            helper.ConsoleCommands.Add("qtodrop", "Set drop item keybind", this.SetDropItemKeybind);

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked;
        }
        
        private void OnOneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e) {
            if (IsDropItemKeybindPressedFirstSequence) {
                IsDropItemKeybindPressedSecondSequence = true;
            }
        }
        
        private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e) {
            if (IsDropItemKeybindPressedSecondSequence) {
                Game1.player.dropActiveItem();
            }
        }

        private void SetDropItemKeybind(string command, string[] args) {
            if (args == null || args.Length == 0) {
                Monitor.Log("Invalid arguments. Usage: qtodrop [none|keybind]", LogLevel.Error);
                return;
            }

            string keybind = args[0];

            if (keybind == "none") {
                this.Config.DropItemKeybind = null;
                Monitor.Log("Drop item keybind disabled.", LogLevel.Info);
                return;
            }

            this.Config.DropItemKeybind = keybind.ToLower();
            Monitor.Log("Drop item keybind set to " + keybind.ToUpper() + "!", LogLevel.Info);
        }

        private void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e) {
            if (e.Button == SButton.LeftControl || e.Button == SButton.RightControl) {
                IsCTRLPressed = true;
            }
            
            if (e.Button.ToString().ToLower().Equals(this.Config.DropItemKeybind.ToLower())) {
                IsDropItemKeybindPressedFirstSequence = true;
                
                if (IsCTRLPressed) {
                    if (Game1.player.CurrentItem == null || !Game1.player.CurrentItem.canBeDropped()) {
                        return;
                    }

                    Game1.createItemDebris(Game1.player.CurrentItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);

                    Game1.player.removeItemsFromInventory(Game1.player.CurrentItem.ParentSheetIndex, Game1.player.CurrentItem.Stack);
                    Game1.player.showNotCarrying();
                }
                
                Game1.player.dropActiveItem();
            }
        }
        
        private void OnButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e) {
            if (e.Button == SButton.LeftControl || e.Button == SButton.RightControl) {
                IsCTRLPressed = false;
            }
            
            if (e.Button.ToString().ToLower().Equals(this.Config.DropItemKeybind.ToLower())) {
                IsDropItemKeybindPressedFirstSequence = false;
                IsDropItemKeybindPressedSecondSequence = false;
            }
        }
    }
}