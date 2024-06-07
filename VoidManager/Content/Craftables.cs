using Gameplay.Quests;
using HarmonyLib;
using ResourceAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Core;
using VoidManager.Utilities;

namespace VoidManager.Content
{
    /// <summary>
    /// API for modifying CraftingRules of recipes.
    /// </summary>
    public class Craftables
    {
        /// <summary>
        /// Static instance of Craftables class.
        /// </summary>
        public static Craftables Instance { get; internal set; }

        /// <summary>
        /// CraftingRules
        /// </summary>
        static FieldInfo CraftingRulesFI = AccessTools.Field(typeof(CraftableItemDef), "crafting");

        /// <summary>
        /// List(CraftableItem)
        /// </summary>
        static FieldInfo CDLCraftablesListFI = AccessTools.Field(typeof(CraftableDataList), "craftables");

        /// <summary>
        /// CraftingRules
        /// </summary>
        static FieldInfo CraftableItemCRFI = AccessTools.Field(typeof(CraftableItem), "customCraftingRules");

        /// <summary>
        /// Bool
        /// </summary>
        static FieldInfo CraftableItemOverrideFI = AccessTools.Field(typeof(CraftableItem), "overrideCraftingRules");

        /// <summary>
        /// Get CraftableItemDef
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>CraftableItemDef</returns>
        public static CraftableItemDef GetCraftableItemDef(GUIDUnion GUID)
        {
            return ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.GetAssetDefById(GUID);
        }

        /// <summary>
        /// Attempt get CraftableItemDef
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="asset"></param>
        /// <returns>Success</returns>
        public static bool TryGetCraftableItemDef(GUIDUnion GUID, out CraftableItemDef asset)
        {
            return ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.TryGetByGuid(GUID, out asset);
        }

        //private List<GUIDUnion> AddedRecipes;
        private Dictionary<GUIDUnion, CraftingRules> ModifiedRecipes = new();

        /// <summary>
        /// Sets recipe for GUID if previously-existing recipe exists.
        /// </summary>
        /// <param name="GUID">Object GUID for recipe assignement</param>
        /// <param name="craftingRules">Rules object to assign</param>
        /// <exception cref="ArgumentException">An asset with the provided GUID does not exist.</exception>
        public void SetRecipe(GUIDUnion GUID, CraftingRules craftingRules)
        {
            CraftableItemDef asset = GetCraftableItemDef(GUID);

            if (!ModifiedRecipes.ContainsKey(GUID))
            {
                ModifiedRecipes.Add(GUID, asset.CraftingRules);
            }

            CraftingRulesFI.SetValue(asset, craftingRules);
        }


        /// <summary>
        /// Undoes recipe modification for the provided GUID
        /// </summary>
        /// <param name="GUID"></param>
        public void ResetRecipe(GUIDUnion GUID)
        {
            if (ModifiedRecipes.TryGetValue(GUID, out var value))
            {
                CraftingRulesFI.SetValue(GetCraftableItemDef(GUID), value);
                ModifiedRecipes.Remove(GUID);
            }
        }


        /// <summary>
        /// Returns the recipe for the given GUID.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>CraftingRules for recipe of GUID</returns>
        public CraftingRules GetRecipe(GUIDUnion GUID)
        {
            return GetCraftableItemDef(GUID).CraftingRules;
        }



        /// <summary>
        /// Returns whether the given GUID UnlockOptions was modified.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>UnlockOptions modified</returns>
        public bool RecipeModified(GUIDUnion GUID)
        {
            return ModifiedRecipes.ContainsKey(GUID);
        }


