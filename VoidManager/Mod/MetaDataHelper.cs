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

        /*public static ManagerPlugin GetMetadata(Type pluginType)
        {
            object[] customAttributes = pluginType.GetCustomAttributes(typeof(ManagerPlugin), inherit: false);
            if (customAttributes.Length == 0)
            {
                return null;
            }

            return (ManagerPlugin)customAttributes[0];
        }*/

        /*public static ManagerPlugin GetMetadata(object plugin)
        {
            return GetMetadata(plugin.GetType());
        }*/

        public static T[] GetAttributes<T>(Type pluginType) where T : Attribute
        {
            return (T[])pluginType.GetCustomAttributes(typeof(T), inherit: true);
        }

        /*public static IEnumerable<T> GetAttributes<T>(object plugin) where T : Attribute
        {
            return GetAttributes<T>(plugin.GetType());
        }

        public static IEnumerable<BepInDependency> GetDependencies(Type plugin)
        {
            return plugin.GetCustomAttributes(typeof(BepInDependency), inherit: true).Cast<BepInDependency>();
        }*/
    }
}
