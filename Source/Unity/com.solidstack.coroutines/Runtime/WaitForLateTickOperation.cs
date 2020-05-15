using System;

namespace SolidStack.Coroutines
{
    class WaitForLateTickOperation : IAsyncOperation
    {
        /// <summary>
        /// Set automatically by coroutine service when next late tick occurs
        /// </summary>
        public bool IsDone { get; set; } = false;

        /// <summary>
        /// Waiting for fixed update can not fail
        /// </summary>
        public Exception Exception => null;
    }
}
