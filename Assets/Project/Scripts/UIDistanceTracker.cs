using UnityEngine;
using TMPro;

public class UIDistanceTracker : MonoBehaviour
{
    [Header("UI Targets (Assign)")]
    public TextMeshProUGUI redAppleText;
    public TextMeshProUGUI goldenAppleText;

    private Transform playerTransform;

    void Start()
    {
        // Find the player's transform once
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Initialize text
        if (redAppleText != null) redAppleText.text = "Red: -";
        if (goldenAppleText != null) goldenAppleText.text = "Golden: -";
    }

    void Update()
    {
        // Only run if the player exists and the game is running
        if (playerTransform == null || Time.timeScale == 0f) return;

        float nearestRedDist = float.MaxValue;
        float nearestGoldenDist = float.MaxValue;
        Vector3 playerPos = playerTransform.position;

        // Use the static list from the Apple script
        foreach (Apple apple in Apple.ActiveApples)
        {
            if (apple == null) continue; // Safety check

            float dist = Vector3.Distance(playerPos, apple.transform.position);

            if (apple.fruitType == "Red")
            {
                if (dist < nearestRedDist) nearestRedDist = dist;
            }
            else if (apple.fruitType == "Golden")
            {
                if (dist < nearestGoldenDist) nearestGoldenDist = dist;
            }
        }

        // Update the UI text
        if (redAppleText != null)
        {
            redAppleText.text = (nearestRedDist == float.MaxValue)
                ? "Red: -"
                : "Red: " + nearestRedDist.ToString("F1") + "m";
        }

        if (goldenAppleText != null)
        {
            goldenAppleText.text = (nearestGoldenDist == float.MaxValue)
                ? "Golden: -"
                : "Golden: " + nearestGoldenDist.ToString("F1") + "m";
        }
    }
}