using System;
using UnityEngine;

public class PlayerCombatSystem : MonoBehaviour, IDamageable
{
    public int Maxhealth { get => _maxHealth; set => _maxHealth = value; }
    public int Damage { get => _damage; set => _damage = value; }

    [SerializeField] Sword _sword;

    [SerializeField] private int _maxHealth;
    [SerializeField] private int _damage;
    private int _hp;

    private void Awake()
    {
        _hp = Maxhealth;
    }

    public void GetDamage(int damage)
    {
        _hp -= damage;
        if (_hp <= 0)
        {
            Debug.Log("Сдох");
        }
    }

    public void ActivateSword()
    {
        _sword.gameObject.SetActive(true);
    }

    public void DeactivateSword()
    {
        _sword.gameObject.SetActive(false);
    }

    public void Attack(IDamageable target, int damage)
    {
        throw new NotImplementedException();
    }
}
