using BepInEx;
using Mono.Cecil;
using System.Linq;

namespace VoidManager.Mod
{
    public class ManagerPlugin : BepInPlugin
    {
        public MultiplayerType MPType;
        public ManagerPlugin(string Author, string ModName, string Version, MultiplayerType MultiPlayerType = MultiplayerType.Client) : base($"{Author}.{ModName}", ModName, Version)
        {
            MPType = MultiPlayerType;
        }
        internal static ManagerPlugin FromCecilType(TypeDefinition td)
        {
            CustomAttribute customAttribute = MetadataHelper.GetCustomAttributes<ManagerPlugin>(td, inherit: false).FirstOrDefault();
            if (customAttribute == null)
            {
                return null;
            }
            return new ManagerPlugin((string)customAttribute.ConstructorArguments[0].Value, (string)customAttribute.ConstructorArguments[1].Value, (string)customAttribute.ConstructorArguments[2].Value, (customAttribute.ConstructorArguments.Count > 3 ? (MultiplayerType)customAttribute.ConstructorArguments[3].Value : MultiplayerType.Client ) );
        }
    }
}
