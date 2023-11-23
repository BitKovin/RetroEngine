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
    public class ParticleSystemAttribute : Attribute
    {
        public string TechnicalName { get; }

        public ParticleSystemAttribute(string technicalName)
        {
            TechnicalName = technicalName;
        }
    }

    public class ParticleSystemFactory
    {
        public static ParticleSystem CreateByTechnicalName(string technicalName)
        {
            Type objectType = GetObjectTypeByTechnicalName(technicalName);

            if (objectType != null)
            {
                return Activator.CreateInstance(objectType) as ParticleSystem;
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
                    var attribute = type.GetCustomAttribute<ParticleSystemAttribute>();
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
