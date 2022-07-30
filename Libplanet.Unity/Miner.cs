using System.Collections;
using System.Threading.Tasks;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;
using UnityEngine;

namespace Libplanet.Unity
{
    /// <summary>
    /// <see cref="Miner"/> class provides basic mining coroutine.
    /// </summary>
    public class Miner
    {
        /// <summary>
        /// The <see cref="Swarm{T}"/> to use for mining.
        /// </summary>
        private Swarm<PolymorphicAction<ActionBase>> _swarm;

        /// <summary>
        /// The <see cref="PrivateKey"/> to sign mined <see cref="Block{T}"/>s.
        /// </summary>
        private PrivateKey _privateKey;

        /// <summary>
        /// Initialize a <see cref="Miner"/> instance.
        /// </summary>
        /// <param name="swarm">The <see cref="Swarm{T}"/> to use for mining.</param>
        /// <param name="privateKey">The <see cref="PrivateKey"/> to sign
        /// mined <see cref="Block{T}"/>s.</param>
        public Miner(
            Swarm<PolymorphicAction<ActionBase>> swarm,
            PrivateKey privateKey)
        {
            _swarm = swarm;
            _privateKey = privateKey;
        }

        /// <summary>
        /// Processes mining and wait.
        /// </summary>
        /// <param name="swarm">The <see cref="SwarmRunner"/> to check for preload. </param>
        /// <returns>Mining Coroutine.</returns>
        public IEnumerator CoStart(SwarmRunner swarm)
        {
            while (true)
            {
                yield return new WaitUntil(() => swarm.PreloadTask_ended);

                var task = Task.Run(async () => await Mine());
                yield return new WaitUntil(() => task.IsCompleted);

                if (!task.IsCanceled && !task.IsFaulted)
                {
                    var block = task.Result;
                    Debug.Log($"Created block index[{block.Index}] difficulty {block.Difficulty}.");
                }
                else
                {
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Uses <see cref="BlockChain{T}"/> to implement <see cref="Mine"/>.
        /// </summary>
        /// <returns>An awaitable task with a <see cref="Block{T}"/> that is mined.</returns>
        public async Task<Block<PolymorphicAction<ActionBase>>> Mine()
        {
            var block = await _swarm.BlockChain.MineBlock(_privateKey);

            return block;
        }
    }
}
