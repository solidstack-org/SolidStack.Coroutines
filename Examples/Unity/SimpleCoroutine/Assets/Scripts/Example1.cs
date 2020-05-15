using SolidStack.Coroutines;
using UnityEngine;

public class Example1 : MonoBehaviour
{
    [SerializeField]
    private CoroutineServiceRunner _coroutineServiceRunner;

    [SerializeField]
    private Player _player;

    [SerializeField]
    private DamageOverTime _damageOverTime;

    public void Awake()
    {
        // Setup coroutine service
        CoroutineService coroutineService = new CoroutineService();
        _coroutineServiceRunner.SetCoroutineService(coroutineService);

        // Setup DOT component
        _damageOverTime.Setup(coroutineService);

        // Start dealing damage
        _damageOverTime.DoDamage(_player, 20, 5);
    }
}
