using NUnit.Framework;
using Sample;

namespace Tests.EditMode
{
    public class HpTests
    {
        [Test]
        public void 初期HPはMaxと等しい()
        {
            var hp = new Hp(100);
            Assert.AreEqual(100, hp.Current);
            Assert.AreEqual(100, hp.Max);
        }

        [Test]
        public void ダメージでCurrentが減る()
        {
            var hp = new Hp(100);
            hp.TakeDamage(30);
            Assert.AreEqual(70, hp.Current);
        }

        [Test]
        public void HPは0未満にならない()
        {
            var hp = new Hp(100);
            hp.TakeDamage(999);
            Assert.AreEqual(0, hp.Current);
        }

        [Test]
        public void HP0でIsDeadがtrue()
        {
            var hp = new Hp(100);
            hp.TakeDamage(100);
            Assert.IsTrue(hp.IsDead);
        }

        [Test]
        public void HP残存時はIsDeadがfalse()
        {
            var hp = new Hp(100);
            hp.TakeDamage(99);
            Assert.IsFalse(hp.IsDead);
        }

        [Test]
        public void 回復でCurrentが増える()
        {
            var hp = new Hp(100);
            hp.TakeDamage(50);
            hp.Heal(20);
            Assert.AreEqual(70, hp.Current);
        }

        [Test]
        public void HPはMaxを超えない()
        {
            var hp = new Hp(100);
            hp.Heal(999);
            Assert.AreEqual(100, hp.Current);
        }

        [Test]
        public void MaxHPが0以下のとき例外をスロー()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Hp(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Hp(-1));
        }

        [Test]
        public void 負のダメージ量は例外をスロー()
        {
            var hp = new Hp(100);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => hp.TakeDamage(-1));
        }

        [Test]
        public void 負の回復量は例外をスロー()
        {
            var hp = new Hp(100);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => hp.Heal(-1));
        }
    }
}
