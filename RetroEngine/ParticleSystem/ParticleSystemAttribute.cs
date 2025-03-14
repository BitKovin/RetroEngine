﻿using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ParticleSysAttribute : Attribute
    {
        public string TechnicalName { get; }

        public ParticleSysAttribute(string technicalName)
        {
            TechnicalName = technicalName;
        }
    }

    public static class ParticleSystemFactory
    {
        private static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        public static ParticleSystemEnt CreateByTechnicalName(string technicalName)
        {
            if (TypeCache.TryGetValue(technicalName, out var objectType))
            {

                if (objectType == null)
                    return null;

                return Activator.CreateInstance(objectType) as ParticleSystemEnt;
            }

            return null;

            if(Level.ChangingLevel == false)
            Logger.Log($"System '{technicalName}' not found. Searching...");
            objectType = GetObjectTypeByTechnicalName(technicalName);

            if (objectType != null)
            {
                TypeCache[technicalName] = objectType; // Cache the result
                return Activator.CreateInstance(objectType) as ParticleSystemEnt;
            }
            else
            {
                TypeCache.TryAdd(technicalName, null);
                return null;
            }
        }

        private static Type GetObjectTypeByTechnicalName(string technicalName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {

                try
                {

                    foreach (Type type in assembly.GetTypes())
                    {
                        var attribute = type.GetCustomAttribute<ParticleSysAttribute>();
                        if (attribute != null && attribute.TechnicalName == technicalName)
                        {
                            return type;
                        }
                    }
                }
                catch (Exception) { }
            }

            return null; // Class with the specified TechnicalName not found
        }

        public static void InitializeTypeCache()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {

                if (assembly.FullName.Contains("RetroEngine") == false) continue;

                foreach (Type type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<ParticleSysAttribute>();
                    if (attribute != null)
                    {
                        TypeCache[attribute.TechnicalName] = type;
                    }
                }
            }
        }

    }

}
