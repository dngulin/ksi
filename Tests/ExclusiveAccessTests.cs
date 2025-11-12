using System;
using NUnit.Framework;

namespace Ksi.Tests
{
    [TestFixture]
    public class ExclusiveAccessTests
    {
        [Test]
        public void IsLocked()
        {
            var exAcc = new ExclusiveAccess<RefList<int>>();
            Assert.That(exAcc.IsLocked, Is.False);

            using (var accessScope = exAcc.Mutable)
            {
                Assert.That(exAcc.IsLocked, Is.True);
                _ = ref accessScope.Value;
            }

            Assert.That(exAcc.IsLocked, Is.False);

            using (var accessScope = exAcc.ReadOnly)
            {
                Assert.That(exAcc.IsLocked, Is.True);
                _ = ref accessScope.Value;
            }

            Assert.That(exAcc.IsLocked, Is.False);
        }

        [Test]
        public void ProvidesExclusiveAccess()
        {
            var exAcc = new ExclusiveAccess<RefList<int>>();
            using (var accessScope = exAcc.Mutable)
            {
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.Mutable);
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.ReadOnly);
                _ = ref accessScope.Value;
            }

            using (var accessScope = exAcc.ReadOnly)
            {
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.Mutable);
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.ReadOnly);
                _ = ref accessScope.Value;
            }
        }

        [Test]
        public void ProvidesAccess()
        {
            var exAcc = new ExclusiveAccess<RefList<int>>();
            using (var accessScope = exAcc.Mutable)
            {
                accessScope.Value.Add(42);
            }

            using (var accessScope = exAcc.ReadOnly)
            {
                Assert.That(accessScope.Value.RefReadonlyAt(0), Is.EqualTo(42));
            }

            using (var accessScope = exAcc.Mutable)
            {
                accessScope.Value.Dealloc();
            }
        }
    }
}