using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitySkills
{
    /// <summary>
    /// Centralized skill discovery with assembly trust boundary.
    /// Only methods from the UnitySkills editor assembly are exposed as skills.
    /// </summary>
    internal static class UnitySkillDiscovery
    {
        private static readonly Assembly TrustedAssembly = typeof(UnitySkillAttribute).Assembly;

        internal sealed class DiscoveredSkill
        {
            public Type Type;
            public MethodInfo Method;
            public UnitySkillAttribute Attribute;
        }

        public static IEnumerable<DiscoveredSkill> GetSkills()
        {
            foreach (var type in GetTrustedTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    UnitySkillAttribute attr;
                    try { attr = method.GetCustomAttribute<UnitySkillAttribute>(); }
                    catch { continue; }

                    if (attr != null)
                    {
                        yield return new DiscoveredSkill
                        {
                            Type = type,
                            Method = method,
                            Attribute = attr
                        };
                    }
                }
            }
        }

        private static IEnumerable<Type> GetTrustedTypes()
        {
            try
            {
                return TrustedAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
