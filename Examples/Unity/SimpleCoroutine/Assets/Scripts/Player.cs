using System;
using UnityEngine;

public interface IPlayer
{
    void DealDamage(float dmg);
}

public class Player : MonoBehaviour, IPlayer, IDisposable
{
    [SerializeField]
    private TextMesh _hpText;

    public float Hp { get; private set; } = 100;

    public void DealDamage(float dmg)
    {
        Hp = Mathf.Max(0, Hp - dmg);

        if(Hp > 0)
        {
            _hpText.text = $"{Hp}/100";
        }
        else
        {
            _hpText.text = "dead";
        }

        Debug.Log($"Player {gameObject.name} received damage: {dmg}, current hp: {Hp}");
    }

    public void Dispose()
    {
        Destroy(gameObject);
    }
}
