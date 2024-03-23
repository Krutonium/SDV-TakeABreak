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

        private static float PreviousEnergy = 0;
        private static int PreviousHealth = 0;
        private static Vector2 PlayerPosition;
        private static int Count = 0;
        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                //Game isn't ready
                return;
            }
            if (Game1.player.hasMenuOpen.Value)
            {
                //Don't run while Menu's are open
                return;
            }

            if (Count != config.HowOften)
            {
                //Add 1 to the second counter and return because we're not ready yet.
                Count += 1;
                return;
            }
            else
            {
                //Reset the counter and run the code.
                Count = 0;
            }

            // Check if the player has been hurt, used a tool, or moved, or slept. If any are true, update values and return.
            const float tolerance = 0.5f;
            if(   Game1.player.health != PreviousHealth
               || Math.Abs((int)Math.Round(Game1.player.stamina, 0) - PreviousEnergy) > tolerance 
               || Game1.player.position.Value != PlayerPosition 
               || DayJustChanged)
            {
                PreviousHealth = Game1.player.health;
                PreviousEnergy = (int)Math.Round(Game1.player.stamina, 0);
                PlayerPosition = Game1.player.position.Value;
                DayJustChanged = false;
                //this.Monitor.Log("Player Moved or used a tool", LogLevel.Debug);
                return;
            }

            if (Game1.player.health != Game1.player.maxHealth)
            {
                int potentialHealth = PreviousHealth += config.HealthPerSecond;
                Game1.player.health = Math.Min(potentialHealth, Game1.player.maxHealth);
            }

            if((int)Math.Round(Game1.player.stamina, 0) <= (int)Math.Round((float)Game1.player.MaxStamina, 0))
            {
                float potentialEnergy = PreviousEnergy += config.EnergyPerSecond;
                Game1.player.Stamina = Math.Min(potentialEnergy, Game1.player.MaxStamina);
            }   
            //this.Monitor.Log("Added to Energy and Health (if applicable)", LogLevel.Debug);
        }

        class ModConfig
        {
            public int HealthPerSecond = 2;
            public float EnergyPerSecond = 2;
            public int HowOften = 1;
        }
    }
}
