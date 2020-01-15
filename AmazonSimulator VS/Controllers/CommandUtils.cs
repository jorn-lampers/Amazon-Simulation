using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Controllers
{
    public static class CommandUtils
    {
        public struct CommandArgumentInfo
        {
            public string Name;
            public string Type;

            public dynamic DefaultValue;
            public bool IsRequired;
        }

        public struct CommandInfo
        {
            public string Name;
            public string Description;

            public Dictionary<string, CommandArgumentInfo> Arguments;
        }

        public static List<Type> GetAvailableViewCommandTypes()
        {
            Type baseType = typeof(ServerCommand);
            Assembly asm = baseType.Assembly;

            return asm.GetTypes().Where(type => type != baseType && type.IsAssignableFrom(baseType)).ToList();
        }

        public static FieldInfo[] GetCommandArguments<T>() where T: ServerCommand
        {
            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        public static string GetFriendlyFieldNameType(FieldInfo fieldInfo)
        {
            string name = fieldInfo.FieldType.Name;
            if (fieldInfo.FieldType.GenericTypeArguments.Length == 0) return name;

            name += "<" + fieldInfo.FieldType.GenericTypeArguments[0].Name;
            foreach(Type type in fieldInfo.FieldType.GenericTypeArguments.Skip(1))
            {
                name += ", " + type.Name;
            }
            name += ">";
            return name;
        }

        public static CommandInfo GenerateCommandInfo<T>(T command) where T: ServerCommand
        {
            T defaultInstance = Activator.CreateInstance<T>();
            CommandInfo commandInfo = new CommandInfo();

            commandInfo.Name = command.type;
            commandInfo.Description = "";
            commandInfo.Arguments = new Dictionary<string, CommandArgumentInfo>();

            FieldInfo[] args = GetCommandArguments<T>();
            foreach (FieldInfo fieldInfo in args)
            {
                CommandArgumentInfo argInfo = new CommandArgumentInfo();

                argInfo.Name = fieldInfo.Name;
                argInfo.Type = GetFriendlyFieldNameType(fieldInfo); 

                argInfo.DefaultValue = fieldInfo.GetValue(defaultInstance);
                argInfo.IsRequired = argInfo.DefaultValue == null;

                commandInfo.Arguments.Add(argInfo.Name, argInfo);
            }

            return commandInfo;
        }

    }
}
