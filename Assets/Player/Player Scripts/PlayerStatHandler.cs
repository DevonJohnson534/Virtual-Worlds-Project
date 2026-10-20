using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatHandler : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public PlayerHUD playerHUD;
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
        playerHUD.UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
    }

    private void Die(Vector3 position)
    {
            audioSource.PlayOneShot(deathSound);
            // Reloads the scene, functions like a respawn.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
