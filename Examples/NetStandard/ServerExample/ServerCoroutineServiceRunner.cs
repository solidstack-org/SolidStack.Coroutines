using SolidStack.Coroutines;
using System;
using System.Diagnostics;

namespace ServerExample
{
    class ServerCoroutineServiceRunner
    {
        private readonly CoroutineService _coroutineService;

        public ServerCoroutineServiceRunner(CoroutineService coroutineService)
        {
            _coroutineService = coroutineService;
        }

        public void Tick()
        {
            UpdateCoroutineServiceTime();
            _coroutineService.TickCoroutines();
            _coroutineService.TickFixedCoroutines();
            _coroutineService.TickLateCoroutines();
        }

        private void UpdateCoroutineServiceTime()
        {
            TimeSpan timeSinceStartup = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();

            _coroutineService?.SetTime((float)timeSinceStartup.TotalSeconds);
            _coroutineService?.SetTime((float)timeSinceStartup.TotalSeconds);
        }
    }
}
