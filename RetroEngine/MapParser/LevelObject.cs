using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LevelObjectAttribute : Attribute
    {
        public string TechnicalName { get; }

        public LevelObjectAttribute(string technicalName)
        {
            TechnicalName = technicalName;
        }
    }

    public class LevelObjectFactory
    {

        private static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        public static Entity CreateByTechnicalName(string technicalName)
        {
            if (TypeCache.TryGetValue(technicalName, out var objectType))
            {
                return Activator.CreateInstance(objectType) as Entity;
            }

            objectType = GetObjectTypeByTechnicalName(technicalName);

            if (objectType != null)
            {
                TypeCache[technicalName] = objectType; // Cache the result
                return Activator.CreateInstance(objectType) as Entity;
            }
            else
            {
                return null;
            }
        }

        private static Type GetObjectTypeByTechnicalName(string technicalName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<LevelObjectAttribute>();
                    if (attribute != null && attribute.TechnicalName == technicalName)
                    {
                        return type;
                    }
                }
            }

            return null; // Class with the specified TechnicalName not found
        }

        public static void InitializeTypeCache()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<LevelObjectAttribute>();
                    if (attribute != null)
                    {
                        TypeCache[attribute.TechnicalName] = type;
                    }
                }
            }
        }
    }
}
