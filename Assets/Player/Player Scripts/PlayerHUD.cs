using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class PlayerHUD : MonoBehaviour
{
    [Header("Health Bar Visuals")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image damageFill;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float textPulseDuration = 0.2f;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float delayBeforeDrain = 0.5f;

    private float drainTimer;
    private float targetFillAmount = 1f;

    private Color originalColor;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = healthText.transform.localScale;
        originalColor = healthText.color;
    }
    // Update is called once per time interval
    void FixedUpdate()
    {
        fillImage.fillAmount = targetFillAmount;
        if(drainTimer > 0)
        {
            drainTimer -= Time.deltaTime;
        }
        else
        {
            if(damageFill.fillAmount > fillImage.fillAmount)
            {
                damageFill.fillAmount = Mathf.MoveTowards(damageFill.fillAmount, targetFillAmount, smoothSpeed * Time.deltaTime);
            }
        }
                
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        targetFillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        float healthRatioLost = 1 + (damageFill.fillAmount - targetFillAmount); 
        drainTimer = delayBeforeDrain;

        int currentInt = Mathf.CeilToInt(currentHealth);
        int maxInt = Mathf.CeilToInt(maxHealth);
        healthText.text = $"{currentInt}\n/\n{maxInt}";
        
        //This is from the tween library DOTween, just helps
        //with tweening, makes it a lot more convient. Killing
        // just makes sure prior animations don't overlap.
        healthText.transform.DOKill();
        healthText.DOKill();

        // This mess basically makes the health get big when you take damage, change to red, then get smaller and revert to white.
        healthText.transform.DOScale(healthRatioLost, textPulseDuration / 2f).SetLoops(2, LoopType.Yoyo);
        healthText.DOColor(damageColor, textPulseDuration / 2f)
            .OnComplete(() => healthText.DOColor(originalColor, textPulseDuration / 2f));
    }
}
