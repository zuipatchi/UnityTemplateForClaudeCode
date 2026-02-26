using System.Threading;
using Cysharp.Threading.Tasks;

namespace Sample
{
    /// <summary>
    /// UniTask を使って HP を非同期で回復するユーティリティ。
    /// </summary>
    public static class AsyncHpRecovery
    {
        /// <summary>
        /// HP が最大になるまで、一定間隔で回復し続けます。
        /// CancellationToken でキャンセル可能です。
        /// </summary>
        /// <param name="hp">回復対象の Hp インスタンス</param>
        /// <param name="amountPerTick">1回あたりの回復量</param>
        /// <param name="intervalMs">回復間隔（ミリ秒）</param>
        /// <param name="ct">キャンセルトークン</param>
        public static async UniTask RecoverAsync(Hp hp, int amountPerTick, int intervalMs, CancellationToken ct)
        {
            while (hp.Current < hp.Max)
            {
                await UniTask.Delay(intervalMs, cancellationToken: ct);
                hp.Heal(amountPerTick);
            }
        }
    }
}
