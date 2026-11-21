using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public int currentScore { get; private set; } = 0;
    public int totalPositiveScore { get; private set; } = 0;
    public int totalNegativeScore { get; private set; } = 0;

    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        totalPositiveScore += amount;
        UpdateScoreUI();
    }

    public void RemoveScore(int amount)
    {
        currentScore -= amount;
        totalNegativeScore += amount;

        if (currentScore < 0)
            currentScore = 0;

        UpdateScoreUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        totalPositiveScore = 0;
        totalNegativeScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }
}
