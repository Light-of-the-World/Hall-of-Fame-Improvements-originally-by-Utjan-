using SPT.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChatShared;
using Comfort.Logs;
using EFT;
using EFT.HandBook;
using EFT.Hideout;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Interactive;
using HallOfFameImprovements.Patches;

namespace HallOfFameImprovements
{
    [BepInPlugin("com.utjanandlight.HoFImprovements", "utjanandlight.HoFImprovements", "2.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        internal static ConfigEntry<bool> enabledPlugin;
        internal static ConfigEntry<double> bonusMultiplierNonDogtagItems;
        internal static ConfigEntry<double> bonusMultiplierDogtags;
        internal static ConfigEntry<double> uniqueItemBonus;
        internal static ConfigEntry<bool> uniqueBonusOnlyFIR;
        internal static ConfigEntry<double> FIRmultiplier;
        internal static ConfigEntry<double> nonFIRmultiplier;

        private void Awake() //Awake() will run once when your plugin loads
        {
            enabledPlugin = Config.Bind(
                "Hall of Fame Improvements",
                "1. Enable Mod",
                true,
                new ConfigDescription("Enable Hall of Fame Improvements")
            );

            bonusMultiplierNonDogtagItems = Config.Bind(
                "Hall of Fame Improvements",
                "2. Trophy skill bonus multiplier",
                1d,
                new ConfigDescription("Multiplies the base bonus to Hall of Fame skill leveling bonus gained from non-dogtag items. 0.5 = half bonus gain. 2.0 = double bonus gain", new AcceptableValueRange<double>(0, 5))
            );

            bonusMultiplierDogtags = Config.Bind(
                "Hall of Fame Improvements",
                "3. Dogtag skill bonus multiplier",
                1d,
                new ConfigDescription("Multiplies the base bonus to Hall of Fame skill leveling bonus gained from dogtags. Set to 1 for default EFT bonus", new AcceptableValueRange<double>(0, 5))
            );

            uniqueItemBonus = Config.Bind(
                "Hall of Fame Improvements",
                "4. Unique trophy skill bonus",
                0.2d,
                new ConfigDescription("Hall of Fame skill leveling bonus gained per unique trophy item. Unique meaning one of each item. Applies on top of the bonus gained based on the item's value", new AcceptableValueRange<double>(0, 1))
            );

            uniqueBonusOnlyFIR = Config.Bind(
                "Hall of Fame Improvements",
                "5. Only FIR trophies give unique bonus",
                true,
                new ConfigDescription("Only give the unique bonus for a trophy if it is tagged as Found-In-Raid")
            );

            FIRmultiplier = Config.Bind(
                "Hall of Fame Improvements",
                "6. Trophy found-in-raid multiplier",
                1d,
                new ConfigDescription("Multiplies the skill bonus from trophies that are tagged as Found-In-Round. Does not apply to Unique trophy bonus", new AcceptableValueRange<double>(0, 1))
            );

            nonFIRmultiplier = Config.Bind(
                "Hall of Fame Improvements",
                "7. Trophy NOT found-in-raid multiplier",
                0.5d,
                new ConfigDescription("Multiplies the skill bonus from trophies that are NOT tagged as Found-In-Round. Does not apply to Unique trophy bonus", new AcceptableValueRange<double>(0, 1))
            );

            LogSource = Logger;

            new ApplyNonDogtagItemsPatch().Enable();
            new UpdateBonusOnItemSlottedPatch().Enable();

            LogSource.LogInfo("(HOFI) Hall of Fame Improvements has loaded! Thank you to Utjan for originally making this.");
        }
    }
}
