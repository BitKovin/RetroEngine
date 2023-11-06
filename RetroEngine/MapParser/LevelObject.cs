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
        public static object CreateByTechnicalName(string technicalName)
        {
            Type objectType = GetObjectTypeByTechnicalName(technicalName);

            if (objectType != null)
            {
                return Activator.CreateInstance(objectType);
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
    }
}
