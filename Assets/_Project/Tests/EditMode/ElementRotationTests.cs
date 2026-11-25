using MangaLegends.Core.Simulation;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class ElementRotationTests
    {
        [Test]
        public void FirstSpawnsNeverRepeat_ThenTheMapElementStays([Range(0, 40)] int seed)
        {
            int first = ElementRotation.Pick((ulong)seed, 0, 4, 2);
            int second = ElementRotation.Pick((ulong)seed, 1, 4, 2);
            int third = ElementRotation.Pick((ulong)seed, 2, 4, 2);
            Assert.AreNotEqual(first, second);
            Assert.AreNotEqual(first, third);
            Assert.AreNotEqual(second, third);
            for (int spawn = 3; spawn < 10; spawn++) Assert.AreEqual(third, ElementRotation.Pick((ulong)seed, spawn, 4, 2));
        }

        [Test]
        public void SameSeed_GivesSameDraw()
        {
            for (int spawn = 0; spawn < 5; spawn++)
                Assert.AreEqual(ElementRotation.Pick(1234UL, spawn, 4, 2), ElementRotation.Pick(1234UL, spawn, 4, 2));
        }

        [Test]
        public void EveryElementCanOpenTheGame()
        {
            var seen = new bool[4];
            for (ulong seed = 0; seed < 200; seed++) seen[ElementRotation.Pick(seed, 0, 4, 2)] = true;
            CollectionAssert.AreEqual(new[] { true, true, true, true }, seen);
        }
    }
}
