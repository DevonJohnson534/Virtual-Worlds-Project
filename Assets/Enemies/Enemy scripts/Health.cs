using UnityEngine;

public class Health : MonoBehaviour, Damageable
{
    [SerializeField]
    private int _maxHealth = 100;
    [SerializeField]
    private int _health;
    public int CurrentHealth { get => _health; private set => _health = value; }
    public int MaxHealth { get => _maxHealth; private set => _maxHealth = value; }

    public event Damageable.TakeDamageEvent OnTakeDamage;
    public event Damageable.DeathEvent OnDeath;

    private void OnEnable()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Clamp(damage, 0, CurrentHealth);
        CurrentHealth -= damageTaken;
        if(damageTaken != 0)
        {
            // If we're dying we don't wanna take damage again.
            OnTakeDamage?.Invoke(damageTaken);
        }
        if(CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
    }
}
