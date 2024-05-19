using ResourceAssets;
using System;
using System.Collections.Generic;

namespace VoidManager.Content
{
    public class Craftables
    {
        public static Craftables Instance { get; internal set; }

        //private List<GUIDUnion> AddedRecipes;
        private Dictionary<GUIDUnion, Tuple<string, CraftingRules>> ModifiedRecipes = new();

        /// <summary>
        /// Sets recipe for GUID if previously-existing recipe exists.
        /// </summary>
        /// <param name="GUID">Object GUID for recipe assignement</param>
        /// <param name="CallerID">Unique ID of calling method. Try using Mod GUID</param>
        /// <param name="craftingRules">Rules object to assign</param>
        /// <exception cref="ArgumentException">An asset with the provided GUID does not exist.</exception>
        /// <returns>Recipe succesfully modified</returns>
        public bool SetRecipe(GUIDUnion GUID, string CallerID, CraftingRules craftingRules)
        {
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
            else if (ModifiedRecipes.TryGetValue(GUID, out var value))
            {
                if (value.Item1 != CallerID)
                {
                    BepinPlugin.Log.LogError($"Attempted to modify recipe for object at GUID: {GUID}, however it has already been modified.");
                    return false;
                }
                else
                {
                    asset.crafting = craftingRules;
                    return true;
                }
            }
            else
            {
                ModifiedRecipes.Add(GUID, new Tuple<string, CraftingRules>(CallerID, asset.crafting));
                asset.crafting = craftingRules;
                return true;
            }
        }

        public bool SetRecipe(string GUID, string CallerID, CraftingRules craftingRules)
        {
            return SetRecipe(new GUIDUnion(GUID), CallerID, craftingRules);
        }

        public bool SetRecipe(int[] GUID, string CallerID, CraftingRules craftingRules)
        {
            return SetRecipe(new GUIDUnion(GUID), CallerID, craftingRules);
        }


        /// <summary>
        /// Undoes recipe modification for the provided GUID
        /// </summary>
        /// <param name="GUID"></param>
        public void ResetRecipe(GUIDUnion GUID, string CallerID)
        {
            if (ModifiedRecipes.TryGetValue(GUID, out var value))
            {
                if(value.Item1 != CallerID)
                {
                    //BepinPlugin.Log.LogError("CallerID must match Assignment CallerID. Maybe another mod changed the same recipe?");
                    throw new ArgumentException("CallerID must match Assignment CallerID. Maybe another mod changed the same recipe?", "CallerID");
                }
                ResourceAssetContainer<CraftingDataContainer, UnityEngine.Object, CraftableItemDef>.Instance.GetAssetDefById(GUID).crafting = value.Item2;
                ModifiedRecipes.Remove(GUID);
            }
        }

        public void ResetRecipe(string GUID, string CallerID)
        {
            ResetRecipe(new GUIDUnion(GUID), CallerID);
        }

        public void ResetRecipe(int[] GUID, string CallerID)
        {
            ResetRecipe(new GUIDUnion(GUID), CallerID);
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


        /// <summary>
        /// Returns whether the given GUID UnlockOptions was modified.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>UnlockOptions modified</returns>
        public bool RecipeModified(GUIDUnion GUID)
        {
            return ModifiedRecipes.ContainsKey(GUID);
        }

        public bool RecipeModified(string GUID)
        {
            return RecipeModified(new GUIDUnion(GUID));
        }

        public bool RecipeModified(int[] GUID)
        {
            return RecipeModified(new GUIDUnion(GUID));
        }
    }
}
