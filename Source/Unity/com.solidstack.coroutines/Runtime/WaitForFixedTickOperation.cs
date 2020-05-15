using System;

namespace SolidStack.Coroutines
{
    class WaitForFixedTickOperation : IAsyncOperation
    {
        /// <summary>
        /// Set automatically by coroutine service when next fixed tick occurs
        /// </summary>
        public bool IsDone { get; set; } = false;

        /// <summary>
        /// Waiting for fixed update can not fail
        /// </summary>
        public Exception Exception => null;
    }
}
