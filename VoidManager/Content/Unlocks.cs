using CG.Client.UserData;
using HarmonyLib;
using ResourceAssets;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VoidManager.Content
{
    public class Unlocks
    {
        public static Unlocks Instance { get; internal set; }

        static FieldInfo UnlockOptionsFI = AccessTools.Field(typeof(UnlockItemDef), "unlockOptions");
        private Dictionary<GUIDUnion, UnlockOptions> ModifiedUnlockOptions = new();

        /// <summary>
        /// Sets UnlockOptions for GUID if previously-existing UnlockOptions exists.
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="UnlockOptions"></param>
        /// <exception cref="ArgumentException">An asset with the provided GUID does not exist.</exception>
        public void SetUnlockOptions(GUIDUnion GUID, UnlockOptions unlockOptions)
        {
            if (ModifiedUnlockOptions.ContainsKey(GUID))
            {
                BepinPlugin.Log.LogError($"Attempted to modify recipe for object at GUID: {GUID}, however it has already been modified.");
            }
            else if (!ResourceAssetContainer<UnlockContainer, UnityEngine.Object, UnlockItemDef>.Instance.TryGetByGuid(GUID, out UnlockItemDef asset))
            {
                throw new ArgumentException("An asset with the provided GUID does not exist.");
            }
            else
            {
                ModifiedUnlockOptions.Add(GUID, (UnlockOptions)UnlockOptionsFI.GetValue(asset));;
                UnlockOptionsFI.SetValue(asset, unlockOptions);
            }
        }

        public void SetUnlockOptions(string GUID, UnlockOptions unlockOptions)
        {
            SetUnlockOptions(new GUIDUnion(GUID), unlockOptions);
        }

        public void SetUnlockOptions(int[] GUID, UnlockOptions unlockOptions)
        {
            SetUnlockOptions(new GUIDUnion(GUID), unlockOptions);
        }


        /// <summary>
        /// Undoes recipe modification for the provided GUID
        /// </summary>
        /// <param name="GUID"></param>
        public void ResetUnlockOptions(GUIDUnion GUID)
        {
            if (ModifiedUnlockOptions.TryGetValue(GUID, out UnlockOptions value))
            {
                UnlockOptionsFI.SetValue(ResourceAssetContainer<UnlockContainer, UnityEngine.Object, UnlockItemDef>.Instance.GetAssetDefById(GUID), value);
                ModifiedUnlockOptions.Remove(GUID);
            }
        }

        public void ResetUnlockOptions(string GUID)
        {
            ResetUnlockOptions(new GUIDUnion(GUID));
        }

        public void ResetUnlockOptions(int[] GUID)
        {
            ResetUnlockOptions(new GUIDUnion(GUID));
        }


        /// <summary>
        /// Returns the recipe for the given GUID.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>UnlockOptions for recipe of GUID</returns>
        public UnlockOptions GetUnlockOptions(GUIDUnion GUID)
        {
            return (UnlockOptions)UnlockOptionsFI.GetValue(ResourceAssetContainer<UnlockContainer, UnityEngine.Object, UnlockItemDef>.Instance.GetAssetDefById(GUID));
        }

        public UnlockOptions GetUnlockOptions(string GUID)
        {
            return GetUnlockOptions(new GUIDUnion(GUID));
        }

        public UnlockOptions GetUnlockOptions(int[] GUID)
        {
            return GetUnlockOptions(new GUIDUnion(GUID));
        }
    }
}
