﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using StardewObject = StardewValley.Object;

namespace MushroomRancher
{
    internal class Patcher
    {
        private static MushroomRancher mod;

        public static void PatchAll(MushroomRancher mushroomRancher)
        {
            mod = mushroomRancher;

            var harmony = new Harmony(mod.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewObject), nameof(StardewObject.DayUpdate)),
               prefix: new HarmonyMethod(typeof(Patcher), nameof(MushroomIncubatorUpdate_Pre)));

            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               prefix: new HarmonyMethod(typeof(Patcher), nameof(SlimeHutchDayUpdate_Pre)));

            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               postfix: new HarmonyMethod(typeof(Patcher), nameof(SlimeHutchDayUpdate_Post)));

            harmony.Patch(
               original: AccessTools.Method(typeof(DustSpirit), nameof(DustSpirit.behaviorAtGameTick)),
               prefix: new HarmonyMethod(typeof(Patcher), nameof(BehaviorAtGameTick_LivingMushroom)));
        }

        public static bool BehaviorAtGameTick_LivingMushroom(DustSpirit __instance, ref bool ___chargingFarmer, ref bool ___seenFarmer)
        {
            try
            {
                if (___seenFarmer && __instance.currentLocation is SlimeHutch)
                {
                    var farmer = Utility.isThereAFarmerWithinDistance(__instance.getStandingPosition() / 64f, 3, __instance.currentLocation);

                    if (farmer == null)
                    {
                        ___chargingFarmer = false;
                        __instance.controller = null;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
                return true;
            }
        }

        public static bool SlimeHutchDayUpdate_Pre(SlimeHutch __instance, ref List<Monster> __state)
        {
            try
            {
                var spritesCount = 0;
                var fakeCapsCount = 0;

                // we collect and remove the non slimes, so we can run the base implementation for the slimes
                __state = new List<Monster>();

                for (int i = __instance.characters.Count - 1; i >= 0; i--)
                {
                    if (__instance.characters[i] is DustSpirit dust)
                    {
                        spritesCount++;
                        __state.Add(dust);
                        __instance.characters.RemoveAt(i);
                    }
                    else if (__instance.characters[i] is RockCrab crab && __instance.characters[i].Name.Equals("False Magma Cap"))
                    {
                        fakeCapsCount++;
                        __state.Add(crab);
                        __instance.characters.RemoveAt(i);
                    }
                }

                int waters = 0;
                int startIndex = Game1.random.Next(__instance.waterSpots.Length);

                for (int i = 0; i < __instance.waterSpots.Length; i++)
                {
                    if (__instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] && waters * 5 < spritesCount + fakeCapsCount)
                    {
                        waters++;
                        __instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] = false;
                    }
                }

                for (int numMushrooms = Math.Min((int)Math.Round(spritesCount / 2.5f, MidpointRounding.AwayFromZero), waters * 2); numMushrooms > 0; numMushrooms--)
                {
                    if (numMushrooms % 2 == 0)
                    {
                        waters--;
                    }

                    int tries = 50;
                    Vector2 tile = __instance.getRandomTile();
                    while ((!__instance.isTileLocationTotallyClearAndPlaceable(tile) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 12f) && tries > 0)
                    {
                        tile = __instance.getRandomTile();
                        tries--;
                    }

                    if (tries > 0)
                    {
                        __instance.Objects.Add(tile, new StardewObject(MushroomRancher.purpleMushroomId, 1, false, -1, 0)
                        {
                            IsSpawnedObject = true
                        });
                    }
                }

                for (int numMushrooms = Math.Min((int)Math.Round(fakeCapsCount / 2.5f, MidpointRounding.AwayFromZero), waters * 2); numMushrooms > 0; numMushrooms--)
                {
                    if (numMushrooms % 2 == 0)
                    {
                        waters--;
                    }

                    int tries = 50;
                    Vector2 tile = __instance.getRandomTile();
                    while ((!__instance.isTileLocationTotallyClearAndPlaceable(tile) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 12f) && tries > 0)
                    {
                        tile = __instance.getRandomTile();
                        tries--;
                    }

                    if (tries > 0)
                    {
                        __instance.Objects.Add(tile, new StardewObject(MushroomRancher.magmaCapId, 1, false, -1, 0)
                        {
                            IsSpawnedObject = true
                        });
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
                __state = new();
                return true;
            }
        }

        public static void SlimeHutchDayUpdate_Post(SlimeHutch __instance, ref List<Monster> __state)
        {
            try
            {
                // there is no AddRange for NetCollections
                foreach (var item in __state)
                {
                    __instance.characters.Add(item);
                }
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
            }
        }

        public static bool MushroomIncubatorUpdate_Pre(StardewObject __instance, GameLocation location)
        {
            try
            {
                if (__instance.Name.Equals("MushroomIncubator") && __instance.heldObject.Value != null)
                {
                    if (__instance.MinutesUntilReady > 0)
                    {
                        var minutesWorking = 2880 - __instance.MinutesUntilReady;

                        if (minutesWorking < 1440)
                        {
                            __instance.MinutesUntilReady = Game1.MasterPlayer.professions.Contains(2) ? 0 : Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                        }
                    }

                    if (__instance.MinutesUntilReady <= 0 && location is SlimeHutch && location.canSlimeHatchHere())
                    {
                        Monster monster = null;
                        Vector2 v = new Vector2((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y + 1) * 64f;
                        int heldId = __instance.heldObject.Value.ParentSheetIndex;

                        if (heldId == MushroomRancher.redMushroomId)
                        {
                            monster = MushroomRancher.CreateFriendlyLivingMushroom(v);
                        }
                        else if (heldId == MushroomRancher.magmaCapId)
                        {
                            monster = MushroomRancher.CreateFriendlyMagmaCap(v, mod);
                        }

                        if (monster != null)
                        {
                            // Game1.showGlobalMessage(slime.cute ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
                            Vector2 openSpot = Utility.recursiveFindOpenTileForCharacter(monster, location, __instance.TileLocation + new Vector2(0f, 1f), 10, false);
                            monster.setTilePosition((int)openSpot.X, (int)openSpot.Y);
                            location.characters.Add(monster);
                            __instance.heldObject.Value = null;
                            __instance.MinutesUntilReady = -1;
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);

                return true;
            }
        }
    }
}