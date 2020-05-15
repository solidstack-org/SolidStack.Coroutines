using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;

namespace SolidStack.Coroutines.Tests.Editor
{
    public class CoroutineServiceUnitTests
    {
        #region CoroutineService.StartCoroutine
        [Test]
        public void CoroutineService_StartCoroutine_RunsCoroutine()
        {
            bool didComplete = false;

            IEnumerator RunCoroutine()
            {
                yield return null;
                didComplete = true;
            }

            var coroutineService = new CoroutineService();
            coroutineService.StartCoroutine(RunCoroutine());

            Assert.AreEqual(1, coroutineService.NumScheduledCoroutines, "Coroutine was not scheduled");

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Coroutine did not complete");
        }

        [Test]
        public void CoroutineService_StartCoroutine_RunsCoroutine_MultipleFrames()
        {
            bool didComplete = false;

            IEnumerator RunCoroutine()
            {
                yield return null;
                yield return null;
                didComplete = true;
            }

            var coroutineService = new CoroutineService();
            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickCoroutines();
            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Coroutine did not complete");
        }

        [Test]
        public void CoroutineService_StartCoroutine_RunsCoroutine_WithNestedCoroutine()
        {
            bool didCompleteInner = false;
            bool didCompleteOutter = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutineInner()
            {
                yield return null;
                didCompleteInner = true;
            }

            IEnumerator RunCoroutineOutter()
            {
                yield return coroutineService.StartCoroutine(RunCoroutineInner());
                didCompleteOutter = true;
            }

            coroutineService.StartCoroutine(RunCoroutineOutter());

            coroutineService.TickCoroutines();

            Assert.IsTrue(didCompleteInner, "Coroutine did not complete");
            Assert.IsTrue(didCompleteOutter, "Coroutine did not complete");
        }

        [Test]
        public void CoroutineService_StartCoroutine_ThrowsException_WhenCoroutineThrowsImmediately()
        {
            IEnumerator RunCoroutine()
            {
                throw new InvalidOperationException();
            }

            var coroutineService = new CoroutineService();

            Assert.Throws<InvalidOperationException>(() => coroutineService.StartCoroutine(RunCoroutine()));
        }

