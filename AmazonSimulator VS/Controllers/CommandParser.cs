using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Controllers
{
    public static class CommandParser
    {
        private static dynamic JTokenToType(JToken value, Type typeName)
        {
            dynamic val = null;
            
            switch (typeName.Name)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                    val = value.ToObject<int>();
                    break;

                case "string":
                    val = value.ToObject<string>();
                    break;

                case "Guid":
                    val = Guid.Parse(value.ToObject<string>());
                    break;

                case string s when (s.StartsWith("List`")):
                    val = Activator.CreateInstance(typeName);
                    JEnumerable<JToken> children = value.Children();
                    foreach (JToken token in children) val.Add(JTokenToType(token, typeName.GenericTypeArguments[0]));
                    break;

                case string s when (s.StartsWith("Nullable`")):
                    val = Activator.CreateInstance(typeName, value.ToObject(typeName));
                    break;

                case "Vector3":
                    float x = value.SelectToken("x").ToObject<float>();
                    float y = value.SelectToken("y").ToObject<float>();
                    float z = value.SelectToken("z").ToObject<float>();

                    val = new Vector3(x, y, z);
                    break;

                default:
                    // This will probably rise an exception
                    val = value.ToObject<object>();
                    break;
            }

            return val;
        }

        public static ServerCommand Parse(string json)
        {
            Console.WriteLine("Received: " + json);
            Console.WriteLine("Length: " + json.Length);

            // Parse JSON in args to access its values
            JObject obj = JObject.Parse(json);

            // Get typename of required command from json
            string commandName = "Controllers." + ((string)obj.SelectToken("type"));
            // Get command's type from typename
            Type commandType = typeof(ServerCommand).Assembly.GetType(commandName);

            // Get command's parameters
            JObject parameters = obj.SelectToken("parameters") as JObject;

            // Create an empty instance of TypeName specified in message
            ServerCommand command = Activator.CreateInstance(commandType) as ServerCommand;

            // Get fields of ViewCommand's derived class
            var fields = commandType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fields)
                try
                {
                    dynamic val = CommandParser.JTokenToType(parameters.SelectToken(field.Name), field.FieldType);
                    field.SetValue(command, val);
                }
                catch (NullReferenceException)
                {
                    if (field.GetValue(command) == null)
                        throw new ArgumentException(String.Format("Command of type '{0}' is missing required field '{1}' in supplied parameters.", commandName, field.Name));
                    else continue;
                }

            return command;
        }
    }

}