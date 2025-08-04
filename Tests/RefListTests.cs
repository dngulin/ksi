using NUnit.Framework;

namespace Frog.Collections.Tests
{
    [TestFixture]
    public class RefListTests
    {
        [TestCase(7)]
        [TestCase(15)]
        [TestCase(32)]
        public void AddAndClear(int count)
        {
            var list = RefList.Empty<int>();
            for (var i = 0; i < count; i++)
            {
                list.Add(i);
                var value = list.Count();
                var expected = i + 1;
                Assert.That(value == expected, $" {nameof(AddAndClear)}.Count: {value} != {expected}");
            }

            for (var i = 0; i < count; i++)
            {
                var value = list.RefReadonlyAt(i);
                var expected = i;
                Assert.That(value == expected, $" {nameof(AddAndClear)}.Value: {value} != {expected}");
            }

            list.Clear();
            Assert.That(list.Count() == 0, $"{nameof(AddAndClear)}.Clear");
        }

        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        public void RefAddAndClear(int count)
        {
            var list = RefList.WithCapacity<int>(count);
            for (var i = 0; i < count; i++)
            {
                list.RefAdd() = i;
                var value = list.Count();
                var expected = i + 1;
                Assert.That(value == expected, $" {nameof(RefAddAndClear)}.Count: {value} != {expected}");
            }

            for (var i = 0; i < count; i++)
            {
                var value = list.RefReadonlyAt(i);
                var expected = i;
                Assert.That(value == expected, $" {nameof(RefAddAndClear)}.Value: {value} != {expected}");
            }

            list.Clear();
            Assert.That(list.Count() == 0, $"{nameof(RefAddAndClear)}.Clear");
        }

        [TestCase(9)]
        [TestCase(17)]
        [TestCase(21)]
        public void UpdateValue(int count)
        {
            var list = RefList.WithDefaultItems<int>(count);
            for (var i = 0; i < count; i++)
            {
                list.RefAt(i) = -i * 2;
            }

            for (var i = 0; i < count; i++)
            {
                var value = list.RefReadonlyAt(i);
                var expected = -i * 2;
                Assert.That(value == expected, $" {nameof(UpdateValue)}.Value: {value} != {expected}");
            }

            list.Clear();
            Assert.That(list.Count() == 0, $"{nameof(UpdateValue)}.Clear");
        }

        [TestCase(11)]
        [TestCase(13)]
        [TestCase(29)]
        public void RemoveAtBegin(int count)
        {
            var list = RefList.Empty<int>();
            for (var i = 0; i < count; i++)
            {
                list.Add(i * 3);
            }

            for (var i = 0; i < count; i++)
            {
                {
                    var value = list.RefReadonlyAt(0);
                    var expected = i * 3;
                    Assert.That(value == expected, $" {nameof(RemoveAtBegin)}.Value: {value} != {expected}");
                }

                {
                    list.RemoveAt(0);
                    var value = list.Count();
                    var expected = count - i - 1;
                    Assert.That(value == expected, $" {nameof(RemoveAtBegin)}.Count: {value} != {expected}");
                }
            }

            Assert.That(list.Count() == 0, $"{nameof(RemoveAtBegin)}.Empty");
        }
    }
}