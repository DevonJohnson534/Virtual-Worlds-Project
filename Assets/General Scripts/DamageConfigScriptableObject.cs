using UnityEngine;
using static UnityEngine.ParticleSystem;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Damage config", order = 1)]
public class DamageConfigScriptableObject : ScriptableObject
{
    // This allows us to test with constant damage, or with fall off.
    public MinMaxCurve DamageCurve;

    private void Reset()
    {

        DamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int GetDamage(float distance = 0)
    {
        return Mathf.CeilToInt(DamageCurve.Evaluate(distance, Random.value));
    }
}
