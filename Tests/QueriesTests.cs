using NUnit.Framework;

namespace Ksi.Tests
{
    [KsiComponent]
    internal struct Comp { public int Value; }

    [KsiEntity]
    internal struct Entity { public Comp Component; }

    [KsiArchetype]
    [ExplicitCopy, DynSized, Dealloc]
    internal struct Archetype { public RefList<Comp> Component; }

    [KsiDomain]
    [ExplicitCopy, DynSized, Dealloc]
    internal partial struct Domain
    {
        public RefList<Entity> AoS;
        public Archetype SoA;
    }

    internal static partial class IncrementSystem
    {
        [KsiQuery]
        private static void Tick(in Domain.KsiHandle _, ref Comp c, [KsiQueryParam] ref int inc)
        {
            c.Value += inc;
            inc++;
        }
    }

    [TestFixture]
    public class QueriesTests
    {
        [Test]
        public void Smoke()
        {
            const int n = 10;
            var domain = new Domain();
            try
            {
                domain.AoS.AppendDefault(n);
                for (var i = 0; i < n; i++)
                    domain.SoA.Add();

                var inc = 0;
                IncrementSystem.Tick(ref domain, ref inc);

                Assert.That(inc == n * 2);

                for (var i = 0; i < n; i++)
                {
                    Assert.That(domain.AoS.RefAt(i).Component.Value == i);
                    Assert.That(domain.SoA.Component.RefAt(i).Value == i + n);
                }
            }
            finally
            {

                domain.Dealloc();
            }
        }
    }
}