using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Conductor for the full boss intermission sequence.
/// 
/// Flow:
///   1. Boss1 "dies" → TriggerIntermission()
///   2. Wave 1 starts (meteors + spirits) in Zone 1
///   3. Wave 1 ends → TriggerRune1 enabled
///   4. Player charges TriggerRune1 → Wave 2 starts in Zone 2
///   5. Wave 2 ends → TriggerRune2 enabled
///   6. Player charges TriggerRune2 → Boss2 spawns with health bonus
///
/// Setup:
///   - Assign all references in Inspector
///   - TriggerRunes start with their Collider2D disabled
///   - Boss2 starts inactive (SetActive false)
/// </summary>
public class BossIntermissionManager : MonoBehaviour
{
    public static BossIntermissionManager Instance { get; private set; }
    
    [Header("Wave 1")]
    [SerializeField] private SpiritWave            wave1;
    [SerializeField] private List<IntermissionMeteorZone> meteorZones1 = new List<IntermissionMeteorZone>();
    [SerializeField] private TriggerRune            triggerRune1;

    [Header("Wave 2")]
    [SerializeField] private SpiritWave            wave2;
    [SerializeField] private List<IntermissionMeteorZone> meteorZones2 = new List<IntermissionMeteorZone>();
    [SerializeField] private TriggerRune            triggerRune2;

    [Header("Boss 2")]
    [SerializeField] private GameObject boss2GameObject;
    [SerializeField] private Transform  boss2SpawnPoint;

    [Header("Entry Trigger")]
    [SerializeField] private Collider2D entryTrigger; // Player walks into this to start intermission
    [SerializeField] private GameObject Light;

    private enum IntermissionPhase
    {
        Inactive,
        Wave1Active,
        Wave1Complete,
        Wave2Active,
        Wave2Complete,
        SpawningBoss2
    }

    private IntermissionPhase currentPhase = IntermissionPhase.Inactive;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Runes start disabled
        SetRuneInteractable(triggerRune1, false);
        SetRuneInteractable(triggerRune2, false);
        if (Light != null)
            Light.SetActive(false);
        // Boss2 starts hidden
        if (boss2GameObject != null)
            boss2GameObject.SetActive(false);
    }

    // ─── Entry Trigger ────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (currentPhase != IntermissionPhase.Inactive) return;

        TriggerIntermission();
    }

    // ─── Start Intermission ───────────────────────────────────────────────────────

    /// <summary>
    /// Called when player walks into the entry trigger OR directly by Boss1's death.
    /// </summary>
    public void TriggerIntermission()
    {
        if (currentPhase != IntermissionPhase.Inactive) return;

        // Reset statue count for fresh run
        SpiritStatue.ResetAll();

        Debug.Log("Intermission started!");
        Light.SetActive(true);
        
        StartWave1();
    }

    void StartMeteorZones(List<IntermissionMeteorZone> zones)
    {
        foreach (var zone in zones)
            zone?.StartZone();
    }

    void StopMeteorZones(List<IntermissionMeteorZone> zones)
    {
        foreach (var zone in zones)
            zone?.StopZone();
    }
    // ─── Wave 1 ───────────────────────────────────────────────────────────────────

    void StartWave1()
    {
        currentPhase = IntermissionPhase.Wave1Active;
        StartCoroutine(WaveDelay());
    }
    
    IEnumerator WaveDelay()
    {

        yield return new WaitForSeconds(3f);
        if (meteorZones1 != null) StartMeteorZones(meteorZones1);

        if (wave1 != null)
        {
            wave1.OnWaveComplete += OnWave1Complete;
            wave1.StartWave();
        }
        else
        {
            Debug.LogWarning("BossIntermissionManager: wave1 not assigned!");
        }
    }
    

    void OnWave1Complete()
    {
        currentPhase = IntermissionPhase.Wave1Complete;

        if (meteorZones1 != null) StopMeteorZones(meteorZones1);

        Debug.Log("Wave 1 complete — enabling TriggerRune1.");

        // Small dramatic pause before enabling rune
        StartCoroutine(EnableRuneDelayed(triggerRune1, 1f));
    }

    // ─── Wave 2 ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by TriggerRune1's RunePathSequence.onSequenceComplete event.
    /// </summary>
    public void StartWave2()
    {
        if (currentPhase != IntermissionPhase.Wave1Complete) return;

        currentPhase = IntermissionPhase.Wave2Active;

        if (meteorZones2 != null) StartMeteorZones(meteorZones2);

        if (wave2 != null)
        {
            wave2.OnWaveComplete += OnWave2Complete;
            wave2.StartWave();
        }
    }

    void OnWave2Complete()
    {
        currentPhase = IntermissionPhase.Wave2Complete;

        if (meteorZones2 != null) StopMeteorZones(meteorZones2);

        Debug.Log("Wave 2 complete — enabling TriggerRune2.");

        StartCoroutine(EnableRuneDelayed(triggerRune2, 1f));
    }

    // ─── Boss 2 Spawn ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by TriggerRune2's RunePathSequence.onSequenceComplete event.
    /// </summary>
    public void SpawnBoss2()
    {
        if (currentPhase != IntermissionPhase.Wave2Complete) return;

        currentPhase = IntermissionPhase.SpawningBoss2;

        Debug.Log($"Spawning Boss2. Health bonus: +{SpiritStatue.TotalHealthBonus * 100f}%");

        StartCoroutine(SpawnBoss2Sequence());
    }

    IEnumerator SpawnBoss2Sequence()
    {
        // Dramatic pause
        yield return new WaitForSeconds(1.5f);

        if (boss2GameObject == null)
        {
            Debug.LogWarning("BossIntermissionManager: boss2GameObject not assigned!");
            yield break;
        }

        // Move to spawn point if assigned
        if (boss2SpawnPoint != null)
            boss2GameObject.transform.position = boss2SpawnPoint.position;

        // Apply health bonus from spirits that reached statues
        BossBase boss2 = boss2GameObject.GetComponent<BossBase>();
        if (boss2 != null)
        {
            float baseHealth = boss2.GetMaxHealth();
            float newHealth  = baseHealth * (1f + SpiritStatue.TotalHealthBonus);
            boss2.SetMaxHealth(newHealth);
            Debug.Log($"Boss2 health set to {newHealth} (+{SpiritStatue.TotalHealthBonus * 100f}% from {SpiritStatue.EmpoweredCount} statues)");
        }

        // Activate and trigger entrance
        boss2GameObject.SetActive(true);
        boss2?.TriggerEntrance();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    IEnumerator EnableRuneDelayed(TriggerRune rune, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetRuneInteractable(rune, true);
    }

    void SetRuneInteractable(TriggerRune rune, bool interactable)
    {
        if (rune == null) return;
        Collider2D col = rune.GetComponent<Collider2D>();
        if (col != null) col.enabled = interactable;
    }
}