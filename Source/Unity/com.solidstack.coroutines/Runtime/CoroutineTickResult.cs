namespace SolidStack.Coroutines
{
    /// <summary>
    /// Result of a coroutine tick
    /// </summary>
    static class CoroutineTickResult
    {
        public static TickResultRunning Running = new TickResultRunning();

        public static TickResultStopped Stopped = new TickResultStopped();

        public static TickResultDelayed Delayed(IAsyncOperation asyncOperation) 
            => new TickResultDelayed() { DelayOperation = asyncOperation };


        public class TickResultRunning : ICoroutineTickResult { }

        public class TickResultStopped : ICoroutineTickResult { }

        public class TickResultDelayed : ICoroutineTickResult 
        {
            public IAsyncOperation DelayOperation { get; set; }
        }
    }
}
