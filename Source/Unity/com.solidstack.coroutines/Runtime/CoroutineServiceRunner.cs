using System;
using UnityEngine;

namespace SolidStack.Coroutines
{
    /// <summary>
    /// Simple MonoBehaviour that advances coroutine service
    /// based on Unity event loop.
    /// </summary>
    public class CoroutineServiceRunner : MonoBehaviour, IDisposable
    {
        public CoroutineService CoroutineService { get; set; }

        public void SetCoroutineService(CoroutineService coroutineService)
        {
            CoroutineService = coroutineService;
        }

        private void Update()
        {
            if (CoroutineService != null)
            {
                UpdateCoroutineServiceTime();
                CoroutineService.TickCoroutines();
            }
        }

        private void FixedUpdate()
        {
            if (CoroutineService != null)
            {
                UpdateCoroutineServiceTime();
                CoroutineService.TickFixedCoroutines();
            }
        }

        private void LateUpdate()
        {
            if (CoroutineService != null)
            {
                UpdateCoroutineServiceTime();
                CoroutineService.TickLateCoroutines();
            }
        }

        private void UpdateCoroutineServiceTime()
        {
            CoroutineService.SetTime(Time.time);
            CoroutineService.SetRealTime(Time.realtimeSinceStartup);
        }

        public void Dispose()
        {
            GameObject.Destroy(gameObject);
        }
    }
}