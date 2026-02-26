using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Sample;

namespace Tests
{
    public class AsyncHpRecoveryTests
    {
        [Test]
        public void RecoverAsync_WhenHpIsFull_CompletesImmediately()
        {
            var hp = new Hp(100);
            // HP はすでに最大値なのでループに入らず同期的に完了する
            var task = AsyncHpRecovery.RecoverAsync(hp, 10, 1000, CancellationToken.None);

            Assert.AreEqual(UniTaskStatus.Succeeded, task.Status);
            Assert.AreEqual(100, hp.Current);
        }

        [Test]
        public void RecoverAsync_WhenAlreadyCancelled_ReturnsCancelledTask()
        {
            var hp = new Hp(100);
            hp.TakeDamage(50); // Current = 50 → ループに入る

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // 事前にキャンセル

            // 最初の UniTask.Delay で即座にキャンセルされる
            var task = AsyncHpRecovery.RecoverAsync(hp, 10, 1000, cts.Token);

            Assert.AreEqual(UniTaskStatus.Canceled, task.Status);
            Assert.AreEqual(50, hp.Current, "キャンセル後に回復されていないこと");
        }
    }
}