        /// <summary>
        /// Safely attempts to get CraftableItem List of a QuestAsset
        /// </summary>
        /// <param name="QuestAsset"></param>
        /// <param name="CraftableDataList"></param>
        /// <returns>Success</returns>
        public bool TryGetQuestCraftableList(QuestAsset QuestAsset, out List<CraftableItem> CraftableDataList)
        {
            if (QuestAsset != null)
            {
                CraftableDataList CDL = QuestAsset.CraftableDataList;
                if (CDL != null)
                {
                    CraftableDataList = (List<CraftableItem>)CDLCraftablesListFI.GetValue(CDL);
                    return true;
                }
                else
                {
                    BepinPlugin.Log.LogError("QuestAsset's CraftableDataList override was null. Try adding a CraftableDataList.");
                }
            }
            else
            {
                BepinPlugin.Log.LogError("QuestAsset is null");
            }
            CraftableDataList = null;
            return false;
        }

        /// <summary>
        /// Safely attempts to get CraftableItem List of a QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <param name="CraftableDataList"></param>
        /// <returns>Success</returns>
        public bool TryGetQuestCraftableList(GUIDUnion QuestGUID, out List<CraftableItem> CraftableDataList)
        {
            if (Game.TryGetQuestAsset(QuestGUID, out QuestAsset asset))
            {
                return TryGetQuestCraftableList(asset, out CraftableDataList);
            }
            CraftableDataList = null;
            return false;
        }


        /// <summary>
        /// Gets CraftableItem List of a QuestAsset
        /// </summary>
        /// <param name="QuestAsset">QuestAsset for inspection</param>
        /// <returns>List(CraftableItem)</returns>
        public List<CraftableItem> GetQuestCraftableList(QuestAsset QuestAsset)
        {
            if (QuestAsset == null)
            {
                throw new ArgumentException("QuestAsset is null");
            }
            if (QuestAsset.CraftableDataList == null)
            {
                throw new ArgumentException("QuestAsset's CraftableDataList override was null. Try adding a CraftableDataList.");

            }

            return (List<CraftableItem>)CDLCraftablesListFI.GetValue(QuestAsset.CraftableDataList);
        }

        /// <summary>
        /// Gets CraftableItem List of a QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <returns>List(CraftableItem)</returns>
        public List<CraftableItem> GetQuestCraftableList(GUIDUnion QuestGUID)
        {
            if (Game.TryGetQuestAsset(QuestGUID, out QuestAsset asset))
            {
                return GetQuestCraftableList(asset);
            }
            throw new ArgumentException("QuestAsset does not exist");
        }

        /// <summary>
        /// Retrieves CraftableItem from CraftableItemList
        /// </summary>
        /// <param name="CraftableDataList">CDL for inspection</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <returns>CraftableItem</returns>
        public CraftableItem ItemFromCraftableItemList(List<CraftableItem> CraftableDataList, GUIDUnion RecipeGUID)
        {
            return CraftableDataList.FirstOrDefault(value => value.AssetGuid == RecipeGUID);
        }

        /// <summary>
        /// Safely attempts to get CraftableItem of a QuestAsset
        /// </summary>
        /// <param name="QuestAsset">QuestAsset for inspection</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <param name="CI">Retrieved CraftableItem</param>
        /// <returns>Success</returns>
        public bool TryGetQuestCraftableItem(QuestAsset QuestAsset, GUIDUnion RecipeGUID, out CraftableItem CI)
        {
            if (TryGetQuestCraftableList(QuestAsset, out List<CraftableItem> CraftableDataList))
            {
                CI = ItemFromCraftableItemList(CraftableDataList, RecipeGUID);
                if (CI != null)
                {
                    return true;
                }
                else
                {
                    BepinPlugin.Log.LogError("Quest's CraftableDataList did not contain the specified recipe GUID");
                    return false;
                }
            }
            CI = null;
            return false;
        }

