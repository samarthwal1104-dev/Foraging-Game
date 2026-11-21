using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class PositionLogger : MonoBehaviour
{
    [Header("Targets (assign)")]
    public Transform playerTransform;
    public Transform droneTransform;

    [Header("Sampling")]
    public float sampleInterval = 1f;

    private float sampleTimer = 0f;
    private List<string> csvRows = new List<string>();

    private MovementMap movementMap;

    private void Awake()
    {
        movementMap = FindFirstObjectByType<MovementMap>();
    }

    private void Start()
    {
        sampleTimer = 0f;
        csvRows.Clear();
        csvRows.Add("Timestamp,Seconds,Entity,X,Y,Z");
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer -= sampleInterval;
            Sample();
        }
    }

    private void Sample()
    {
        float t = Time.time;

        if (playerTransform != null)
        {
            Vector3 p = playerTransform.position;
            csvRows.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{t:F2},Player,{p.x:F3},{p.y:F3},{p.z:F3}");
            if (movementMap != null) movementMap.AddPoint(MovementMap.Entity.Player, p);
        }

        if (droneTransform != null)
        {
            Vector3 d = droneTransform.position;
            csvRows.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{t:F2},Drone,{d.x:F3},{d.y:F3},{d.z:F3}");
            if (movementMap != null) movementMap.AddPoint(MovementMap.Entity.Drone, d);
        }
    }

    public void SaveLogToFile(string extraTag = "")
    {
        try
        {
            string directoryPath = Path.Combine(Application.dataPath, "Project/OutputFiles");
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            string fileName = $"PositionLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{(string.IsNullOrEmpty(extraTag) ? "" : "_" + extraTag)}.csv";
            string filePath = Path.Combine(directoryPath, fileName);

            File.WriteAllLines(filePath, csvRows, Encoding.UTF8);
            Debug.Log($"PositionLogger: saved {csvRows.Count} rows to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"PositionLogger Save failed: {ex.Message}");
        }
    }
}
