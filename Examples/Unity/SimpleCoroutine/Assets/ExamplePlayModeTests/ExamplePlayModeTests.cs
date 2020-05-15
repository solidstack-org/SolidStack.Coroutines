using NUnit.Framework;
using SolidStack.Coroutines;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ExamplePlayModeTests
    {
        /// <summary>
        /// Example play mode integration test - testing component integration within Unity
        /// </summary>
        [UnityTest]
        public IEnumerator DamageOverTime_DealsPlayerDamage_Example1()
        {
            var coroutineService = new CoroutineService();

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            using (DamageOverTime damageOverTime = new GameObject()
                .AddComponent<DamageOverTime>())
            using (Player player = Object.Instantiate(Resources.Load<GameObject>("PlayerPrefab"))
                .GetComponent<Player>())
            {
                var playerHpText = player.GetComponentInChildren<TextMesh>();

                serviceRunner.SetCoroutineService(coroutineService);
                damageOverTime.Setup(coroutineService);

                damageOverTime.DoDamage(player, 10, 2);

                // Wait for Unity seconds
                yield return new WaitForSeconds(2);

                Assert.AreEqual(80, player.Hp, "Incorrect player HP");

                Assert.AreEqual("80/100", playerHpText.text, "Incorrect player hp text");
            }
        }

        /// <summary>
        /// Example play mode test - testing that component deals damage, not using coroutine runner
        /// </summary>
        [Test]
        public void DamageOverTime_DealsPlayerDamage_Example2()
        {
            var coroutineService = new CoroutineService();

            using (DamageOverTime damageOverTime = new GameObject()
                .AddComponent<DamageOverTime>())
            using (Player player = Object.Instantiate(Resources.Load<GameObject>("PlayerPrefab"))
                .GetComponent<Player>())
            {
                var playerHpText = player.GetComponentInChildren<TextMesh>();

                damageOverTime.Setup(coroutineService);

                damageOverTime.DoDamage(player, 10, 2);

                // Set time and tick coroutines
                coroutineService.SetTime(1);
                coroutineService.TickCoroutines();

                Assert.AreEqual(80, player.Hp, "Incorrect player HP");
                Assert.AreEqual("80/100", playerHpText.text, "Incorrect player hp text");
            }
        }
    }
}