        /// <summary>
        /// Safely attempts to get CraftableItem of a QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <param name="CI">Retrieved CraftableItem</param>
        /// <returns>Success</returns>
        public bool TryGetQuestCraftableItem(GUIDUnion QuestGUID, GUIDUnion RecipeGUID, out CraftableItem CI)
        {
            if (TryGetQuestCraftableList(QuestGUID, out List<CraftableItem> CraftableDataList))
            {
                CI = ItemFromCraftableItemList(CraftableDataList, RecipeGUID);
                if (CI != null)
                    return true;
                else
                    BepinPlugin.Log.LogError("Quest's CraftableDataList did not contain the specified recipe GUID");
            }
            else
            {
                CI = null;
            }
            return false;
        }


        /// <summary>
        /// Attempts to get CraftableItem of a QuestAsset
        /// </summary>
        /// <param name="QuestAsset">QuestAsset for inspection</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <returns>CraftableItem</returns>
        public CraftableItem GetQuestCraftableItem(QuestAsset QuestAsset, GUIDUnion RecipeGUID)
        {
            return ItemFromCraftableItemList(GetQuestCraftableList(QuestAsset), RecipeGUID);
        }

        /// <summary>
        /// Attempts to get CraftableItem of a QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <returns>CraftableItem</returns>
        public CraftableItem GetQuestCraftableItem(GUIDUnion QuestGUID, GUIDUnion RecipeGUID)
        {
            return ItemFromCraftableItemList(GetQuestCraftableList(QuestGUID), RecipeGUID);
        }


        /// <summary>
        /// Gets CraftingRules for given QuestAsset
        /// </summary>
        /// <param name="QuestAsset">QuestAsset for inspection</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <returns>CraftingRules</returns>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public CraftingRules GetQuestRecipe(QuestAsset QuestAsset, GUIDUnion RecipeGUID)
        {
            if (TryGetQuestCraftableItem(QuestAsset, RecipeGUID, out CraftableItem CI))
            {
                return CI.CraftingRules;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Gets CraftingRules for given QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <returns>CraftingRules</returns>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public CraftingRules GetQuestRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID)
        {
            if (TryGetQuestCraftableItem(QuestGUID, RecipeGUID, out CraftableItem CI))
            {
                return CI.CraftingRules;
            }
            throw new ArgumentException();
        }


        /// <summary>
        /// Safely attempts to get CraftingRules for given QuestAsset
        /// </summary>
        /// <param name="QuestAsset">QuestAsset for modification</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <param name="CR">Retrieved CraftingRules</param>
        /// <returns>Success</returns>
        public bool TryGetQuestRecipe(QuestAsset QuestAsset, GUIDUnion RecipeGUID, out CraftingRules CR)
        {
            if (TryGetQuestCraftableItem(QuestAsset, RecipeGUID, out CraftableItem CI))
            {
                CR = CI.CraftingRules;
                return true;
            }
            else
            {
                CR = new CraftingRules();
                return false;
            }
        }

        /// <summary>
        /// Safely attempts to get CraftingRules for given QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <param name="CR">Retrieved CraftingRules</param>
        /// <returns>Success</returns>
        public bool TryGetQuestRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID, out CraftingRules CR)
        {
            if (TryGetQuestCraftableItem(QuestGUID, RecipeGUID, out CraftableItem CI))
            {
                CR = CI.CraftingRules;
                return true;
            }
            else
            {
                CR = new CraftingRules();
                return false;
            }
        }


        /// <summary>
        /// Sets CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="CIToModify">Craftable Item for Modification</param>
        /// <param name="CraftingRules">Assigned Crafting Recipe</param>
        /// <param name="OverrideDefaultRecipe">Override Default Recipe</param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void SetQuestRecipe(CraftableItem CIToModify, CraftingRules CraftingRules, bool OverrideDefaultRecipe = true)
        {
            CraftableItemCRFI.SetValue(CIToModify, CraftingRules);
            CraftableItemOverrideFI.SetValue(CIToModify, OverrideDefaultRecipe);
            BepinPlugin.Log.LogInfo($"Overwrote CraftingRules for {CIToModify.ContextInfo.HeaderText}.");
        }

