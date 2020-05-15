using Moq;
using NUnit.Framework;
using SolidStack.Coroutines;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ExampleEditModeTests
    {
        /// <summary>
        /// Example edit mode unit test - testing with mock ICoroutineService and IPlayer
        /// <seealso cref="https://github.com/Moq/moq4/wiki/Quickstart"/>
        /// </summary>
        [Test]
        public void DamageOverTime_DealsPlayerDamage_Example3()
        {
            IEnumerator coro = null;
            float numSecond = 0;
            float totalDamageDealt = 0;

            // Setup mock Coroutine Service
            var coroutineServiceMock = new Mock<ICoroutineService>();
            coroutineServiceMock.SetupGet(coroutineService => coroutineService.TimeSeconds)
                .Returns(() => numSecond);

            coroutineServiceMock.Setup(coroutineService => coroutineService.StartCoroutine(It.IsAny<IEnumerator>()))
                .Callback<IEnumerator>(c => coro = c);

            // Setup mock player
            var playerMock = new Mock<IPlayer>();
            playerMock.Setup(player => player.DealDamage(It.IsAny<float>()))
                .Callback<float>(dmg => totalDamageDealt += dmg);

            DamageOverTime damageOverTime = new GameObject().AddComponent<DamageOverTime>();
            damageOverTime.Setup(coroutineServiceMock.Object);

            // Deal 10 damage every seconds, for 2 seconds
            damageOverTime.DoDamage(playerMock.Object, 10, 2);

            // Simulate iterating coroutine
            while(coro.MoveNext() && numSecond < 10)
            {
                numSecond++;
            }
                
            Assert.AreEqual(20, totalDamageDealt, "Did not deal 20 damage");
        }
    }
}
