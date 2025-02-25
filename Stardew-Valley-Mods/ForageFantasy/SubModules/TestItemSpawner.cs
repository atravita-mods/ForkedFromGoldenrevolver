﻿namespace ForageFantasy
{
    using Microsoft.Xna.Framework;
    using StardewValley;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class TestItemSpawner
    {
        public static void TestValues(ForageFantasy mod, bool showOnlyTotals, bool ignoreSecretForest)
        {
            var seasons = new List<string>() { "summer" }; ////, "spring", "fall", "winter" };

            foreach (var season in seasons)
            {
                int[][] results = new int[7][];
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = new int[1000];
                }

                foreach (var loc in Game1.locations)
                {
                    var r = new Random(((int)Game1.uniqueIDForThisGame / 2) + (int)Game1.stats.DaysPlayed);
                    Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data/Locations");

                    int id;
                    switch (loc.Name)
                    {
                        case "BusStop":
                            id = 0;
                            break;

                        case "Forest":

                            id = 1;
                            break;

                        case "Mountain":

                            id = 2;
                            break;

                        case "Railroad":

                            id = 3;
                            break;

                        case "Woods":
                            if (ignoreSecretForest)
                            {
                                continue;
                            }

                            id = 4;
                            break;

                        case "Town":
                            id = 5;
                            break;

                        case "Backwoods":
                            id = 6;
                            break;

                        default:
                            continue;
                    }

                    if (locationData.ContainsKey(loc.Name))
                    {
                        string rawData = locationData[loc.Name].Split(new char[]
                        {
                    '/'
                        })[Utility.getSeasonNumber(season)];
                        //// && loc.numberOfSpawnedObjectsOnMap < 6)
                        if (!rawData.Equals("-1"))
                        {
                            string[] split = rawData.Split(new char[]
                            {
                        ' '
                            });
                            int numberToSpawn = 100000;
                            for (int i = 0; i < numberToSpawn; i++)
                            {
                                int xCoord = r.Next(loc.map.DisplayWidth / 64);
                                int yCoord = r.Next(loc.map.DisplayHeight / 64);
                                var location = new Vector2(xCoord, yCoord);
                                StardewValley.Object o;
                                //// loc.objects.TryGetValue(location, out StardewValley.Object o);
                                int whichObject = r.Next(split.Length / 2) * 2;
                                if (r.NextDouble() < Convert.ToDouble(split[whichObject + 1], CultureInfo.InvariantCulture))
                                {
                                    o = new StardewValley.Object(location, Convert.ToInt32(split[whichObject]), null, false, true, false, true);
                                    results[id][o.ParentSheetIndex]++;
                                }
                            }
                        }
                    }
                }

                int[] totals = new int[1000];

                float totaltotal = 0;
                for (int j = 0; j < results.Length; j++)
                {
                    if (j == 4 && ignoreSecretForest)
                    {
                        continue;
                    }

                    string s = string.Empty;
                    switch (j)
                    {
                        case 0:
                            s += "BusStop";
                            break;

                        case 1:
                            s += "Forest";
                            break;

                        case 2:
                            s += "Mountain";
                            break;

                        case 3:
                            s += "Railroad";
                            break;

                        case 4:
                            s += "Woods";
                            break;

                        case 5:
                            s += "Town";
                            break;

                        case 6:
                            s += "Backwoods";
                            break;
                    }

                    float total = 0;
                    for (int i = 0; i < results[j].Length; i++)
                    {
                        total += results[j][i];
                    }

                    totaltotal += total;
                    s += $"({total} items)";

                    if (!showOnlyTotals)
                    {
                        mod.DebugLog(s);
                        mod.DebugLog(string.Empty);
                    }

                    for (int i = 0; i < results[j].Length; i++)
                    {
                        if (results[j][i] > 0)
                        {
                            var o = new StardewValley.Object(new Vector2(0, 0), i, null, false, true, false, true);

                            int number = results[j][i];
                            float percentage = number / total;
                            string percentageString = percentage.ToString("P", CultureInfo.InvariantCulture);
                            if (!showOnlyTotals)
                            {
                                mod.DebugLog($"{o.DisplayName}: {number}, {percentageString}");
                            }
                        }

                        totals[i] += results[j][i];
                    }

                    if (!showOnlyTotals)
                    {
                        mod.DebugLog(string.Empty);
                    }
                }

                mod.DebugLog($"Total ({totaltotal} items)");

                mod.DebugLog(string.Empty);

                for (int i = 0; i < totals.Length; i++)
                {
                    if (totals[i] > 0)
                    {
                        var o = new StardewValley.Object(new Vector2(0, 0), i, null, false, true, false, true);

                        int number = totals[i];
                        float percentage = number / totaltotal;
                        string percentageString = percentage.ToString("P", CultureInfo.InvariantCulture);
                        mod.DebugLog($"{o.DisplayName}: {number}, {percentageString}");
                    }
                }

                mod.DebugLog(string.Empty);
            }
        }
    }
}