using System.Linq;
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
            SkillRouter.Refresh();

            var manifest = JObject.Parse(SkillRouter.GetManifest());
            var skillNames = manifest["skills"]
                .Select(skill => skill["name"]?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();

            CollectionAssert.DoesNotContain(skillNames, "malicious_external_skill_probe");
            Assert.IsNotEmpty(skillNames);
        }
    }
}
