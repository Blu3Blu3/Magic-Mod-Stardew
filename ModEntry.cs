using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Menus;
using xTile.Tiles;

namespace FirstStardewModCBG
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        // To do: Make controller equivalents for these combos.

        private List<SButton>? recentPressedKeys;

        // Have all spells inflict nausea and health drain to prevent eating to nullify costs. These effects decrease in severity the higher the magic skill is, however.

        // Warp to given location (home, statues, island). Home warp open by default. Costs a quarter of max energy.
        public static readonly List<SButton> warpToLocationComboKeys = [SButton.W, SButton.A, SButton.S, SButton.D, SButton.W, SButton.A, SButton.S, SButton.D];
        // Speed up by given levels based on magic skill. Costs a quarter of max energy + weakness + lowered defense.
        public static readonly List<SButton> speedComboKeys = [SButton.S, SButton.A, SButton.D, SButton.W, SButton.S, SButton.W, SButton.MouseRight];
        // Luck up by given levels based on magic skill. Costs a third of max energy + weakness + lowered defense.
        public static readonly List<SButton> luckComboKeys = [SButton.A, SButton.S, SButton.A, SButton.S, SButton.A, SButton.D, SButton.MouseLeft, SButton.MouseRight];
        // Create "Help Wanted" quest for a random character that only lasts 1 day, and can only be cast once per day. Costs a half of max energy, and requires level 3 magic. [Make new dialogue for this!]
        public static readonly List<SButton> readMindComboKeys = [SButton.MouseLeft, SButton.A, SButton.S, SButton.D, SButton.W, SButton.MouseRight];
        // Transmute items (1 quartz to 1 clay, 3 gold to 1 iridium, 2 bombs to 1 mega bomb, etc.) based on magic skill. Costs a quarter of max energy + lowered defense, and requires level 3 magic for the default option.
        public static readonly List<SButton> transmuteComboKeys = [SButton.A, SButton.D, SButton.W, SButton.S, SButton.D, SButton.A, SButton.S, SButton.W];
        // Temporarily become intangible, taking and dealing no damage to enemies. Costs a third of max energy + lowered luck, and requires level 4 magic for the default duration. Learned at Yoba altar.
        public static readonly List<SButton> ghostModeComboKeys = [SButton.MouseLeft, SButton.MouseRight, SButton.MouseLeft, SButton.A, SButton.D, SButton.A, SButton.S, SButton.S];
        // Reset the week's given gifts to 0 for a target person, allowing 2 more gifts to be given this week. Costs a half of max energy + lowered speed, and requires level 5 magic. Learned at the Witch's Hut.
        public static readonly List<SButton> amnesiaComboKeys = [SButton.D, SButton.S, SButton.A, SButton.W, SButton.D, SButton.MouseLeft, SButton.MouseRight, SButton.MouseLeft];



        /*********
        ** Public methods
        *********/
            /// <summary>The mod entry point, called after the mod is first loaded.</summary>
            /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

        }

        /*********
        ** Private methods
        *********/
        // ButtonsChanged methods (Held, Pressed, Released) all return SButton IEnumerables, which are basically read-only Lists.
        // You have to enumerate over them with foreach or some other iterator. In order to get the value of the iterator, use the "yield" keyword with either "return" to get the value, or "break" to stop the iteration early.
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            bool spaceReleased = e.Released.Contains(SButton.Space);
            if (spaceReleased && Game1.activeClickableMenu == null)
            {
                SpellCheck(recentPressedKeys);
            }
            return;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if(!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }

        /// <summary>Removes control from the player, puts their character into a spin + hand up gesture, then edits the sprite with a purple-white overlay to indicate magic.</summary>
        /// <returns></returns>
        /*private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/farmer_base") || e.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/farmer_girl_base"))
            {
                var editor = asset.AsImage();
                // Eventually make a spritesheet please...
                IRawTextureData sourceImage = this.Helper.ModContent.Load<IRawTextureData>("magic_cast_overlay_1.png");
                editor.PatchImage(sourceImage, targetArea: new Rectangle());
                Game1.player.animateInFacingDirection
                Character.
                // Rectangle constructor is (x, y, width, height). Not overloaded.
            }
        }*/

        /// <summary> Raised after the player releases Space after holding it and entering a key combo. Checks all magic lists for matches to the key combo, then performs the spell and returns true if a match is found.</summary>
        /// <param name="keysToCheck">The keys to check against, or "recentKeysPressed".</param>
        private bool SpellCheck(List<SButton>? keysToCheck)
        {
            // Null check. Do nothing, return false.
            if(keysToCheck == null)
                return false;

            // If not null. then convert and check.
            IEnumerable<SButton> keys = keysToCheck as IEnumerable<SButton>;
            switch (keysToCheck[0])
            {
                case SButton.W:
                    if (keys.Intersect(warpToLocationComboKeys) == keys)
                    {
                        // Response constructor has dialogue key, then dialogue text.
                        List<Response> choices = new List<Response>()
                        {
                            new Response("dialogue_id1", "Farm"),
                            new Response("dialogue_id2", "Mountain"),
                            new Response("dialogue_id3", "Beach")
                        };
                        // GameStateQuery's are like SQL queries, with "ANY" = "SELECT" and each condition being listed separately in escaped quotes. No need for "AND", "OR", or such.
                        if (GameStateQuery.CheckConditions("ANY \"PLAYER_VISITED_LOCATION Current IslandSouth\" \"PLAYER_VISITED_LOCATION Current IslandWest\""))
                        {
                            choices.Add(new Response("dialogue_id4", "Island"));
                        }
                        if (GameStateQuery.CheckConditions("ANY \"PLAYER_VISITED_LOCATION Current Desert\""))
                        {
                            choices.Add(new Response("dialogue_id5", "Desert"));
                            if (GameStateQuery.CheckConditions("ANY \"PLAYER_FRIENDSHIP_POINTS Current Sandy 1500\""))
                            {
                                choices.Add(new Response("dialogue_id6", "The Oasis"));
                            }
                        }

                        Game1.currentLocation.createQuestionDialogue($"Warp to where?", choices.ToArray(), new GameLocation.afterQuestionBehavior(WarpDialogue));
                    }
                    break;
            }

            return false;
        }

        private void WarpDialogue(Farmer who, string dialogue_id)
        {
            switch (dialogue_id)
            {
                case "dialogue_id1":
                    //who.warpFarmer(new Warp())
                    if (Game1.GetFarmTypeID() == "Beach")
                    {
                        Game1.warpFarmer("Farm", 82, 29, false);
                    }
                    else if (Game1.GetFarmTypeID() == "FourCorners")
                    {
                        Game1.warpFarmer("Farm", 48, 39, false);
                    }
                    else if (Game1.GetFarmTypeID() == "Meadowlands")
                    {
                        Game1.warpFarmer("Farm", 71, 6, false);
                    }
                    else
                    {
                        Game1.warpFarmer("Farm", 48, 7, false);
                    }
                    break;
                case "dialogue_id2":
                    Game1.warpFarmer("Mountain", 31, 20, false);
                    break;
                case "dialogue_id3":
                    Game1.warpFarmer("Beach", 82, 29, false);
                    break;
                case "dialogue_id4":
                    Game1.warpFarmer("IslandSouth", 11, 11, false);
                    break;
                case "dialogue_id5":
                    Game1.warpFarmer("Desert", 35, 43, false);
                    break;
                case "dialogue_id6":
                    Game1.warpFarmer("SandyHouse", 2, 7, false);
                    break;
                default:
                    break;
            }
            // [TODO] Default value for now... eventually make a function to calculate energy to deduct based on magic skill & max stamina.
            who.Stamina -= (270 / 4);
            return;
        }

        
    }
}
