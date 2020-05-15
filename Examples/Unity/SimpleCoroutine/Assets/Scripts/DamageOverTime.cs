using SolidStack.Coroutines;
using System.Collections;
using UnityEngine;
using System;

public class DamageOverTime : MonoBehaviour, IDisposable
{
    private ICoroutineService _coroutineService;


    public void Setup(ICoroutineService coroutineService)
    {
        _coroutineService = coroutineService;
    }

    public void DoDamage(IPlayer player, float damagePerSecond, int timeSeconds)
    {
        _coroutineService.StartCoroutine(DoDamageCoroutine(player, damagePerSecond, timeSeconds));
    }

    private IEnumerator DoDamageCoroutine(IPlayer player, float damagePerSecond, int timeSeconds)
    {
        float startTime = _coroutineService.TimeSeconds;
        float endTime = startTime + timeSeconds; 

        while(_coroutineService.TimeSeconds < endTime)
        {
            player.DealDamage(damagePerSecond);
            yield return _coroutineService.WaitForSeconds(1.0f);
        }
    }

    public void Dispose()
    {
        Destroy(gameObject);
    }
}
