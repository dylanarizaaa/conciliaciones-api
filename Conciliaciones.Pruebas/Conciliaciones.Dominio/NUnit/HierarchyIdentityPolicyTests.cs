using Conciliaciones.Dominio.Services;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class HierarchyIdentityPolicyTests
    {
        [Test]
        public void IsExistingIdentifier_ValidGuid_ReturnsTrue()
        {
            string guid = Guid.NewGuid().ToString();

            HierarchyIdentityPolicy.Instance.IsExistingIdentifier(guid).Should().BeTrue();
            HierarchyIdentityPolicy.UsesExistingIdentifier(guid).Should().BeTrue();
        }

        [Test]
        public void IsExistingIdentifier_Name_ReturnsFalse()
        {
            HierarchyIdentityPolicy.Instance.IsExistingIdentifier("Proyecto A").Should().BeFalse();
        }

        [Test]
        public void IsExistingIdentifier_NullOrEmpty_ReturnsFalse()
        {
            HierarchyIdentityPolicy.Instance.IsExistingIdentifier(null).Should().BeFalse();
            HierarchyIdentityPolicy.Instance.IsExistingIdentifier("").Should().BeFalse();
            HierarchyIdentityPolicy.Instance.IsExistingIdentifier("   ").Should().BeFalse();
        }
    }
}
