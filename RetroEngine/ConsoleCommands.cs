using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Command { get; }

        public ConsoleCommandAttribute(string command)
        {
            Command = command;
        }
    }

    class ConsoleCommands
    {
        // Add more commands as needed

        public static void ProcessCommand(string input)
        {
            string[] parts = input.Split(' ');

            if (parts.Length == 0)
            {
                Console.WriteLine("Invalid command");
                return;
            }

            string command = parts[0];
            string[] arguments = parts.Skip(1).ToArray();

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods();

                foreach (var method in methods)
                {
                    var attribute = (ConsoleCommandAttribute)Attribute.GetCustomAttribute(method, typeof(ConsoleCommandAttribute));

                    if (attribute != null && attribute.Command.Equals(command, StringComparison.OrdinalIgnoreCase))
                    {
                        // Ensure the number of arguments match the method's parameters
                        var parameters = method.GetParameters();
                        if (arguments.Length == parameters.Length)
                        {
                            // Convert arguments to the appropriate types
                            object[] convertedArgs = arguments.Select((arg, index) => Convert.ChangeType(arg, parameters[index].ParameterType)).ToArray();

                            // Invoke the method
                            var instance = Activator.CreateInstance(type);
                            method.Invoke(instance, convertedArgs);
                            return;
                        }
                        else
                        {
                            Logger.Log("Invalid number of arguments");
                            return;
                        }
                    }
                }
            }

            Logger.Log("Command not found");
        }
    }
}
