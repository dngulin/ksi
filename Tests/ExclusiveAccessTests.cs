using System;
using NUnit.Framework;

namespace Ksi.Tests
{
    [TestFixture]
    public class ExclusiveAccessTests
    {
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

        [Test]
        public void IsLocked()
        {
            var exAcc = new ExclusiveAccess<RefList<int>>();
            Assert.That(exAcc.IsLocked, Is.False);

            using (exAcc.Mutable)
            {
                Assert.That(exAcc.IsLocked, Is.True);
            }

            Assert.That(exAcc.IsLocked, Is.False);

            using (exAcc.ReadOnly)
            {
                Assert.That(exAcc.IsLocked, Is.True);
            }

            Assert.That(exAcc.IsLocked, Is.False);
        }

        [Test]
        public void ProvidesExclusiveAccess()
        {
            var exAcc = new ExclusiveAccess<RefList<int>>();
            using (exAcc.Mutable)
            {
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.Mutable);
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.ReadOnly);
            }

            using (exAcc.ReadOnly)
            {
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.Mutable);
                Assert.Throws<InvalidOperationException>(() => _ = exAcc.ReadOnly);
            }
        }
    }
}