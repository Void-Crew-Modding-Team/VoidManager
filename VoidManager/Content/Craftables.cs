using ResourceAssets;
using System;
using System.Collections.Generic;

namespace VoidManager.Content
{
    internal class Craftables
    {
        public static Craftables Instance { get; internal set; }

        /// <summary>
        /// Sets recipe for GUID if previously-existing recipe exists.
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="craftingRules"></param>
        /// <exception cref="ArgumentException">An asset with the provided GUID does not exist.</exception>
        public void SetRecipe(GUIDUnion GUID, CraftingRules craftingRules)
        {
            if (!ModifiedRecipes.ContainsKey(GUID))
            {
                BepinPlugin.Log.LogError($"Attempted to modify recipe for object at GUID: {GUID}, however it has already been modified.");
                return;
            }

            if (!ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.TryGetByGuid(GUID, out CraftableItemDef asset))
            {
                throw new ArgumentException("An asset with the provided GUID does not exist.");
                //CraftableItemDef does not exist, so add CraftableItemDef
                /*
                CraftableItemDef craftableItemDef = new CraftableItemDef();
                craftableItemDef.Ref.AssetGuid = GUID;
                craftableItemDef.crafting = craftingRules;
                craftableItemDef.ContextInfo = new // Not sure how this will be handled with a null object, so I'm not gonna make false promises.
                ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.AssetDescriptions.Add(craftableItemDef);
                ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.RebuildLUT();
                */
            }
            else
            {
                ModifiedRecipes.Add(GUID, asset.crafting);
                asset.crafting = craftingRules;
            }
        }

        public void SetRecipe(string GUID, CraftingRules craftingRules)
        {
            SetRecipe(new GUIDUnion(GUID), craftingRules);
        }

        public void SetRecipe(int[] GUID, CraftingRules craftingRules)
        {
            SetRecipe(new GUIDUnion(GUID), craftingRules);
        }


        /// <summary>
        /// Undoes recipe modification for the provided GUID
        /// </summary>
        /// <param name="GUID"></param>
        public void ResetRecipe(GUIDUnion GUID)
        {
            if (ModifiedRecipes.TryGetValue(GUID, out CraftingRules value))
            {
                ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.GetAssetDefById(GUID).crafting = value;
                ModifiedRecipes.Remove(GUID);
            }
        }

        public void ClearRecipe(string GUID)
        {
            ResetRecipe(new GUIDUnion(GUID));
        }

        public void ClearRecipe(int[] GUID)
        {
            ResetRecipe(new GUIDUnion(GUID));
        }


        /// <summary>
        /// Returns the recipe for the given GUID.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>CraftingRules for recipe of GUID</returns>
        public CraftingRules GetRecipe(GUIDUnion GUID)
        {
            return ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.GetAssetDefById(GUID).crafting;
        }

        public CraftingRules GetRecipe(string GUID)
        {
            return GetRecipe(new GUIDUnion(GUID));
        }

        public CraftingRules GetRecipe(int[] GUID)
        {
            return GetRecipe(new GUIDUnion(GUID));
        }

        //private List<GUIDUnion> AddedRecipes;
        private Dictionary<GUIDUnion, CraftingRules> ModifiedRecipes = new();
    }
}
