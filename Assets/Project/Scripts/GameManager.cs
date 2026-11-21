using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float trialTime = 120f;
    private float currentTime;
    public TextMeshProUGUI timerText;
    public bool trialRunning = false;

    void Start()
    {
       
        // StartTrial();  Removed
        Time.timeScale = 0f; // Pause at start for StartMenu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!trialRunning) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else if (currentTime > -1f) // ensure one-time execution
        {
            EndTrial();
        }
    }

    public void StartTrial()
    {
        trialRunning = true;
        currentTime = trialTime;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Apple.ClearActiveList();

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ResetScore();

        if (DataLogger.Instance != null)
            DataLogger.Instance.StartNewTrial();
    }

    void EndTrial()
    {
        MovementMap map = FindFirstObjectByType<MovementMap>();
        if (map != null)
        {
            map.SavePNG();
            Debug.Log(" Movement Map Saved as PNG");
        }
        trialRunning = false;
        currentTime = -1f;
        Debug.Log("Trial Over! Final Score: " + ResourceManager.Instance.currentScore);

        if (DataLogger.Instance != null)
            DataLogger.Instance.WriteDataToFile(ResourceManager.Instance.currentScore, trialTime);

        Time.timeScale = 0f;
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "Time: " + Mathf.Ceil(currentTime).ToString();
    }
}
