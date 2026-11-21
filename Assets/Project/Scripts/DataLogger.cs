// DataLogger.cs
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance { get; private set; }

    private List<string> eventLog;
    private float totalImmobilityTime = 0f;
    private int totalObstacleCollisions = 0;
    private float lastAppleCollectionTime = 0f;
    private Vector3 lastAppleCollectionPosition;
    private List<Vector3> pathSinceLastApple;
    private Vector3 lastPositionForImmobilityCheck;
    private float immobilityThreshold = 0.1f;

    // Variables for detailed crouch logging
    private bool isCurrentlyCrouchingForLog = false;
    private float crouchStartTime;
    private float crouchStartDroneDistance;

    // --- NEW: Apple Counters ---
    private int totalRedApples = 0;
    private int totalGoldenApples = 0;

    // --- NEW: Patch Tracker ---
    private HashSet<string> visitedPatches = new HashSet<string>();

    private string currentPatch = "";

    public void SetCurrentPatch(string patchName) { currentPatch = patchName; }

    public void ClearCurrentPatch(string patchName)
    {
        // Only clear if it matches, just in case zones overlap slightly
        if (currentPatch == patchName) currentPatch = "";
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartNewTrial()
    {
        eventLog = new List<string>();
        totalImmobilityTime = 0f;
        totalObstacleCollisions = 0;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            lastAppleCollectionTime = Time.time;
            lastAppleCollectionPosition = player.transform.position;
            lastPositionForImmobilityCheck = player.transform.position;
        }

        pathSinceLastApple = new List<Vector3>();
        isCurrentlyCrouchingForLog = false;

        // --- NEW: Reset Counters ---
        totalRedApples = 0;
        totalGoldenApples = 0;
        visitedPatches.Clear();
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        Vector3 currentPlayerPosition = player.transform.position;

        if (Vector3.Distance(currentPlayerPosition, lastPositionForImmobilityCheck) < immobilityThreshold * Time.fixedDeltaTime)
        {
            totalImmobilityTime += Time.fixedDeltaTime;
        }

        lastPositionForImmobilityCheck = currentPlayerPosition;
        pathSinceLastApple.Add(currentPlayerPosition);
    }

    public void LogObstacleCollision()
    {
        totalObstacleCollisions++;
        string row = string.Join(",",
            Time.time.ToString("F2"),       // Timestamp
            "ObstacleCollision",            // EventType
            "",                             // ITT (blank)
            "",                             // ITD (blank)
            "",                             // ARS_Index (blank)
            "",                             // CrouchDuration (blank)
            "",                             // CrouchStartDistance (blank)
            "",                             // CrouchEndDistance (blank)
            "",                             // AppleDroneDistance (blank)
            "",                             // AppleType (blank)
            totalObstacleCollisions.ToString(), // ObstacleCollisionCount
            ""
        );
        eventLog.Add(row);
    }

    // --- MODIFIED METHOD ---
    public void LogAppleCollected(Vector3 collectionPosition, Apple appleComponent)
    {
        float currentTime = Time.time;
        float itt = currentTime - lastAppleCollectionTime;
        float itd = Vector3.Distance(collectionPosition, lastAppleCollectionPosition);
        float ars = CalculateARS();

        float droneDistance = -1f;
        DroneAI drone = FindFirstObjectByType<DroneAI>();
        if (drone != null)
        {
            droneDistance = Vector3.Distance(collectionPosition, drone.transform.position);
        }

        string appleType = (appleComponent != null) ? appleComponent.fruitType : "Unknown";

        // --- NEW: Increment Counters ---
        if (appleType == "Red")
        {
            totalRedApples++;
        }
        else if (appleType == "Golden")
        {
            totalGoldenApples++;
        }

        string row = string.Join(",",
            currentTime.ToString("F2"),     // Timestamp
            "AppleCollected",               // EventType
            itt.ToString("F2"),             // ITT
            itd.ToString("F2"),             // ITD
            ars.ToString("F3"),             // ARS_Index
            "",                             // CrouchDuration (blank)
            "",                             // CrouchStartDistance (blank)
            "",                             // CrouchEndDistance (blank)
            droneDistance.ToString("F2"),   // AppleDroneDistance
            appleType,                      // AppleType
            "",                             // ObstacleCollisionCount (blank)
            currentPatch
        );
        eventLog.Add(row);

        lastAppleCollectionTime = currentTime;
        lastAppleCollectionPosition = collectionPosition;
        pathSinceLastApple.Clear();
    }

    public void LogCrouchStart()
    {
        if (isCurrentlyCrouchingForLog) return;
        isCurrentlyCrouchingForLog = true;

        crouchStartTime = Time.time;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        DroneAI drone = FindFirstObjectByType<DroneAI>();
        if (player != null && drone != null)
        {
            crouchStartDroneDistance = Vector3.Distance(player.transform.position, drone.transform.position);
        }
    }

    public void LogCrouchEnd()
    {
        if (!isCurrentlyCrouchingForLog) return;
        isCurrentlyCrouchingForLog = false;

        float crouchDuration = Time.time - crouchStartTime;
        float crouchEndDroneDistance = -1f;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        DroneAI drone = FindFirstObjectByType<DroneAI>();
        if (player != null && drone != null)
        {
            crouchEndDroneDistance = Vector3.Distance(player.transform.position, drone.transform.position);
        }

        string row = string.Join(",",
            Time.time.ToString("F2"),
            "CrouchEvent",
            "", "", "", // Apple columns (blank)
            crouchDuration.ToString("F2"),
            crouchStartDroneDistance.ToString("F2"),
            crouchEndDroneDistance.ToString("F2"),
            "", "",      // Apple... (blank)
            "",           // ObstacleCollisionCount (blank)
            ""
        );
        eventLog.Add(row);
    }

    // --- MODIFIED METHOD ---
    public void WriteDataToFile(int finalScore, float trialDuration)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("--- Summary Metrics ---");
        sb.AppendLine($"Trial Duration,{trialDuration:F2}");
        float rewardRate = (trialDuration > 0) ? finalScore / trialDuration : 0;
        sb.AppendLine($"Reward Rate,{rewardRate:F3}");
        sb.AppendLine($"Total Positive Score,{ResourceManager.Instance.totalPositiveScore}");
        sb.AppendLine($"Total Negative Score,{ResourceManager.Instance.totalNegativeScore}");
        sb.AppendLine($"Total Obstacle Collisions,{totalObstacleCollisions}");
        sb.AppendLine($"Total Immobility Time,{totalImmobilityTime:F2}");

        // --- NEW: Added Apple Counts ---
        sb.AppendLine($"Total Red Apples,{totalRedApples}");
        sb.AppendLine($"Total Golden Apples,{totalGoldenApples}");

        // --- ADD THIS LINE TO THE SUMMARY ---
        sb.AppendLine($"Total Unique Patches Visited,{visitedPatches.Count}");

        sb.AppendLine();

        sb.AppendLine("--- Event Log ---");

        string header = string.Join(",",
            "Timestamp",
            "EventType",
            "ITT",
            "ITD",
            "ARS_Index",
            "CrouchDuration",
            "CrouchStartDistance",
            "CrouchEndDistance",
            "AppleDroneDistance",
            "AppleType",
            "ObstacleCollisionCount",
            "CurrentPatch"
        );
        sb.AppendLine(header);

        foreach (string row in eventLog)
        {
            sb.AppendLine(row);
        }

        string directoryPath = Path.Combine(Application.dataPath, "Project/OutputFiles");
        string fileName = $"TrialData_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        string filePath = Path.Combine(directoryPath, fileName);

        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"Data saved to: {filePath}");
    }

    private float CalculateARS()
    {
        if (pathSinceLastApple.Count < 2) return 1.0f;

        float pathLength = 0f;
        for (int i = 0; i < pathSinceLastApple.Count - 1; i++)
        {
            pathLength += Vector3.Distance(pathSinceLastApple[i], pathSinceLastApple[i + 1]);
        }

        if (pathLength < 0.1f) return 1.0f;

        float beelineDistance = Vector3.Distance(
            pathSinceLastApple[0],
            pathSinceLastApple[pathSinceLastApple.Count - 1]
        );

        return beelineDistance / pathLength;
    }

    public void LogPatchVisited(string patchName)
    {
        if (!visitedPatches.Contains(patchName))
        {
            visitedPatches.Add(patchName);

            // Optionally add it to the timeline event log as well
            string row = string.Join(",",
            Time.time.ToString("F2"),         // Timestamp
            $"PatchVisited_{patchName}",      // EventType
            "", "", "", "", "", "", "", "", "", "" // Empty columns
            );
            eventLog.Add(row);
        }
    }
}

    