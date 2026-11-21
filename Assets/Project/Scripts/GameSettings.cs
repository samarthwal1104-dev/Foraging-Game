using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public enum NoiseCondition { Low, High }

    [Header("Experiment Condition")]
    public NoiseCondition selectedCondition = NoiseCondition.High;

    [Tooltip("Volume multiplier for Low noise condition")]
    public float lowNoiseMultiplier = 0.35f;

    [Tooltip("Volume multiplier for High noise condition")]
    public float highNoiseMultiplier = 1.0f;

    [Header("Timbre (Filter Cutoff)")]
    [Tooltip("Cutoff frequency for Low noise (e.g., 800)")]
    public float lowNoiseCutoff = 800f;
    [Tooltip("Cutoff frequency for High noise (e.g., 22000)")]
    public float highNoiseCutoff = 22000f;

    public float NoiseMultiplier => selectedCondition == NoiseCondition.Low ? lowNoiseMultiplier : highNoiseMultiplier;
    public float NoiseCutoffFrequency => selectedCondition == NoiseCondition.Low ? lowNoiseCutoff : highNoiseCutoff;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