        /// <summary>
        /// Sets CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="QuestGUID">Target Quest</param>
        /// <param name="RecipeGUID">Target Recipe</param>
        /// <param name="CraftingRules">Assigned Crafting Recipe</param>
        /// <param name="OverrideDefaultRecipe">Override Default Recipe</param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void SetQuestRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID, CraftingRules CraftingRules, bool OverrideDefaultRecipe = true)
        {
            CraftableItem CI = GetQuestCraftableItem(QuestGUID, RecipeGUID);
            SetQuestRecipe(CI, CraftingRules, OverrideDefaultRecipe);
        }


        /// <summary>
        /// Sets CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="CIToModify">Craftable Item for Modification</param>
        /// <param name="OverrideDefaultRecipe">Override Default Recipe</param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void SetQuestRecipe(CraftableItem CIToModify, bool OverrideDefaultRecipe)
        {
            CraftableItemOverrideFI.SetValue(CIToModify, OverrideDefaultRecipe);
            BepinPlugin.Log.LogInfo($"Overwrote CraftingRules Override for {CIToModify.ContextInfo.HeaderText}.");
        }

        /// <summary>
        /// Sets CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="QuestGUID"></param>
        /// <param name="RecipeGUID"></param>
        /// <param name="OverrideDefaultRecipe">Override Default Recipe</param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void SetQuestRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID, bool OverrideDefaultRecipe)
        {
            SetQuestRecipe(GetQuestCraftableItem(QuestGUID, RecipeGUID), OverrideDefaultRecipe);
        }


        /// <summary>
        /// Creates a CraftableItem based on parameters
        /// </summary>
        /// <param name="RecipeGUID"></param>
        /// <param name="CraftingRules"></param>
        /// <param name="OverrideDefaultRecipe"></param>
        /// <returns>Created CraftableItem</returns>
        public CraftableItem CreateCraftableItem(GUIDUnion RecipeGUID, CraftingRules CraftingRules, bool OverrideDefaultRecipe = true)
        {
            CraftableItem CI = new();

            CI.craftItemRef = (CraftableItemRef)GetCraftableItemDef(RecipeGUID).Ref;
            CraftableItemCRFI.SetValue(CI, CraftingRules);
            CraftableItemOverrideFI.SetValue(CI, OverrideDefaultRecipe);

            return CI;
        }

        /// <summary>
        /// Creates a CraftableItem using default recipe
        /// </summary>
        /// <param name="RecipeGUID"></param>
        /// <returns>Created CraftableItem</returns>
        public CraftableItem CreateCraftableItemUsingDefaultRecipe(GUIDUnion RecipeGUID)
        {
            CraftableItem CI = new();

            CI.craftItemRef = (CraftableItemRef)GetCraftableItemDef(RecipeGUID).Ref;
            CraftableItemOverrideFI.SetValue(CI, false);

            return CI;
        }


        /// <summary>
        /// Sets CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="QuestAsset"></param>
        /// <param name="RecipeGUID"></param>
        /// <param name="CraftingRules"></param>
        /// <param name="OverrideDefaultRecipe">Override Default Recipe</param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void AddQuestRecipe(QuestAsset QuestAsset, GUIDUnion RecipeGUID, CraftingRules CraftingRules, bool OverrideDefaultRecipe = true)
        {
            CraftableItem CI = CreateCraftableItem(RecipeGUID, CraftingRules, OverrideDefaultRecipe);
            GetQuestCraftableList(QuestAsset).Add(CI);
            BepinPlugin.Log.LogInfo($"Overwrote CraftingRules Override for {CI.ContextInfo.HeaderText} contained in QuestOverrides for quest '{QuestAsset.DisplayName}'");
        }

