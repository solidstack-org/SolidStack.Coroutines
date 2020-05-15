using System;
using System.Collections;
#if UNITY
using UnityEngine;
#endif

namespace SolidStack.Coroutines
{
    /// <summary>
    /// Coroutine Handle 
    /// Stores a handle to a running coroutine
    /// </summary>
    public class Coroutine :
#if UNITY
        CustomYieldInstruction,
#endif
        ICoroutine
    {
        /// <summary>
        /// Current routine
        /// </summary>
        private IEnumerator _routine;

        public bool IsRunning { get; private set; } = true;

        public bool IsDone => !IsRunning;

        public Exception Exception { get; private set; } = null;

        public IAsyncOperation DelayOperation { get; private set; } = null;

        // CustomYieldInstruction implementation
#if UNITY
        public override bool keepWaiting => IsRunning;
#endif

        public Coroutine(IEnumerator routine)
        {
            _routine = routine;
        }

        internal ICoroutineTickResult Tick()
        {
            // Check if delay operation is present, and if it has ended or not
            if (DelayOperation != null)
            {
                if(!DelayOperation.IsDone)
                {
                    return CoroutineTickResult.Delayed(DelayOperation);
                }
                else
                {
                    // Reset delay operation and continue
                    DelayOperation = null;
                }
            }

            // Advance coroutine
            bool hasEnded;
            try
            {
                hasEnded = !_routine.MoveNext();
            }
            catch(Exception e)
            {
                Exception = e;
                IsRunning = false;
                throw;
            }

            // If coroutine can not be advanced any more, stop
            if (hasEnded)
            {
                IsRunning = false;
                return CoroutineTickResult.Stopped;
            }

            // Read current value
            object value = _routine.Current;

            // Check if returned value is another async operation
            if (value is IAsyncOperation)
            {
                // Value is async operation, such as another coroutine.
                // wait until it's done before progressing
                IAsyncOperation delayOperation = (IAsyncOperation)value;
                if(!delayOperation.IsDone)
                {
                    DelayOperation = delayOperation;
                    return CoroutineTickResult.Delayed(delayOperation);
                }
            }

            return CoroutineTickResult.Running;
        }

        internal void Stop()
        {
            IsRunning = false;
        }
    }
}