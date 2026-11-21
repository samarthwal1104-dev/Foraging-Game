using UnityEngine;
using UnityEngine.UI;

public class PenaltyUI : MonoBehaviour
{
    public static PenaltyUI Instance { get; private set; }

    [Header("Settings")]
    public float fadeSpeed = 5f;
    [Tooltip("How opaque the red gets (0 to 1)")]
    public float maxAlpha = 0.4f;

    private Image vignetteImage;
    private bool isPenalized = false;
    private Color targetColor;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        vignetteImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (vignetteImage != null)
        {
            targetColor = vignetteImage.color;
            targetColor.a = 0f;
            vignetteImage.color = targetColor;
        }
    }

    public void SetPenaltyState(bool state)
    {
        isPenalized = state;
    }

    private void Update()
    {
        if (vignetteImage == null) return;

        // Target alpha is maxAlpha if penalized, 0 if safe
        targetColor.a = isPenalized ? maxAlpha : 0f;
        vignetteImage.color = Color.Lerp(vignetteImage.color, targetColor, Time.deltaTime * fadeSpeed);
    }
}
