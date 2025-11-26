using UnityEngine;

public interface IDamageable
{
    public int Maxhealth { get; set; }
    public int Damage {  get; set; }

    public void Attack(int damage);
    public void GetDamage(int damage);
}

