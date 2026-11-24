using System;
using MangaLegends.Core.Content;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class ContentIndexTests
    {
        private sealed class Entry : IContentEntry
        {
            public Entry(string id) => Id = id;
            public string Id { get; }
        }

        [TestCase("champion.ainz", true)]
        [TestCase("ability.ainz.mantle_of_chaos", true)]
        [TestCase("Champion.Ainz", false)]
        [TestCase("ainz", false)]
        [TestCase("item.", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void ContentId_FollowsDomainNamePattern(string id, bool expected)
        {
            Assert.That(ContentId.IsValid(id), Is.EqualTo(expected));
        }

        [Test]
        public void Index_IsIndependentOfInputOrder()
        {
            ContentIndex<Entry> a = new ContentIndex<Entry>(new[] { new Entry("item.b"), new Entry("item.a"), new Entry("item.c") });
            ContentIndex<Entry> b = new ContentIndex<Entry>(new[] { new Entry("item.c"), new Entry("item.b"), new Entry("item.a") });

            Assert.That(a.IndexOf("item.b"), Is.EqualTo(b.IndexOf("item.b")));
            Assert.That(a[0].Id, Is.EqualTo("item.a"));
        }

        [Test]
        public void Index_RejectsDuplicatesAndInvalidIds()
        {
            Assert.Throws<ArgumentException>(() => new ContentIndex<Entry>(new[] { new Entry("item.a"), new Entry("item.a") }));
            Assert.Throws<ArgumentException>(() => new ContentIndex<Entry>(new[] { new Entry("Bad Id") }));
        }

        [Test]
        public void TryGet_ReturnsFalseForUnknownId()
        {
            ContentIndex<Entry> index = new ContentIndex<Entry>(new[] { new Entry("item.a") });

            Assert.That(index.TryGet("item.z", out Entry entry), Is.False);
            Assert.That(entry, Is.Null);
            Assert.That(index.IndexOf("item.z"), Is.EqualTo(-1));
        }

        [Test]
        public void Report_FlagsInvalidAndDuplicateIds_AndRankMismatch()
        {
            ValidationReport report = new ValidationReport();

            report.CheckIds(new IContentEntry[] { new Entry("item.a"), new Entry("item.a"), new Entry("oops") });
            report.CheckRankArray("ability.x", "Cooldown", length: 3, expected: 5);
            report.CheckRankArray("ability.x", "Cost", length: 1, expected: 5);

            Assert.That(report.HasErrors, Is.True);
            Assert.That(report.Issues.Count, Is.EqualTo(3));
        }
    }
}
