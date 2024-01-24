using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoidManager.Mod
{
    public static class MetadataHelper
    {
        internal static IEnumerable<CustomAttribute> GetCustomAttributes<T>(TypeDefinition td, bool inherit) where T : Attribute
        {
            List<CustomAttribute> list = new List<CustomAttribute>();
            Type type = typeof(T);
            TypeDefinition typeDefinition = td;
            do
            {
                list.AddRange(typeDefinition.CustomAttributes.Where((CustomAttribute ca) => ca.AttributeType.FullName == type.FullName));
                typeDefinition = typeDefinition.BaseType?.Resolve();
            }
            while (inherit && typeDefinition?.FullName != "System.Object");
            return list;
        }

        /// <summary>
        /// Retrieves the Custom Attribute Details for a given plugin.
        /// </summary>
        /// <typeparam name="T">Attribute metadata to look for</typeparam>
        /// <param name="pPlugin">Object to look for the metadata with</param>
        /// <param name="pAttributeDetails">Metadata for the objects detailed Attribute</param>
        /// <returns>True/False on if it was successful</returns>
        internal static bool TryGetMetaData<T>(object pPlugin, out T pAttributeDetails) where T : Attribute
        {
            object[] customAttributes = pPlugin.GetType().GetCustomAttributes(typeof(T), inherit: false);
            pAttributeDetails = null;
            if (customAttributes.Length == 0)
            {
                return false;
            }
            pAttributeDetails = (T)customAttributes[0];
            return true;
        }
    }
}
