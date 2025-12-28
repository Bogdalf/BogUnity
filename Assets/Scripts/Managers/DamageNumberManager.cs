using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance;

    [Header("Prefab")]
    [SerializeField] private GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnDamageNumber(Vector3 position, float damage, bool isPlayerDamage = false)
    {
        if (damageNumberPrefab == null) return;

        // Spawn slightly above the hit position
        Vector3 spawnPos = position + Vector3.up * 0.5f;
        GameObject damageObj = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);

        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.SetDamage(damage, isPlayerDamage);
        }
    }
}