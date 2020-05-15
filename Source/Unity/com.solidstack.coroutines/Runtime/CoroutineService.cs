using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace SolidStack.Coroutines
{
    /// <summary>
    /// Runtime Coroutine Service implementation.
    /// </summary>
    public class CoroutineService : ICoroutineService
    {
        /// <summary>
        /// Coroutines that will be invoked every frame
        /// </summary>
        private Queue<Coroutine> _coroutines;

        /// <summary>
        /// Stores coroutines that are scheduled for the next late update
        /// </summary>
        private Queue<Coroutine> _lateTickScheduledCoroutines;

        /// <summary>
        /// Stores coroutines that are scheduled for the next fixed update
        /// </summary>
        private Queue<Coroutine> _fixedTickScheduledCoroutines;

        /// <summary>
        /// Delayed coroutine map.
        /// Async operation that delays the coroutine -> queue of coroutines
        /// </summary>
        private Dictionary<IAsyncOperation, Queue<Coroutine>> _delayedCoroutines;

        /// <summary>
        /// Current time. Update using SetTime()
        /// </summary>
        public float TimeSeconds { get; private set; } = 0f;

        /// <summary>
        /// Real time since startup. Update using SetRealTime();
        /// </summary>
        public float RealtimeSinceStartupSeconds { get; private set; } = 0f;

        /// <summary>
        /// Current frame number. Update using SetNumTick()
        /// </summary>
        public int NumTick { get; private set; } = 0;

        /// <summary>
        /// Number of scheduled coroutines
        /// </summary>
        public int NumScheduledCoroutines => _coroutines.Count;

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoroutineService()
        {
            _coroutines = new Queue<Coroutine>();
            _lateTickScheduledCoroutines = new Queue<Coroutine>();
            _fixedTickScheduledCoroutines = new Queue<Coroutine>();
            _delayedCoroutines = new Dictionary<IAsyncOperation, Queue<Coroutine>>();
        }
        #endregion

        #region Time Update
        public void SetTime(float timeSeconds)
        {
            TimeSeconds = timeSeconds;
        }

        public void SetRealTime(float realTimeSeconds)
        {
            RealtimeSinceStartupSeconds = realTimeSeconds;
        }

        public void SetNumTick(int numTick)
        {
            NumTick = numTick;
        }
        #endregion

        #region Start Coroutine Implementation
        /// <inheritdoc/>
        public ICoroutine StartCoroutine(IEnumerator routine)
        {
            Coroutine coroutine = new Coroutine(routine);

            TickCoroutine(coroutine);

            return coroutine;
        }


        /// <inheritdoc/>
        public ICoroutine StartCoroutine(Task task)
        {
            return StartCoroutine(WaitUntilTaskCompletes(task));
        }

        /// <inheritdoc/>
        public Task StartCoroutineAsync(Func<IEnumerator> enumeratorHandler)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            StartCoroutine(WaitForCoroutineAndCompleteTcsCoro(enumeratorHandler, tcs));

            return tcs.Task;
        }

        private IEnumerator WaitForCoroutineAndCompleteTcsCoro(Func<IEnumerator> enumeratorHandler, TaskCompletionSource<object> tcs)
        {
            ICoroutine coroutine;
            
            // First run - if coroutine fails here, set task exception too
            try
            {
                coroutine = StartCoroutine(enumeratorHandler());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
                yield break;
            }

            // Wait until this coroutine is finished
            yield return coroutine;

            // Check if coroutine has completed successfully
            if (coroutine.Exception == null)
            {
                tcs.SetResult(null);
            }
            else
            {
                tcs.SetException(coroutine.Exception);
            }
        }

        private IEnumerator WaitUntilTaskCompletes(Task task)
        {
            while(!task.IsCompleted)
            {
                yield return null;
            }

            if(task.IsFaulted)
            {
                // Throw task exception, preserving original stack trace
                ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
            }
        }
        #endregion

        #region  Stop Coroutine Implementation
        /// <inheritdoc/>
        public void StopCoroutine(ICoroutine coroutine)
        {
            if(coroutine == null)
            {
                throw new ArgumentNullException(nameof(coroutine), "Coroutine Handle can not be null");
            }

            Coroutine coro;
            
            try
            {
                coro = (Coroutine)coroutine;
            }
            catch(InvalidCastException e)
            {
                throw new InvalidCastException("Coroutine Handle must be implemented by Coroutine", e);
            }

            // Will mark for removal
            coro.Stop();
        }
        #endregion

        #region Service Implementation
        public void TickCoroutines()
        {
            // Increment tick count
            NumTick++;

            // Do not process newly added coroutines
            int numCoroutines = _coroutines.Count; 

            for(int numCoro = 0; numCoro < numCoroutines; numCoro++)
            {
                var coroutine = _coroutines.Dequeue();

                TickCoroutine(coroutine);
            }
        }

        public void TickLateCoroutines()
        {
            // Do not process newly added coroutines
            int numCoroutines = _lateTickScheduledCoroutines.Count;

            for (int numCoro = 0; numCoro < numCoroutines; numCoro++)
            {
                var coroutine = _lateTickScheduledCoroutines.Dequeue();

                var delayOperation = (WaitForLateTickOperation)coroutine.DelayOperation;

                // Update delay operation
                delayOperation.IsDone = true;

                TickCoroutine(coroutine);
            }
        }

        public void TickFixedCoroutines()
        {
            // Do not process newly added coroutines
            int numCoroutines = _fixedTickScheduledCoroutines.Count;

            for (int numCoro = 0; numCoro < numCoroutines; numCoro++)
            {
                var coroutine = _fixedTickScheduledCoroutines.Dequeue();

                var delayOperation = (WaitForFixedTickOperation)coroutine.DelayOperation;

                // Update delay operation
                delayOperation.IsDone = true;

                TickCoroutine(coroutine);
            }
        }

        private void TickCoroutine(Coroutine coroutine)
        {
            // If coroutine is not running, do not do anything
            if (!coroutine.IsRunning)
            {
                return;
            }

            // Tick coroutine!
            ICoroutineTickResult coroutineTickResult = coroutine.Tick();

            // Check tick result
            if(coroutineTickResult is CoroutineTickResult.TickResultRunning)
            {
                // Still running - enqueue to be ticked on the next tick
                _coroutines.Enqueue(coroutine);
                return;
            }
            else if(coroutineTickResult is CoroutineTickResult.TickResultStopped)
            {
                // Stopped. Do not enqueue. Check if any other coroutines waited for it to complete.
                if(_delayedCoroutines.TryGetValue(coroutine, out Queue<Coroutine> delayedCoroutineQueue))
                {
                    int numCoroutines = delayedCoroutineQueue.Count;

                    for (int numCoro = 0; numCoro < numCoroutines; numCoro++)
                    {
                        var dependendCoroutine = delayedCoroutineQueue.Dequeue();

                        TickCoroutine(dependendCoroutine);
                    }
                }
                return;
            }
            if(coroutineTickResult is CoroutineTickResult.TickResultDelayed)
            {
                // Delayed by other async operation. Check kind of async operation and delay it.

                var tickResultDelayed = (CoroutineTickResult.TickResultDelayed)coroutineTickResult;

                if (tickResultDelayed.DelayOperation is WaitForLateTickOperation)
                {
                    ScheduleCoroutineForLateTick(coroutine);
                }
                else if (tickResultDelayed.DelayOperation is WaitForFixedTickOperation)
                {
                    ScheduleCoroutineForFixedTick(coroutine);
                }
                else
                {
                    // Coroutine or an unknown delay operation. Add to delay operation map
                    Queue<Coroutine> delayedCoroutineQueue;

                    if (!_delayedCoroutines.TryGetValue(tickResultDelayed.DelayOperation, out delayedCoroutineQueue))
                    {
                        delayedCoroutineQueue = new Queue<Coroutine>();
                        _delayedCoroutines.Add(tickResultDelayed.DelayOperation, delayedCoroutineQueue);
                    }

                    delayedCoroutineQueue.Enqueue(coroutine);
                }
            }
        }

        private void ScheduleCoroutineForLateTick(Coroutine coroutine)
        {
            _lateTickScheduledCoroutines.Enqueue(coroutine);
        }

        private void ScheduleCoroutineForFixedTick(Coroutine coroutine)
        {
            _fixedTickScheduledCoroutines.Enqueue(coroutine);
        }
        #endregion

        #region Wait For Methods

        /// <inheritdoc/>
        public IAsyncOperation WaitForSeconds(float seconds)
        {
            return StartCoroutine(WaitForSecondsCoro(seconds));
        }

        IEnumerator WaitForSecondsCoro(float seconds)
        {
            float startTime = TimeSeconds;

            while (TimeSeconds < startTime + seconds)
            {
                yield return null;
            }
        }
        /// <inheritdoc/>

        public IAsyncOperation WaitForSecondsRealtime(float seconds)
        {
            return StartCoroutine(WaitForSecondsRealtimeCoro(seconds));
        }

        IEnumerator WaitForSecondsRealtimeCoro(float seconds)
        {
            float startTime = RealtimeSinceStartupSeconds;

            while (RealtimeSinceStartupSeconds < startTime + seconds)
            {
                yield return null;
            }
        }

        /// <inheritdoc/>
        public IAsyncOperation WaitForLateTick()
        {
            return new WaitForLateTickOperation();
        }

        private IEnumerator WaitForLateTickCoro()
        {
            yield return WaitForLateTick();
        }

        /// <inheritdoc/>
        public IAsyncOperation WaitForFixedTick()
        {
            return new WaitForFixedTickOperation();
        }

        private IEnumerator WaitForFixedTickCoro()
        {
            yield return WaitForFixedTick();
        }

        /// <inheritdoc/>
        public IAsyncOperation WaitForNextTick()
        {
            return StartCoroutine(WaitForNextTickCoro());
        }

        private IEnumerator WaitForNextTickCoro()
        {
            yield return null;
        }

        /// <inheritdoc/>
        public IAsyncOperation WaitForNumTicks(int numTicks)
        {
            return StartCoroutine(WaitForNumTicksCoro(numTicks));
        }

        private IEnumerator WaitForNumTicksCoro(int numTicks)
        {
            int numTick = NumTick;

            while(NumTick < numTick + numTicks)
            {
                yield return null;
            }
        }

        /// <inheritdoc/>
        public IAsyncOperation WaitWhile(Func<bool> predicate)
        {
            return StartCoroutine(WaitWhileCoro(predicate));
        }

        private IEnumerator WaitWhileCoro(Func<bool> predicate)
        {
            while (predicate())
            {
                yield return null;
            }
        }

        /// <inheritdoc/>
        public IAsyncOperation WaitUntil(Func<bool> predicate)
        {
            return StartCoroutine(WaitUntilCoro(predicate));
        }

        private IEnumerator WaitUntilCoro(Func<bool> predicate)
        {
            while (!predicate())
            {
                yield return null;
            }
        }
        #endregion

        #region Helper Async Methods
        /// <inheritdoc/>
        public Task WaitForSecondsAsync(float seconds)
        {
            return StartCoroutineAsync(() => WaitForSecondsCoro(seconds));
        }

        /// <inheritdoc/>
        public Task WaitForSecondsRealtimeAsync(float seconds)
        {
            return StartCoroutineAsync(() => WaitForSecondsRealtimeCoro(seconds));
        }

        /// <inheritdoc/>
        public Task WaitForLateTickAsync()
        {
            return StartCoroutineAsync(() => WaitForLateTickCoro());
        }

        /// <inheritdoc/>
        public Task WaitForFixedTickAsync()
        {
            return StartCoroutineAsync(() => WaitForFixedTickCoro());
        }

        /// <inheritdoc/>
        public Task WaitForNextTickAsync()
        {
            return StartCoroutineAsync(() => WaitForNextTickCoro());
        }

        /// <inheritdoc/>
        public Task WaitForNumTicksAsync(int numTicks)
        {
            return StartCoroutineAsync(() => WaitForNumTicksCoro(numTicks));
        }

        /// <inheritdoc/>
        public Task WaitWhileAsync(Func<bool> predicate)
        {
            return StartCoroutineAsync(() => WaitWhileCoro(predicate));
        }

        /// <inheritdoc/>
        public Task WaitUntilAsync(Func<bool> predicate)
        {
            return StartCoroutineAsync(() => WaitUntilCoro(predicate));
        }

        #endregion
    }
}
