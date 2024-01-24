using BepInEx;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoidManager.Mod
{
    /// <summary>
    /// Restrict the BepIn Plugins multiplayer usage based on who needs the mod
    /// (MultiplayerType.All, MultiplayerType.Client)
    /// </summary>
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ManagerRestrict : Attribute
    {
        public MultiplayerType MPType { get; protected set; }
        public ManagerRestrict(MultiplayerType MultiPlayerType = MultiplayerType.Client)
        {
            MPType = MultiPlayerType;
        }
        internal static ManagerRestrict FromCecilType(TypeDefinition td)
        {
            CustomAttribute customAttribute = MetadataHelper.GetCustomAttributes<ManagerRestrict>(td, inherit: false).FirstOrDefault();
            if (customAttribute == null)
            {
                return null;
            }
            return new ManagerRestrict((MultiplayerType)customAttribute.ConstructorArguments[0].Value);
        }
    }
}