        [Test]
        public void CoroutineService_StartCoroutine_RunsParallelCoroutine()
        {
            bool didComplete1 = false;
            bool didComplete2 = false;

            IEnumerator RunCoroutine1()
            {
                yield return null;
                didComplete1 = true;
            }

            IEnumerator RunCoroutine2()
            {
                yield return null;
                yield return null;
                didComplete2 = true;
            }

            var coroutineService = new CoroutineService();
            coroutineService.StartCoroutine(RunCoroutine1());
            coroutineService.StartCoroutine(RunCoroutine2());

            Assert.AreEqual(2, coroutineService.NumScheduledCoroutines, "Coroutine was not scheduled");

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete1, "Coroutine did not complete");
            Assert.AreEqual(1, coroutineService.NumScheduledCoroutines, "Coroutine was not scheduled");

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete2, "Coroutine did not complete");
            Assert.AreEqual(0, coroutineService.NumScheduledCoroutines, "Coroutine was not scheduled");
        }

        [Test]
        public void CoroutineService_StartCoroutine_WithTask_RunsTask()
        {
            Stopwatch sw = new Stopwatch();

            bool didComplete = false;

            var coroutineService = new CoroutineService();

            async Task DoSomethingAsync()
            {
                await coroutineService.WaitForSecondsAsync(0.1f);
                didComplete = true;
            }

            var task = DoSomethingAsync();

            // Start a coroutine
            coroutineService.StartCoroutine(task);

            coroutineService.SetTime(10);
            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Coroutine did not complete");
            Assert.AreEqual(0, coroutineService.NumScheduledCoroutines, "Coroutine was not removed from scheduled");
        }

        #endregion

        #region CoroutineService.TickCoroutines
        [Test]
        public void CoroutineService_TickCoroutines_TicksScheduledCoroutines()
        {
            int numCompleted = 0;

            IEnumerator RunCoroutine()
            {
                yield return null;
                numCompleted++;
            }

            var coroutineService = new CoroutineService();
            coroutineService.StartCoroutine(RunCoroutine());
            coroutineService.StartCoroutine(RunCoroutine());

            Assert.AreEqual(2, coroutineService.NumScheduledCoroutines, "Coroutines were not scheduled");

            coroutineService.TickCoroutines();

            Assert.AreEqual(2, numCompleted, "Coroutines did not complete");
        }

        [Test]
        public void CoroutineService_TickCoroutines_AdjustsTickCount()
        {
            bool isCompleted = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForNumTicks(2);
                isCompleted = true;
            }

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickCoroutines(); // 0->1
            coroutineService.TickCoroutines(); // 1->2

            coroutineService.TickCoroutines(); // 2->3, coroutine completes after 2 full ticks

            Assert.IsTrue(isCompleted, "Coroutine did not complete");
        }

        [Test]
        public void CoroutineService_TickCoroutines_ThrowsException_WhenCoroutineThrowsInThatTick()
        {
            IEnumerator RunCoroutine()
            {
                yield return null;
                throw new InvalidOperationException();
            }

            var coroutineService = new CoroutineService();
            coroutineService.StartCoroutine(RunCoroutine());

            Assert.Throws<InvalidOperationException>(() => coroutineService.TickCoroutines());
        }

        public void CoroutineService_TickCoroutines_AssignsCoroutineException_WhenCoroutineThrowsInThatTick()
        {
            IEnumerator RunCoroutine()
            {
                yield return null;
                throw new InvalidOperationException();
            }

            var coroutineService = new CoroutineService();

            ICoroutine coroutine = coroutineService.StartCoroutine(RunCoroutine());

            try
            {
                coroutineService.TickCoroutines();
            }
            catch
            { }

            Assert.IsInstanceOf(typeof(InvalidOperationException), coroutine.Exception, "Did not assign exception");
        }
        [Test]
        public void CoroutineService_TickCoroutines_WithTask_Throws_WhenTaskFails()
        {
            var coroutineService = new CoroutineService();

            Assert.Throws<InvalidOperationException>(() =>
            {
                coroutineService.StartCoroutine(Task.FromException(new InvalidOperationException()));
            });
        }

        #endregion

        #region CoroutineService.TickLateScheduledCoroutines

        [Test]
        public void CoroutineService_TickLateCoroutines_TicksLateScheduledCoroutine()
        {
            bool isCompleted = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForLateTick();
                isCompleted = true;
            }

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickLateCoroutines();

            Assert.IsTrue(isCompleted, "Coroutine did not complete");
        }
        #endregion

        #region CoroutineService.TickFixedScheduledCoroutines
        [Test]
        public void CoroutineService_TickCoroutines_TicksFixedScheduledCoroutine()
        {
            bool isCompleted = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForFixedTick();
                isCompleted = true;
            }

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickFixedCoroutines();

            Assert.IsTrue(isCompleted, "Coroutine did not complete");
        }
        #endregion

        #region CoroutineService.SetTime
        [Test]
        public void CoroutineService_SetTime_UpdatesCoroutineTime()
        {
            bool isCompleted = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForSeconds(1f);
                isCompleted = true;
            }

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickCoroutines(); // Still waits

            Assert.IsFalse(isCompleted, "Coroutine completed too early");

            coroutineService.SetTime(1f);
            coroutineService.TickCoroutines(); // Completes

            Assert.IsTrue(isCompleted, "Coroutine did not complete");
        }
        #endregion

        #region CoroutineService.SetRealTime
        [Test]
        public void CoroutineService_SetRealTime_UpdatesCoroutineRealTime()
        {
            bool isCompleted = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForSecondsRealtime(1f);
                isCompleted = true;
            }

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickCoroutines(); // Still waits

            Assert.IsFalse(isCompleted, "Coroutine completed too early");

            coroutineService.SetRealTime(1f);
            coroutineService.TickCoroutines(); // Completes

            Assert.IsTrue(isCompleted, "Coroutine did not complete");
        }
        #endregion

        #region CoroutineService.SetNumTick
        [Test]
        public void CoroutineService_SetNumTick_UpdatesCoroutineTickCount()
        {
            bool isCompleted = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForNumTicks(10);
                isCompleted = true;
            }

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickCoroutines(); // Still waits

            Assert.IsFalse(isCompleted, "Coroutine completed too early");

            coroutineService.SetNumTick(10);
            coroutineService.TickCoroutines(); // Completes

            Assert.IsTrue(isCompleted, "Coroutine did not complete");
        }
        #endregion

        #region CoroutineService.StopCoroutine
        [Test]
        public void CoroutineService_StopCoroutine_StopsCoroutine()
        {
            bool didComplete = false;

            IEnumerator RunCoroutine()
            {
                yield return null;
                yield return null;
                didComplete = true;
            }

            var coroutineService = new CoroutineService();
            var coroutine = coroutineService.StartCoroutine(RunCoroutine());
            coroutineService.TickCoroutines();
            coroutineService.StopCoroutine(coroutine);
            coroutineService.TickCoroutines();

            Assert.AreEqual(0, coroutineService.NumScheduledCoroutines, "Coroutine did not stop properly");
            Assert.IsFalse(didComplete, "Coroutine did not stop properly");
        }
        #endregion

        #region WaitFor_Methods
        #region CoroutineService.WaitForSeconds
        [Test]
        public void CoroutineService_WaitForSeconds_CompletesCoroutineAfterNumSeconds()
        {
            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForSeconds(0.1f);
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.SetTime(1);

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion

        #region CoroutineService.WaitForSecondsRealtime
        [Test]
        public void CoroutineService_WaitForSecondsRealtime_CompletesCoroutineAfterNumRealtimeSeconds()
        {
            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForSecondsRealtime(0.1f);
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.SetRealTime(1);

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion

        #region CoroutineService.WaitForLateTick
        [Test]
        public void CoroutineService_WaitForLateTick_CompletesCoroutineAfterNumTicks()
        {
            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForNumTicks(2);
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.SetNumTick(10);

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion

        #region CoroutineService.WaitForFixedTick
        [Test]
        public void CoroutineService_WaitForFixedTick_CompletesCoroutineAfterFixedTick()
        {
            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForFixedTick();
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickFixedCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion

        #region CoroutineService.WaitForNextTick
        [Test]
        public void CoroutineService_WaitForNextTick_CompletesCoroutineAfterNextTicks()
        {
            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForNextTick();
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion

        #region CoroutineService.WaitForNumTicks
        [Test]
        public void CoroutineService_WaitForNumTick_CompletesCoroutineAfterNumTicks()
        {
            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitForNumTicks(2);
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            coroutineService.SetNumTick(10);

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion

        #region CoroutineService.WaitWhile
        [Test]
        public void CoroutineService_WaitWhile_CompletesCoroutineAfterPredicateReturnsTrue()
        {
            bool condition = false;

            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitWhile(() => condition);
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            Assert.IsFalse(didComplete, "Completed too early");

            condition = true;

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion


        #region CoroutineService.WaitUntil
        [Test]
        public void CoroutineService_WaitUntil_CompletesCoroutineAfterPredicateReturnsFalse()
        {
            bool condition = true;

            bool didComplete = false;

            var coroutineService = new CoroutineService();

            IEnumerator RunCoroutine()
            {
                yield return coroutineService.WaitUntil(() => condition);
                didComplete = true;
            };

            coroutineService.StartCoroutine(RunCoroutine());

            Assert.IsFalse(didComplete, "Completed too early");

            condition = false;

            coroutineService.TickCoroutines();

            Assert.IsTrue(didComplete, "Did not complete");
        }
        #endregion
        #endregion

        #region WaitFor_Methods_Async
        #region CoroutineService.WaitForSecondsAsync
        [Test]
        public void CoroutineService_WaitForSecondsAsync_ReturnTaskThatCompletesAfterNumSeconds()
        {
            float timeoutSeconds = 0.2f;
            Stopwatch sw = new Stopwatch();

            var coroutineService = new CoroutineService();
            
            Task task = coroutineService.WaitForSecondsAsync(0.1f);

            sw.Start();

            while (!task.IsCompleted && sw.ElapsedMilliseconds < timeoutSeconds)
            {
                coroutineService.SetTime(sw.ElapsedMilliseconds * 1000);
                Thread.Sleep(1);
            }

            Assert.Less(timeoutSeconds, sw.ElapsedMilliseconds, "Task timed out");
        }
        #endregion

        #region CoroutineService.WaitForSecondsRealtimeAsync
        [Test]
        public void CoroutineService_WaitForSecondsRealtimeAsync_ReturnTaskThatCompletesAfterNumSeconds()
        {
            float timeoutSeconds = 0.2f;
            Stopwatch sw = new Stopwatch();

            var coroutineService = new CoroutineService();

            Task task = coroutineService.WaitForSecondsRealtimeAsync(0.1f);

            sw.Start();

            while (!task.IsCompleted && sw.ElapsedMilliseconds < timeoutSeconds)
            {
                coroutineService.SetRealTime(sw.ElapsedMilliseconds * 1000);
                Thread.Sleep(1);
            }

            Assert.Less(timeoutSeconds, sw.ElapsedMilliseconds, "Task timed out");
        }
        #endregion

        #region CoroutineService.WaitForLateTickAsync
        [Test]
        public void CoroutineService_WaitForLateTickAsync_ReturnTaskThatCompletesAfterLateTick()
        {
            var coroutineService = new CoroutineService();

            Task task = coroutineService.WaitForLateTickAsync();

            coroutineService.TickLateCoroutines();

            Assert.IsTrue(task.IsCompleted, "Task did not complete");
        }
        #endregion

        #region CoroutineService.WaitForFixedTickAsync
        [Test]
        public void CoroutineService_WaitForFixedTickAsync_ReturnTaskThatCompletesAfterFixedTick()
        {
            var coroutineService = new CoroutineService();

            Task task = coroutineService.WaitForFixedTickAsync();

            coroutineService.TickFixedCoroutines();

            Assert.IsTrue(task.IsCompleted, "Task did not complete");
        }
        #endregion

        #region CoroutineService.WaitForNextTickAsync
        [Test]
        public void CoroutineService_WaitForNextTickAsync_ReturnTaskThatCompletesAfterNextTick()
        {
            var coroutineService = new CoroutineService();

            Task task = coroutineService.WaitForNextTickAsync();

            coroutineService.TickCoroutines();

            Assert.IsTrue(task.IsCompleted, "Task did not complete");
        }
        #endregion

        #region CoroutineService.WaitForNumTickAsync
        [Test]
        public void CoroutineService_WaitForNumTicksAsync_ReturnTaskThatCompletesAfterNumTicks()
        {
            var coroutineService = new CoroutineService();

            Task task = coroutineService.WaitForNumTicksAsync(2);

            coroutineService.TickCoroutines();

            Assert.IsFalse(task.IsCompleted, "Task completed too early");

            coroutineService.TickCoroutines();

            Assert.IsTrue(task.IsCompleted, "Task did not complete");

        }
        #endregion

        #region CoroutineService.WaitWhileAsync
        [Test]
        public void CoroutineService_WaitWhileAsync_ReturnTaskThatCompletesAfterPredicateReturnsTrue()
        {
            var coroutineService = new CoroutineService();

            bool condition = true;

            Task task = coroutineService.WaitWhileAsync(() => condition);

            coroutineService.TickCoroutines();

            Assert.IsFalse(task.IsCompleted, "Task completed too early");

            condition = false;

            coroutineService.TickCoroutines();

            Assert.IsTrue(task.IsCompleted, "Task did not complete");
        }
        #endregion

        #region CoroutineService.WaitUntilAsync
        [Test]
        public void CoroutineService_WaitUntilAsync_ReturnTaskThatCompletesAfterPredicateReturnsFalse()
        {
            var coroutineService = new CoroutineService();

            bool condition = false;

            Task task = coroutineService.WaitUntilAsync(() => condition);

            coroutineService.TickCoroutines();

            Assert.IsFalse(task.IsCompleted, "Task completed too early");

            condition = true;

            coroutineService.TickCoroutines();

            Assert.IsTrue(task.IsCompleted, "Task did not complete");
        }
        #endregion
        #endregion
    }
}