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
        private Dictionary<GUIDUnion, Tuple<string, UnlockOptions>> ModifiedUnlockOptions = new();

        /// <summary>
        /// Sets UnlockOptions for GUID if previously-existing UnlockOptions exists.
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="UnlockOptions"></param>
        /// <exception cref="ArgumentException">An asset with the provided GUID does not exist.</exception>
        /// <returns>UnlockOptions succesfully modified</returns>
        public bool SetUnlockOptions(GUIDUnion GUID, string CallerID, UnlockOptions unlockOptions)
        {
            if (!ResourceAssetContainer<UnlockContainer, UnityEngine.Object, UnlockItemDef>.Instance.TryGetByGuid(GUID, out UnlockItemDef asset))
            {
                throw new ArgumentException("An asset with the provided GUID does not exist.");
            }
            else if (ModifiedUnlockOptions.TryGetValue(GUID, out var value))
            {
                if (value.Item1 != CallerID)
                {
                    BepinPlugin.Log.LogError($"Attempted to modify recipe for object at GUID: {GUID}, however it has already been modified by another mod.");
                    return false;
                }
                else //Mod that set GUID is overwriting value.
                {
                    UnlockOptionsFI.SetValue(asset, unlockOptions);
                    return true;
                }
            }
            else
            {
                ModifiedUnlockOptions.Add(GUID, new Tuple<string, UnlockOptions>(CallerID, (UnlockOptions)UnlockOptionsFI.GetValue(asset)));
                UnlockOptionsFI.SetValue(asset, unlockOptions);
                return true;
            }
        }

        public bool SetUnlockOptions(string GUID, string CallerID, UnlockOptions unlockOptions)
        {
            return SetUnlockOptions(new GUIDUnion(GUID), CallerID, unlockOptions);
        }

        public bool SetUnlockOptions(int[] GUID, string CallerID, UnlockOptions unlockOptions)
        {
            return SetUnlockOptions(new GUIDUnion(GUID), CallerID, unlockOptions);
        }


        /// <summary>
        /// Undoes UnlockOptions modification for the provided GUID
        /// </summary>
        /// <param name="GUID"></param>
        public void ResetUnlockOptions(GUIDUnion GUID, string CallerID)
        {
            if (ModifiedUnlockOptions.TryGetValue(GUID, out Tuple<string, UnlockOptions> value))
            {
                if (value.Item1 != CallerID)
                {
                    throw new ArgumentException("CallerID must match Assignment CallerID. Maybe another mod changed the same UnlockOptions?", "CallerID");
                }
                UnlockOptionsFI.SetValue(ResourceAssetContainer<UnlockContainer, UnityEngine.Object, UnlockItemDef>.Instance.GetAssetDefById(GUID), value.Item2);
                ModifiedUnlockOptions.Remove(GUID);
            }
        }

        public void ResetUnlockOptions(string GUID, string CallerID)
        {
            ResetUnlockOptions(new GUIDUnion(GUID), CallerID);
        }

        public void ResetUnlockOptions(int[] GUID, string CallerID)
        {
            ResetUnlockOptions(new GUIDUnion(GUID), CallerID);
        }


        /// <summary>
        /// Returns the current UnlockOptions for the given GUID.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>UnlockOptions for GUID</returns>
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

        /// <summary>
        /// Returns whether the given GUID UnlockOptions was modified.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>UnlockOptions modified</returns>
        public bool UnlockOptionsModified(GUIDUnion GUID)
        {
            return ModifiedUnlockOptions.ContainsKey(GUID);
        }

        public bool UnlockOptionsModified(string GUID)
        {
            return UnlockOptionsModified(new GUIDUnion(GUID));
        }

        public bool UnlockOptionsModified(int[] GUID)
        {
            return UnlockOptionsModified(new GUIDUnion(GUID));
        }
    }
}
