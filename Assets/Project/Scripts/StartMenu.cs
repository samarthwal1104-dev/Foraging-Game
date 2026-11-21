using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public Button lowNoiseButton;
    public Button highNoiseButton;
    public TextMeshProUGUI infoText;
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        // ✅ Pause game & show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        menuPanel.SetActive(true);

        // ✅ Disable MouseLook so it doesn't re-hide cursor
        MouseLook ml = FindFirstObjectByType<MouseLook>();
        if (ml != null) ml.enabled = false;

        // ✅ Assign buttons
        lowNoiseButton.onClick.AddListener(() => SelectCondition("Low"));
        highNoiseButton.onClick.AddListener(() => SelectCondition("High"));
    }

    void SelectCondition(string condition)
    {
        Debug.Log("Selected Condition: " + condition);

        // ✅ Apply noise condition to GameSettings
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.selectedCondition =
                (condition == "Low") ? GameSettings.NoiseCondition.Low : GameSettings.NoiseCondition.High;
        }

        // ✅ Hide menu
        menuPanel.SetActive(false);

        // ✅ Re-enable MouseLook before gameplay
        MouseLook ml = FindFirstObjectByType<MouseLook>();
        if (ml != null) ml.enabled = true;

        // ✅ Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ✅ Start trial
        gameManager.StartTrial();
    }
}
