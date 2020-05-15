using System.Threading.Tasks;
using System;
using System.Collections;

namespace SolidStack.Coroutines
{
    /// <summary>
    /// Coroutine Service.
    /// Allows to run Unity coroutines independently from
    /// unity GameObjects and provides simple compatibility
    /// layer to work with Tasks.
    /// </summary>
    public interface ICoroutineService
    {
        #region Properties
        /// <summary>
        /// Current time
        /// </summary>
        float TimeSeconds { get; }
        /// <summary>
        /// Real time since startup
        /// </summary>
        float RealtimeSinceStartupSeconds { get; }

        /// <summary>
        /// Current frame number
        /// </summary>
        int NumTick { get; }
        #endregion

        /// <summary>
        /// Starts a Coroutine
        /// </summary>
        /// <param name="routine">IEnumerator to start</param>
        /// <returns>Coroutine Handle</returns>
        ICoroutine StartCoroutine(IEnumerator routine);

        /// <summary>
        /// Starts a Coroutine that runs until given Task completes
        /// </summary>
        /// <param name="task">Task to convert to a Coroutine</param>
        /// <returns>Coroutine Handle</returns>
        ICoroutine StartCoroutine(Task task);

        /// <summary>
        /// Starts a Coroutine that will complete or fail a returned Task object
        /// </summary>
        /// <param name="routine">Method that starts IEnumerator to start</param>
        /// <returns>Task object that completes when coroutine has finished</returns>
        Task StartCoroutineAsync(Func<IEnumerator> routine);

        /// <summary>
        /// Stops a Coroutine
        /// </summary>
        /// <param name="routine">Coroutine Handle to stop</param>
        void StopCoroutine(ICoroutine routine);

        #region Helper Methods
        /// <summary>
        /// Waits for number of given seconds
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForSeconds"/>
        /// <param name="seconds">Number of seconds to wait</param>
        /// <returns>Async operation that completes after a number of given seconds</returns>
        IAsyncOperation WaitForSeconds(float seconds);

        /// <summary>
        /// Waits for number of given seconds using real time, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForSecondsRealtime"/>
        /// <param name="seconds"></param>
        /// <returns>Async operation that completes after a number of given seconds</returns>
        IAsyncOperation WaitForSecondsRealtime(float seconds);

        /// <summary>
        /// Waits until the end of frame
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForEndOfFrame"/>
        /// <returns>Async operation that completes after the end of frame</returns>
        IAsyncOperation WaitForLateTick();

        /// <summary>
        /// Waits until the next fixed frame rate update, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForFixedUpdate"/>
        /// <returns>Async operation that completes after the next fixed update</returns>
        IAsyncOperation WaitForFixedTick();

        /// <summary>
        /// Waits until the next tick, then completes returned Task object
        /// </summary>
        /// <returns>Async operation that completes after the next tick</returns>
        IAsyncOperation WaitForNextTick();

        /// <summary>
        /// Waits for a given number of ticks
        /// </summary>
        /// <param name="numTicks"></param>
        /// <returns>Async Operation that completes when given number of ticks has passed</returns>
        IAsyncOperation WaitForNumTicks(int numTicks);

        /// <summary>
        /// Waits until given delegate evaluates to false, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitWhile"/>
        /// <param name="predicate">Predicate method that must evaluate to false for Task to complete</param>
        /// <returns>Async operation that completes after given predicate evaluates to false</returns>
        IAsyncOperation WaitWhile(Func<bool> predicate);

        /// <summary>
        /// Waits until given delegate evaluates to true, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitUntil"/>
        /// <param name="predicate">Predicate method that must evaluate to true for Task to complete</param>
        /// <returns>Async operation that completes after given predicate evaluates to true</returns>
        IAsyncOperation WaitUntil(Func<bool> predicate);
        #endregion

        #region Helper Async Methods
        /// <summary>
        /// Waits for number of given seconds, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForSeconds"/>
        /// <param name="seconds">Number of seconds to wait</param>
        /// <returns>Task object that completes after a number of given seconds</returns>
        Task WaitForSecondsAsync(float seconds);

        /// <summary>
        /// Waits for number of given seconds using real time, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForSecondsRealtime"/>
        /// <param name="seconds"></param>
        /// <returns>Task object that completes after a number of given seconds</returns>
        Task WaitForSecondsRealtimeAsync(float seconds);

        /// <summary>
        /// Waits until the end of frame, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForEndOfFrame"/>
        /// <returns>Task object that completes after the end of frame</returns>
        Task WaitForLateTickAsync();

        /// <summary>
        /// Waits until the next fixed frame rate update, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitForFixedUpdate"/>
        /// <returns>Task object that completes after the next fixed update</returns>
        Task WaitForFixedTickAsync();

        /// <summary>
        /// Waits until the next tick, then completes returned Task object
        /// </summary>
        /// <returns>Task object that completes after the next tick</returns>
        Task WaitForNextTickAsync();

        /// <summary>
        /// Waits for a given number of ticks, then completes returned Task object
        /// </summary>
        /// <param name="numTicks"></param>
        /// <returns>Async Operation that completes when given number of ticks has passed</returns>
        Task WaitForNumTicksAsync(int numTicks);

        /// <summary>
        /// Waits until given delegate evaluates to false, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitWhile"/>
        /// <param name="predicate">Predicate method that must evaluate to false for Task to complete</param>
        /// <returns>Task object that completes after given predicate evaluates to false</returns>
        Task WaitWhileAsync(Func<bool> predicate);

        /// <summary>
        /// Waits until given delegate evaluates to true, then completes returned Task object
        /// </summary>
        /// <seealso cref="UnityEngine.WaitUntil"/>
        /// <param name="predicate">Predicate method that must evaluate to true for Task to complete</param>
        /// <returns>Task object that completes after given predicate evaluates to true</returns>
        Task WaitUntilAsync(Func<bool> predicate);
        #endregion
    }
}
