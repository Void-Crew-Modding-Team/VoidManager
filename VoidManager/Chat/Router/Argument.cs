using System.Collections.Generic;
using System.IO;

namespace VoidManager.Chat.Router
{
    /// <summary>
    /// A tree structure for containing chat command arguments
    /// </summary>
    public class Argument
    {
        /// <summary>
        /// A list of arguments or aliases on the same level<br/>
        /// E.g. /command &lt;names[0] | names[1] | names[2]&gt;
        /// </summary>
        public readonly string[] names;

        /// <summary>
        /// A list of sub arguments for this argument<br/>
        /// E.g. /command &lt;names[0]&gt; &lt;arguments[0].names[0] | arguments[0].names[1] | arguments[1].names[0]&gt;
        /// </summary>
        public readonly List<Argument> arguments;

        /// <summary>
        /// The last argument in this chain
        /// </summary>
        /// <param name="names"></param>
        public Argument(params string[] names)
        {
            this.names = names;
            arguments = new();
        }

        /// <summary>
        /// An argument with a list of arguments to follow it
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argument"></param>
        public Argument(string name, Argument argument)
        {
            this.names = new string[] { name };
            this.arguments = new() { argument };
        }

        /// <summary>
        /// An argument with a list of arguments to follow it
        /// </summary>
        /// <param name="names"></param>
        /// <param name="argument"></param>
        public Argument(string[] names, Argument argument)
        {
            this.names = names;
            this.arguments = new() { argument };
        }

        /// <summary>
        /// An argument with a list of arguments to follow it
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        public Argument(string name, List<Argument> arguments)
        {
            this.names = new string[] { name };
            this.arguments = arguments;
        }

        /// <summary>
        /// An argument with a list of arguments to follow it
        /// </summary>
        /// <param name="names"></param>
        /// <param name="arguments"></param>
        public Argument(string[] names, List<Argument> arguments)
        {
            this.names = names;
            this.arguments = arguments;
        }

        internal static byte[] ToByteArray(List<Argument> arguments)
        {
            using MemoryStream m = new();
            using BinaryWriter writer = new(m);

            writer.Write(arguments.Count);
            for (int i = 0; i < arguments.Count; i++)
            {
                ToByteArrayRecursive(writer, arguments[i]);
            }

            return m.ToArray();
        }

        internal static List<Argument> FromByteArray(byte[] bytes)
        {
            using MemoryStream m = new(bytes);
            using BinaryReader reader = new(m);
            List<Argument> arguments = new();

            int listCount = reader.ReadInt32();
            for (int i = 0; i < listCount; i++)
            {
                arguments.Add(FromByteArrayRecursive(reader));
            }

            return arguments;
        }

        private static void ToByteArrayRecursive(BinaryWriter writer, Argument argument)
        {
            writer.Write(argument.names.Length);
            foreach (string argumentName in argument.names)
            {
                writer.Write(argumentName);
            }

            writer.Write(argument.arguments.Count);
            if (argument.arguments.Count > 0)
            {
                foreach (Argument nextArgument in argument.arguments)
                {
                    ToByteArrayRecursive(writer, nextArgument);
                }
            }
        }

        private static Argument FromByteArrayRecursive(BinaryReader reader)
        {
            int nameLength = reader.ReadInt32();
            string[] names = new string[nameLength];
            for (int i = 0; i < nameLength; i++)
            {
                names[i] = reader.ReadString();
            }

            int argumentsCount = reader.ReadInt32();
            List<Argument> arguments = new();
            if (argumentsCount != 0)
            {
                for (int i = 0; i < argumentsCount; i++)
                {
                    arguments.Add(FromByteArrayRecursive(reader));
                }
            }

            return new Argument(names, arguments);
        }
    }
}
