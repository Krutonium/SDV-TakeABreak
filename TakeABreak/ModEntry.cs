using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakeABreak
{
    internal class ModEntry : Mod
    {
        private static ModConfig config;
        private static bool DayJustChanged = false;
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            config = this.Helper.ReadConfig<ModConfig>();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            DayJustChanged = true;
            //Other mods can change Max Heath/Energy - This prevents this mod from accidentally hurting you when you wake up.
        }

        private static int PreviousEnergy = 0;
        private static int PreviousHealth = 0;
        private static Vector2 PlayerPosition;
        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            if (Game1.player.hasMenuOpen.Value)
            {
                return;
            }
            // Check if the player has been hurt, used a tool, or moved. If any are true, update values and return.
            if(   Game1.player.health < PreviousHealth
               || (int)Math.Round(Game1.player.stamina, 0) < PreviousEnergy 
               || Game1.player.position.Value != PlayerPosition 
               || DayJustChanged)
            {
                PreviousHealth = Game1.player.health;
                PreviousEnergy = (int)Math.Round(Game1.player.stamina,0);
                PlayerPosition = Game1.player.position.Value;
                DayJustChanged = false;
                this.Monitor.Log("Player Moved or used a tool", LogLevel.Debug);
                return;
            }


            if (Game1.player.health != Game1.player.maxHealth) 
            {
                Game1.player.health = PreviousHealth += config.HealthPerSecond;
            }

            if((int)Math.Round(Game1.player.stamina, 0) != (int)Math.Round((float)Game1.player.MaxStamina, 0))
            {
                Game1.player.Stamina = PreviousEnergy += config.EnergyPerSecond;
            }

            this.Monitor.Log("Added to Energy and Health (if applicable)", LogLevel.Trace);
        }

        class ModConfig
        {
            public int HealthPerSecond = 2;
            public int EnergyPerSecond = 2;
        }
    }
}