        /// <summary>
        /// Adds CraftingRules Recipe Override for given QuestAsset
        /// </summary>
        /// <param name="QuestGUID"></param>
        /// <param name="RecipeGUID"></param>
        /// <param name="CraftingRules"></param>
        /// <param name="OverrideDefaultRecipe">Override Default Recipe</param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void AddQuestRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID, CraftingRules CraftingRules, bool OverrideDefaultRecipe = true)
        {
            CraftableItem CI = CreateCraftableItem(RecipeGUID, CraftingRules, OverrideDefaultRecipe);
            GetQuestCraftableList(QuestGUID).Add(CI);
            BepinPlugin.Log.LogInfo($"Added CraftingRules Override for {CI.ContextInfo.HeaderText} contained in QuestOverrides for questGUID '{QuestGUID}'");
        }


        /// <summary>
        /// Adds CraftingRules Override for given QuestAsset using default recipe
        /// </summary>
        /// <param name="QuestGUID"></param>
        /// <param name="RecipeGUID"></param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void AddQuestRecipeUsingDefaultRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID)
        {
            CraftableItem CI = CreateCraftableItemUsingDefaultRecipe(RecipeGUID);
            GetQuestCraftableList(QuestGUID).Add(CI);
            BepinPlugin.Log.LogInfo($"Added CraftingRules Override for {CI.ContextInfo.HeaderText} contained in QuestOverrides for questGUID '{QuestGUID}'");
        }

        /// <summary>
        /// Adds CraftingRules Override for given QuestAsset using default recipe
        /// </summary>
        /// <param name="QuestAsset"></param>
        /// <param name="RecipeGUID"></param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void AddQuestRecipeUsingDefaultRecipe(QuestAsset QuestAsset, GUIDUnion RecipeGUID)
        {
            CraftableItem CI = CreateCraftableItemUsingDefaultRecipe(RecipeGUID);
            GetQuestCraftableList(QuestAsset).Add(CI);
            BepinPlugin.Log.LogInfo($"Added CraftingRules Override for {CI.ContextInfo.HeaderText} contained in QuestOverrides for QuestAsset '{QuestAsset.DisplayName}'");
        }


        /// <summary>
        /// Removes CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="QuestAsset"></param>
        /// <param name="RecipeGUID"></param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void RemoveQuestRecipe(QuestAsset QuestAsset, GUIDUnion RecipeGUID)
        {
            List<CraftableItem> CIL = GetQuestCraftableList(QuestAsset);
            CraftableItem CI = CIL.FirstOrDefault(item => item.AssetGuid == RecipeGUID);

            //Didn't find Recipe by GUID
            if (CI == null)
            {
                BepinPlugin.Log.LogInfo($"Recipe {RecipeGUID} not found in QuestAsset");
                return;
            }

            CIL.Remove(CI);
            BepinPlugin.Log.LogInfo($"Removed CraftingRules Override for {CI.ContextInfo.HeaderText} contained in QuestOverrides for QuestAsset '{QuestAsset.DisplayName}'");
        }

        /// <summary>
        /// Removes CraftingRules Override for given QuestAsset
        /// </summary>
        /// <param name="QuestGUID"></param>
        /// <param name="RecipeGUID"></param>
        /// <exception cref="ArgumentException">QuestGUID or RecipeGUID were not valid</exception>
        public void RemoveQuestRecipe(GUIDUnion QuestGUID, GUIDUnion RecipeGUID)
        {
            List<CraftableItem> CIL = GetQuestCraftableList(QuestGUID);
            CraftableItem CI = CIL.FirstOrDefault(item => item.AssetGuid == RecipeGUID);

            //Didn't find Recipe by GUID
            if (CI == null)
            {
                BepinPlugin.Log.LogInfo($"Recipe {RecipeGUID} not found in QuestAsset");
                return;
            }

            CIL.Remove(CI);
            BepinPlugin.Log.LogInfo($"Removed CraftingRules Override for {CI.ContextInfo.HeaderText} contained in QuestOverrides for QuestGUID '{QuestGUID}'");
        }
    }
}
