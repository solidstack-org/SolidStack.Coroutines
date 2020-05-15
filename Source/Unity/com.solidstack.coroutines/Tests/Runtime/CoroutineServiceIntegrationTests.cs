using System.Collections;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace SolidStack.Coroutines.Tests.Runtime
{
    public class CoroutineServiceIntegrationTests
    {
        #region CoroutineService.StartCoroutine
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_StartCoroutine_RunsCoroutine_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return null;
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                // This test will complete before ServiceRunner receives it's first update, depending on order of execution
                yield return null;
                yield return null;

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region WaitForSeconds
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForSeconds_CompletesCoroutineAfterSeconds_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return coroutineService.WaitForSeconds(0.1f);
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                yield return new WaitForSeconds(0.11f);

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region WaitForSecondsRealtime
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForSecondsRealtime_CompletesCoroutineAfterRealtimeSeconds_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return coroutineService.WaitForSecondsRealtime(0.1f);
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                yield return new WaitForSecondsRealtime(0.11f);

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region WaitForLateTick
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForLateTick_WaitsForLateUpdate_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return coroutineService.WaitForLateTick();
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                // This test may complete before ServiceRunner receives it's first update, depending on order of execution
                yield return null;
                yield return null;

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region WaitForFixedTick
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForFixedTick_WaitsForFixedUpdate_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return coroutineService.WaitForFixedTick();
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                yield return new WaitForFixedUpdate();

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region WaitForNextTick
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForNextTick_WaitsForNextFrame_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return coroutineService.WaitForNextTick();
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                // This test will complete before ServiceRunner receives it's first update, depending on order of execution
                yield return null;
                yield return null;

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region WaitForNumTicks
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForNumTicks_WaitsForNumTicks_WithServiceRunner()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return coroutineService.WaitForNumTicks(2);
                    didComplete = true;
                }

                coroutineService.StartCoroutine(RunCoroutine());

                // This test may complete before ServiceRunner receives it's first update, depending on order of execution
                yield return null;
                yield return null;

                Assert.IsTrue(didComplete, "Coroutine did not complete");
            }
        }
        #endregion

        #region CoroutineService.WaitForSecondsAsync
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForSecondsAsync_WaitForSeconds()
        {
            var coroutineService = new CoroutineService();
            float delaySeconds = 0.1f;
            float startTimeSeconds = Time.time;

            coroutineService.SetTime(startTimeSeconds);

            Task task = coroutineService.WaitForSecondsAsync(delaySeconds);

            while(!task.IsCompleted)
            {
                coroutineService.SetTime(Time.time);
                coroutineService.TickCoroutines();
                yield return null;
            }

            Assert.IsTrue(Time.time >= startTimeSeconds + delaySeconds, "Not enough time has passed");
        }
        #endregion

        #region CoroutineService.WaitForSecondsRealtimeAsync
        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForSecondsRealtimeAsync_WaitForSecondsRealtime()
        {
            var coroutineService = new CoroutineService();
            float delaySeconds = 0.05f;
            float startTimeSeconds = Time.realtimeSinceStartup;
            coroutineService.SetRealTime(startTimeSeconds);
            Task task = coroutineService.WaitForSecondsRealtimeAsync(delaySeconds);

            while (!task.IsCompleted)
            {
                coroutineService.SetRealTime(Time.realtimeSinceStartup);
                coroutineService.TickCoroutines();
                yield return null;
            }

            Assert.IsTrue(Time.realtimeSinceStartup >= startTimeSeconds + delaySeconds, "Not enough time has passed");
        }
        #endregion

        #region CustomYieldInstruction

        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_StartCoroutine_RunsAsUnityCoroutine()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return null;
                    didComplete = true;
                }

                yield return coroutineService.StartCoroutine(RunCoroutine());

                Assert.IsTrue(didComplete, "Coroutine did complete when it shouldn't have");
            }
        }

        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CoroutineService_WaitForLateTick_RunsAsUnityCoroutine()
        {
            bool didComplete = false;

            using (CoroutineServiceRunner serviceRunner = new GameObject()
                .AddComponent<CoroutineServiceRunner>())
            {
                var coroutineService = new CoroutineService();
                serviceRunner.SetCoroutineService(coroutineService);

                IEnumerator RunCoroutine()
                {
                    yield return null;
                    didComplete = true;
                }

                yield return coroutineService.StartCoroutine(RunCoroutine());

                Assert.IsTrue(didComplete, "Coroutine did complete when it shouldn't have");
            }
        }
        #endregion
    }
}