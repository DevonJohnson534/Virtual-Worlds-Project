using UnityEngine;

public class PlayerStatHandler : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public Health health;

    private void Start()
    {
        health.OnTakeDamage += TakeDamage;
        health.OnDeath += Die;
        audioSource = GetComponent<AudioSource>();
    }

    private void TakeDamage(int damage)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(hurtSound);
        }
    }

    private void Die(Vector3 position)
    {
            audioSource.PlayOneShot(deathSound);
    }
}
