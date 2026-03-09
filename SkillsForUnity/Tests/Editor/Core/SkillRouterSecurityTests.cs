using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace UnitySkills.Tests.Core
{
    internal static class ExternalAssemblySkillProbe
    {
        [UnitySkill("malicious_external_skill_probe", "Should never be auto-exposed from test assembly")]
        public static object MaliciousExternalSkillProbe()
        {
            return new { success = true };
        }
    }

    [TestFixture]
    public class SkillRouterSecurityTests
    {
        [Test]
        public void GetManifest_DoesNotExposeSkillsFromOtherAssemblies()
        {
            var probeMethod = typeof(ExternalAssemblySkillProbe).GetMethod(
                nameof(ExternalAssemblySkillProbe.MaliciousExternalSkillProbe),
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(probeMethod);
            Assert.IsNotNull(probeMethod.GetCustomAttribute<UnitySkillAttribute>());

            var trustedAssemblySkillNames = typeof(SkillRouter).Assembly
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Select(method => new
                {
                    Method = method,
                    Attribute = method.GetCustomAttribute<UnitySkillAttribute>()
                })
                .Where(x => x.Attribute != null)
                .Select(x => x.Attribute.Name ?? ToSnakeCase(x.Method.Name))
                .ToArray();
            Assert.IsNotEmpty(trustedAssemblySkillNames);

            SkillRouter.Refresh();

            var manifest = JObject.Parse(SkillRouter.GetManifest());
            var skillNames = manifest["skills"]
                .Select(skill => skill["name"]?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();

            Assert.IsTrue(trustedAssemblySkillNames.Any(skillNames.Contains));
            CollectionAssert.DoesNotContain(skillNames, "malicious_external_skill_probe");
        }

        private static string ToSnakeCase(string value)
        {
            return Regex.Replace(value, "([a-z])([A-Z])", "$1_$2").ToLowerInvariant();
        }
    }
}
