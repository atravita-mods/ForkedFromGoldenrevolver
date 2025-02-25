﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using xTile.Dimensions;
using StardewObject = StardewValley.Object;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace MaritimeSecrets
{
    internal class Patcher
    {
        private static MaritimeSecrets mod;

        private static readonly string wormBinUpgradeKey = $"{mod?.ModManifest?.UniqueID}/WormBinUpgrade";
        private static readonly string summerForageCalendarKey = $"{mod?.ModManifest?.UniqueID}/SummerForageCalendar";

        private static readonly string receivedLifeSaverKey = $"{mod?.ModManifest?.UniqueID}/ReceivedLifeSaver";
        private static readonly string receivedFrogHatKey = $"{mod?.ModManifest?.UniqueID}/ReceivedFrogHat";
        private static readonly string receivedTrashCanKey = $"{mod?.ModManifest?.UniqueID}/ReceivedTrashCan";
        private static readonly string receivedWallBasketKey = $"{mod?.ModManifest?.UniqueID}/ReceivedWallBasket";
        private static readonly string receivedPyramidDecalKey = $"{mod?.ModManifest?.UniqueID}/ReceivedPyramidDecal";
        private static readonly string receivedGourmandStatueKey = $"{mod?.ModManifest?.UniqueID}/ReceivedGourmandStatue";

        private static readonly XnaRectangle summerForage = new(144, 256, 16, 16);
        private const int driftWoodId = 169;
        private const int pearlId = 797;
        private const int mermaidPendant = 460;

        public static void PatchAll(MaritimeSecrets maritimeSecrets)
        {
            mod = maritimeSecrets;

            var harmony = new Harmony(mod.ModManifest.UniqueID);

            try
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                   prefix: new HarmonyMethod(typeof(Patcher), nameof(GameLocation_AnswerDialogueAction_Prefix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new Type[] { typeof(string), typeof(Response[]), typeof(string) }),
                   prefix: new HarmonyMethod(typeof(Patcher), nameof(GameLocation_CreateQuestionDialogue_Prefix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
                   prefix: new HarmonyMethod(typeof(Patcher), nameof(Beach_CheckAction_Prefix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(Billboard), nameof(Billboard.performHoverAction)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.Billboard_PerformHoverAction_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(Billboard), nameof(Billboard.draw), new Type[] { typeof(SpriteBatch) }),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Billboard_Draw_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(BoatTunnel), nameof(BoatTunnel.getFish)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.BoatTunnel_GetFish_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(Town), nameof(Town.getFish)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.Town_GetFish_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(Woods), nameof(Woods.getFish)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.Woods_GetFish_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(Desert), nameof(Desert.getFish)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.Desert_GetFish_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(IslandSouthEastCave), nameof(IslandSouthEastCave.getFish)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.IslandSouthEastCave_GetFish_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(IslandFarmCave), nameof(IslandFarmCave.getFish)),
                   postfix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.IslandFarmCave_GetFish_Postfix)));

                harmony.Patch(
                   original: AccessTools.Method(typeof(StardewObject), nameof(StardewObject.performObjectDropInAction), new[] { typeof(Item), typeof(bool), typeof(Farmer) }),
                   prefix: new HarmonyMethod(typeof(Patcher), nameof(WormBin_PerformObjectDropInAction)));
            }
            catch (Exception e)
            {
                mod.ErrorLog("Error while trying to setup required patches:", e);
            }
        }

        // patch with very high priority so we get called before mods like 'Free Love' who want to change the cost
        // if people want to use the 'Free Love' feature they can disable my feature, but not the other way around
        [HarmonyPriority(Priority.High)]
        public static bool GameLocation_AnswerDialogueAction_Prefix(string questionAndAnswer, ref bool __result)
        {
            try
            {
                if (!mod.Config.ChangePendantPriceToPearl)
                {
                    return true;
                }

                if (questionAndAnswer == "mariner_Buy")
                {
                    if (Game1.player.hasItemInInventory(pearlId, 1))
                    {
                        Game1.player.removeItemsFromInventory(pearlId, 1);
                        Game1.player.addItemByMenuIfNecessary(new StardewObject(mermaidPendant, 1, false, -1, 0)
                        {
                            specialItem = true
                        }, null);

                        if (Game1.activeClickableMenu == null)
                        {
                            Game1.player.holdUpItemThenMessage(new StardewObject(mermaidPendant, 1, false, -1, 0), true);
                        }
                    }
                    else
                    {
                        // this means 'I need {0}, if it's not too inconvenient.'
                        var iNeedString = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", $"1 {new StardewObject(pearlId, 1).DisplayName}");
                        Game1.drawObjectDialogue(iNeedString);
                    }

                    __result = true;
                    return false;
                }
                else if (questionAndAnswer == "Mariner_Buy")
                {
                    // the capital mariner case is probably no longer used by the game or was used for debugging, but let's change it anyway (the mod 'Free Love' does not change it for example)
                    if (Game1.player.hasItemInInventory(pearlId, 1))
                    {
                        Game1.player.removeItemsFromInventory(pearlId, 1);

                        Game1.player.grabObject(new StardewObject(Vector2.Zero, mermaidPendant, null, false, true, false, false));
                    }
                    else
                    {
                        // this means 'I need {0}, if it's not too inconvenient.'
                        var iNeedString = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", $"1 {new StardewObject(pearlId, 1).DisplayName}");
                        Game1.drawObjectDialogue(iNeedString);
                    }

                    __result = true;
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
                return true;
            }
        }

        // patch with very high priority so this gets called if and only if GameLocation_AnswerDialogueAction_Prefix didn't get overridden by a mod that patches both with even higher priority
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool GameLocation_CreateQuestionDialogue_Prefix(string question, ref Response[] answerChoices, string dialogKey)
        {
            try
            {
                if (!mod.Config.ChangePendantPriceToPearl)
                {
                    return true;
                }

                if (dialogKey == "mariner")
                {
                    foreach (var item in answerChoices)
                    {
                        if (item.responseKey == "Buy")
                        {
                            var buyString = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1654");
                            item.responseText = $"{buyString} (1 {new StardewObject(pearlId, 1).DisplayName})";
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

        public static bool WormBin_PerformObjectDropInAction(ref StardewObject __instance, ref bool __result, ref Item dropInItem, ref bool probe, ref Farmer who)
        {
            try
            {
                if (!probe && __instance.name.Equals("Worm Bin") && who?.modData?.ContainsKey(wormBinUpgradeKey) == true)
                {
                    if (__instance.heldObject.Value != null && !__instance.readyForHarvest.Value && __instance.heldObject.Value.Stack <= 5)
                    {
                        if (dropInItem is StardewObject o && o.ParentSheetIndex == driftWoodId && who.IsLocalPlayer)
                        {
                            who.consumeObject(driftWoodId, 1);
                            // __instance.ConsumeInventoryItem(who, driftWoodId, 1);

                            who.currentLocation.playSound("slimeHit", NetAudio.SoundContext.Default);

                            var newStackSize = Game1.random.Next(8, 11);

                            __instance.heldObject.Value.Stack = newStackSize;

                            __result = false;
                            return false;
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

        public static void Billboard_PerformHoverAction_Postfix(ref Billboard __instance, int x, int y, ref string ___hoverText)
        {
            try
            {
                if (__instance.calendarDays != null && Game1.player?.modData?.ContainsKey(summerForageCalendarKey) == true)
                {
                    for (int day = 0; day < __instance.calendarDays.Count;)
                    {
                        ClickableTextureComponent c = __instance.calendarDays[day++];

                        if (c.bounds.Contains(x, y))
                        {
                            if (Game1.IsSummer && 12 <= day && day <= 14)
                            {
                                if (___hoverText.Length > 0)
                                {
                                    ___hoverText += Environment.NewLine;
                                }

                                ___hoverText += mod.Helper.Translation.Get("GreenOcean");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
            }
        }

        public static void Billboard_Draw_Postfix(Billboard __instance, SpriteBatch b, bool ___dailyQuestBoard, string ___hoverText)
        {
            try
            {
                if (!___dailyQuestBoard)
                {
                    for (int day = 1; day <= 28; day++)
                    {
                        XnaRectangle toDraw = XnaRectangle.Empty;

                        if (Game1.IsSummer && 12 <= day && day <= 14 && Game1.player?.modData?.ContainsKey(summerForageCalendarKey) == true)
                        {
                            toDraw = summerForage;
                        }

                        if (toDraw != XnaRectangle.Empty)
                        {
                            Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)(__instance.calendarDays[day - 1].bounds.X + 12), (float)(__instance.calendarDays[day - 1].bounds.Y + 60) - Game1.dialogueButtonScale / 2f), toDraw, Color.White, 0f, Vector2.Zero, 2f, false, 1f, -1, -1, 0.35f);

                            Game1.mouseCursorTransparency = 1f;
                            __instance.drawMouse(b);
                            if (___hoverText.Length > 0)
                            {
                                IClickableMenu.drawHoverText(b, ___hoverText, Game1.dialogueFont);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
            }
        }

        public static void IslandFarmCave_GetFish_Postfix(IslandFarmCave __instance, Farmer who)
        {
            try
            {
                // if frog hat got dropepd (it's not added to the return value)
                foreach (var item in __instance.debris)
                {
                    if (item?.item is Hat hat && hat.which.Value == 78)
                    {
                        if (mod?.ModManifest?.UniqueID != null)
                        {
                            who.modData[receivedFrogHatKey] = "true";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
            }
        }

        private static void CheckForFurnitureItem(Farmer who, StardewObject fishingDrop, int furnitureId, string moddedKeyToAdd)
        {
            try
            {
                if (fishingDrop is Furniture furniture && furniture.ParentSheetIndex == furnitureId)
                {
                    if (mod?.ModManifest?.UniqueID != null)
                    {
                        who.modData[moddedKeyToAdd] = "true";
                    }
                }
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
            }
        }

        public static void BoatTunnel_GetFish_Postfix(Farmer who, StardewObject __result)
        {
            CheckForFurnitureItem(who, __result, 2418, receivedLifeSaverKey);
        }

        public static void Town_GetFish_Postfix(Farmer who, StardewObject __result)
        {
            CheckForFurnitureItem(who, __result, 2427, receivedTrashCanKey);
        }

        public static void Woods_GetFish_Postfix(Farmer who, StardewObject __result)
        {
            CheckForFurnitureItem(who, __result, 2425, receivedWallBasketKey);
        }

        public static void Desert_GetFish_Postfix(Farmer who, StardewObject __result)
        {
            CheckForFurnitureItem(who, __result, 2334, receivedPyramidDecalKey);
        }

        public static void IslandSouthEastCave_GetFish_Postfix(Farmer who, StardewObject __result)
        {
            CheckForFurnitureItem(who, __result, 2332, receivedGourmandStatueKey);
        }

        // patch with very high priority so we get called before mods like 'Free Love' which want to remove the marriage restriction
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool Beach_CheckAction_Prefix(Beach __instance, Location tileLocation, Farmer who, NPC ___oldMariner, ref bool __result)
        {
            try
            {
                if (who != null && ___oldMariner != null && ___oldMariner.getTileX() == tileLocation.X && ___oldMariner.getTileY() == tileLocation.Y)
                {
                    if (mod?.ModManifest?.UniqueID == null || who.modData.ContainsKey(mod.talkedToMarinerTodayKey))
                    {
                        return true;
                    }

                    who.modData[mod.talkedToMarinerTodayKey] = "true";

                    var marinerName = GetMarinerName();

                    string speechTypeSuffix = "";

                    if ((mod.Config.MarinerSpeechType == SpeechType.dynamic && !mod.IsUsingMermaidMod) || mod.Config.MarinerSpeechType == SpeechType.sailor)
                    {
                        speechTypeSuffix = "_Sailor";
                    }

                    // maybe I should refactor everything to a string builder instead, but it doesn't seem worth it yet for only about 3 string concatenations
                    string secret = mod.Helper.Translation.Get("Secret" + speechTypeSuffix, new { name = marinerName }) + " ";

                    __result = true;

                    // major secrets

                    if (!who.modData.ContainsKey(summerForageCalendarKey))
                    {
                        who.modData[summerForageCalendarKey] = "true";
                        Game1.drawObjectDialogue(Game1.parseText(secret + mod.Helper.Translation.Get("SummerForageCalendar" + speechTypeSuffix)));
                        return false;
                    }

                    // unlike the netfield, the fishing level property also adds buffs and enchantments. the crafting recipe OR check is for mod compatibility
                    if ((who.fishingLevel.Value >= 8 || who.craftingRecipes.ContainsKey("Worm Bin")) && !who.modData.ContainsKey(wormBinUpgradeKey))
                    {
                        who.modData[wormBinUpgradeKey] = "true";
                        Game1.drawObjectDialogue(Game1.parseText(secret + mod.Helper.Translation.Get("WormBinUpgrade" + speechTypeSuffix)));
                        return false;
                    }

                    // removed the 'who.hasMagnifyingGlass' condition so you can do it in year 1
                    // the 'Vanilla' name check is for compatibility with Nayas mermaid mod for lore consistency
                    if (!who.secretNotesSeen.Contains(15))
                    {
                        who.secretNotesSeen.Add(15);

                        // yes, if people set the speech pattern to sailor and use a mermaid mod, the mermaid will say the mermaid specific line like a sailor
                        string transl = mod.IsUsingMermaidMod ? marinerName == "Vanilla" ? "MermaidSecretNote_MermaidBlueHair" : "MermaidSecretNote_Mermaid" : "MermaidSecretNote";

                        Game1.drawObjectDialogue(Game1.parseText(secret + mod.Helper.Translation.Get(transl + speechTypeSuffix)));
                        return false;
                    }

                    if (Game1.stats.DaysPlayed >= 31 && !Game1.IsWinter && (!who.mailReceived.Contains("gotSpaFishing") || !FoundSpaNecklace(who)))
                    {
                        if (!who.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX))
                        {
                            who.secretNotesSeen.Add(GameLocation.NECKLACE_SECRET_NOTE_INDEX);
                        }

                        // xor
                        bool oneDone = who.mailReceived.Contains("gotSpaFishing") ^ FoundSpaNecklace(who);

                        string transl = oneDone ? "SpaPaintingOrNecklace" : "SpaPaintingAndNecklace";

                        Game1.drawObjectDialogue(Game1.parseText(secret + mod.Helper.Translation.Get(transl + speechTypeSuffix)));
                        return false;
                    }

                    // minor secrets

                    string translation = null;

                    bool selectedMinorSecret = false;
                    while (!selectedMinorSecret)
                    {
                        if (Game1.whichFarm == Farm.beach_layout && !who.mailReceived.Contains("gotBoatPainting"))
                        {
                            translation = "BeachFarmBoatPainting";
                            selectedMinorSecret = true;
                            break;
                        }

                        if (!who.modData.ContainsKey(receivedTrashCanKey))
                        {
                            if (CheckJojaMartComplete())
                            {
                                translation = "TownTrashCanJojaWareHouse";
                            }
                            else if (CheckCommunityCenterComplete())
                            {
                                translation = "TownTrashCanRestoredCommunityCenter";
                            }
                            else
                            {
                                translation = "TownTrashCanBrokenCommunityCenter";
                            }

                            selectedMinorSecret = true;
                            break;
                        }

                        // if caught woodskip as secret woods condition
                        if (who.fishCaught.ContainsKey(734) && !who.modData.ContainsKey(receivedWallBasketKey))
                        {
                            translation = "SecretWoodsWallBasket";
                            selectedMinorSecret = true;
                            break;
                        }

                        if (CheckDesertUnlocked() && !who.modData.ContainsKey(receivedPyramidDecalKey))
                        {
                            translation = "DesertPyramidDecal";
                            selectedMinorSecret = true;
                            break;
                        }

                        if (Utility.doesAnyFarmerHaveOrWillReceiveMail("seenBoatJourney"))
                        {
                            if (!who.modData.ContainsKey(receivedLifeSaverKey))
                            {
                                translation = "WillyLifeSaver";
                                selectedMinorSecret = true;
                                break;
                            }

                            if (who.hasOrWillReceiveMail("talkedToGourmand"))
                            {
                                if (!who.modData.ContainsKey(receivedFrogHatKey))
                                {
                                    translation = "IslandGourmandStatue";
                                    selectedMinorSecret = true;
                                    break;
                                }
                            }

                            if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge"))
                            {
                                if (!who.mailReceived.Contains("gotSecretIslandNSquirrel"))
                                {
                                    translation = "IslandSquirrel";
                                    selectedMinorSecret = true;
                                    break;
                                }
                            }

                            if (Game1.MasterPlayer.hasOrWillReceiveMail("reachedCaldera"))
                            {
                                if (!who.mailReceived.Contains("CalderaPainting"))
                                {
                                    translation = "VolcanoPainting";
                                    selectedMinorSecret = true;
                                    break;
                                }
                            }

                            if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_Resort"))
                            {
                                if (!who.modData.ContainsKey(receivedGourmandStatueKey))
                                {
                                    translation = "IslandGourmandStatue";
                                    selectedMinorSecret = true;
                                    break;
                                }
                            }

                            if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_Turtle"))
                            {
                                if (!who.mailReceived.Contains("gotSecretIslandNPainting"))
                                {
                                    translation = "IslandPainting";
                                    selectedMinorSecret = true;
                                    break;
                                }
                            }
                        }

                        // unlike the netfield, the fishing level property also adds buffs and enchantments
                        if (who.fishingLevel.Value >= 10 && !Game1.player.mailReceived.Contains("caughtIridiumKrobus"))
                        {
                            // now we want to calculate with boosts
                            translation = who.FishingLevel < 15 ? "IridiumKrobusNotReady" : "IridiumKrobusReady";

                            selectedMinorSecret = true;
                            break;
                        }

                        break;
                    }

                    // if managed to place a crate
                    if (TryToSpawnSupplyCrate(__instance))
                    {
                        if (selectedMinorSecret && translation != null)
                        {
                            Game1.drawObjectDialogue(Game1.parseText(secret + mod.Helper.Translation.Get(translation) + " " + mod.Helper.Translation.Get("AlsoBeachSupplyCrate" + speechTypeSuffix)));
                        }
                        else
                        {
                            Game1.drawObjectDialogue(Game1.parseText(secret + mod.Helper.Translation.Get("BeachSupplyCrate" + speechTypeSuffix)));
                        }

                        return false;
                    }

                    // if somehow failed to place a crate
                    Game1.drawObjectDialogue(Game1.parseText(mod.Helper.Translation.Get("NoSecret" + speechTypeSuffix, new { name = marinerName })));
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                mod.ErrorLog("There was an exception in a patch", e);
                return true;
            }
        }

        private static string GetMarinerName()
        {
            var sampleMarinerSentence = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerMarried");

            // some asian languages use a different unicode colon character, the fullwidth colon.
            // for some edge cases, this is checked before the normal unicode colon
            var index = sampleMarinerSentence.IndexOf('：');

            if (index < 0)
            {
                index = sampleMarinerSentence.IndexOf(':');
            }

            if (index > 0)
            {
                return sampleMarinerSentence[..index];
            }
            else
            {
                // if we couldn't find a name, use fallback
                return mod.Helper.Translation.Get("OldMarinerName");
            }
        }

        private static bool TryToSpawnSupplyCrate(Beach beach)
        {
            var whichSupplyCrate = Game1.random.Next(922, 925);

            // arbitrary max number (it's about 500*2)
            int maxTries = beach.Map.Layers[0].LayerWidth * beach.Map.Layers[0].LayerHeight * 2;

            for (int tries = 0; tries < maxTries; tries++)
            {
                Vector2 randomPosition = beach.getRandomTile();
                randomPosition.Y /= 2f;
                randomPosition.X /= 2f;

                if (randomPosition.X < 10)
                {
                    randomPosition.X += 10;
                }

                if (randomPosition.Y < 10)
                {
                    randomPosition.Y += 15;
                }

                string prop = beach.doesTileHaveProperty((int)randomPosition.X, (int)randomPosition.Y, "Type", "Back");

                if (beach.isTileLocationTotallyClearAndPlaceable(randomPosition) && (prop == null || !prop.Equals("Wood")))
                {
                    beach.dropObject(new StardewObject(whichSupplyCrate, 1, false, -1, 0)
                    {
                        Fragility = 2,
                        MinutesUntilReady = 3
                    }, randomPosition * 64f, Game1.viewport, true, null);

                    return true;
                }
            }

            return false;
        }

        private static bool FoundSpaNecklace(Farmer who)
        {
            return who.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX) && who.hasOrWillReceiveMail(GameLocation.CAROLINES_NECKLACE_MAIL);
        }

        private static bool CheckDesertUnlocked()
        {
            return Game1.MasterPlayer.mailReceived.Contains("ccVault") || Game1.MasterPlayer.mailReceived.Contains("jojaVault");
        }

        private static bool CheckCommunityCenterComplete()
        {
            return Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.hasCompletedCommunityCenter();
        }

        // based on Utility.hasFinishedJojaRoute, just replaced Game1.player with Game1.MasterPlayer
        private static bool CheckJojaMartComplete()
        {
            bool flag = false;
            if (Game1.MasterPlayer.mailReceived.Contains("jojaVault"))
            {
                flag = true;
            }
            else if (!Game1.MasterPlayer.mailReceived.Contains("ccVault"))
            {
                return false;
            }

            if (Game1.MasterPlayer.mailReceived.Contains("jojaPantry"))
            {
                flag = true;
            }
            else if (!Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
            {
                return false;
            }

            if (Game1.MasterPlayer.mailReceived.Contains("jojaBoilerRoom"))
            {
                flag = true;
            }
            else if (!Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
            {
                return false;
            }

            if (Game1.MasterPlayer.mailReceived.Contains("jojaCraftsRoom"))
            {
                flag = true;
            }
            else if (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom"))
            {
                return false;
            }

            if (Game1.MasterPlayer.mailReceived.Contains("jojaFishTank"))
            {
                flag = true;
            }
            else if (!Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
            {
                return false;
            }

            if (flag || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
            {
                return true;
            }

            return false;
        }
    }
}