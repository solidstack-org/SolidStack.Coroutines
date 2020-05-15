using System;

namespace SolidStack.Coroutines
{
    /// <summary>
    /// Any async operation can be treated as a coroutine tick result
    /// (coroutine yields until async operation is finished)
    /// </summary>
    public interface IAsyncOperation : ICoroutineTickResult
    {
        bool IsDone { get; }

        Exception Exception { get; }
    }
}