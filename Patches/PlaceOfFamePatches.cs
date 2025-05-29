using SPT.Reflection.Patching;
using EFT.HandBook;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;

namespace HallOfFameImprovements.Patches
{
    internal class ApplyNonDogtagItemsPatch : ModulePatch
    {
        public static Dictionary<string, HandbookData> hbData;

        public static List<string> uniqueItemList;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlaceOfFameBehaviour), nameof(PlaceOfFameBehaviour.method_15));
        }

        [PatchPostfix]
        static void Postfix(PlaceOfFameBehaviour __instance)
        {
            if (!Plugin.enabledPlugin.Value)
                return;

            Plugin.LogSource.LogWarning($"Calculating Hall of Fame bonus");

            GClass1598 gclass = __instance.Data.CurrentStage.Bonuses.Data.OfType<GClass1598>().FirstOrDefault<GClass1598>();
            if (gclass != null)
            {
                double levelingBonus = Traverse.Create(__instance).Field("double_0").GetValue<double>();

                levelingBonus *= Plugin.bonusMultiplierDogtags.Value;

                Plugin.LogSource.LogWarning($"Bonus from dogtags {levelingBonus}");

                uniqueItemList = new List<string>();
                CompoundItem lootItemClass = Traverse.Create(__instance).Field("compoundItem_0").GetValue<CompoundItem>();
                using (List<Item>.Enumerator enumerator = lootItemClass.GetAllItems().Where(new Func<Item, bool>(ItemIsNotDogtag)).ToList<Item>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        float price = GetItemHandbookPrice(enumerator.Current);
                        if (price <= 0)
                            continue;

                        double bonus = GetBuffBonusFromPrice(price);
                        double uniqueBonus = GetUniqueBonus(enumerator.Current);
                        double FIRmult = enumerator.Current.MarkedAsSpawnedInSession ? Plugin.FIRmultiplier.Value : Plugin.nonFIRmultiplier.Value;

                        if (bonus > 0)
                        { bonus *= Plugin.bonusMultiplierNonDogtagItems.Value * FIRmult; }
                        levelingBonus += bonus + uniqueBonus;

                        Plugin.LogSource.LogWarning($"BONUS of {enumerator.Current.Name.Localized()} is {Math.Round(bonus, 3)} (unq {uniqueBonus}) and price {price} - FIR: {enumerator.Current.MarkedAsSpawnedInSession}");
                    }
                }
                BonusController bonusController_0 = Traverse.Create(__instance).Field("bonusController_0").GetValue<BonusController>();
                /*
                GClass1598 gclass4 = new GClass1598(gclass.ToDescriptor());//(Math.Round(levelingBonus, 1), gclass.BoostValue, gclass.Id, gclass.IsVisible, gclass.Icon);
                gclass4.Value = (Math.Round(levelingBonus, 1));
                gclass4.BoostValue = gclass.BoostValue;
                gclass4.Id = gclass.Id;
                bonusController_0.RemoveBonus(gclass, false);
                __instance.Data.CurrentStage.Bonuses.Data.Remove(gclass);
                __instance.Data.CurrentStage.Bonuses.Data.Add(gclass4);
                bonusController_0.AddBonus(gclass4, false);
                */

                GClass1996 gclass2 = gclass.ToDescriptor();
                gclass2.Value = (Math.Round(levelingBonus, 1));
                GClass1598 gclass3 = new GClass1598(gclass2);
                gclass3.BoostValue = gclass.BoostValue;
                __instance.bonusController_0.RemoveBonus(gclass, false);
                __instance.Data.CurrentStage.Bonuses.Data.Remove(gclass);
                __instance.Data.CurrentStage.Bonuses.Data.Add(gclass3);
                __instance.bonusController_0.AddBonus(gclass3, false);

                Plugin.LogSource.LogWarning($"HALL OF FAME BONUS UPDATED: {levelingBonus}");
            }
        }
        public static bool ItemIsNotDogtag(Item i)
        {
            return i.GetItemComponent<DogtagComponent>() == null;
        }

        public static double GetUniqueBonus(Item item)
        {
            if (Plugin.uniqueItemBonus.Value > 0 && !uniqueItemList.Contains(item.TemplateId))
            {
                if (!Plugin.uniqueBonusOnlyFIR.Value || item.MarkedAsSpawnedInSession)
                {
                    uniqueItemList.Add(item.TemplateId);
                    return Plugin.uniqueItemBonus.Value;
                }
            }
            return 0;
        }

        public static float GetItemHandbookPrice(Item lootItem)
        {
            if (hbData == null)
            {
                hbData = Singleton<HandbookClass>.Instance.Items.ToDictionary(
                    (item) => item.Id
                );
            }

            hbData.TryGetValue(lootItem.TemplateId, out HandbookData value);
            float price = value?.Price ?? 0;

            return price;
        }

        public static double GetBuffBonusFromPrice(float price)
        {
            if (price <= 0)
                return 0;
            double bonus = Math.Max( (Math.Log10(price) - 4) / 5 , 0.1);
            //double bonus = Math.Pow(price / 5000000, 0.4);
            return bonus;
        }
    }

    internal class UpdateBonusOnItemSlottedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlaceOfFameBehaviour), nameof(PlaceOfFameBehaviour.method_13));
        }

        [PatchPostfix]
        static void Postfix(PlaceOfFameBehaviour __instance, Item item, EFT.InventoryLogic.IContainer itemContainer, CommandStatus status)
        {
            if (!Plugin.enabledPlugin.Value)
                return;

            if (status != CommandStatus.Succeed)
            {
                return;
            }
            if (item.GetItemComponent<DogtagComponent>() == null && __instance.method_14(itemContainer))
            {
                __instance.method_15();
            }
        }
    }
}
