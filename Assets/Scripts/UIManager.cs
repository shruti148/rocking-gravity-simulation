using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TMP_Text distanceText; 

    [Header("Game Settings")]
    public float targetDistance = 500f; // 设置胜利需要达到的距离

    private bool gameStarted = false;

    void Start()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        Time.timeScale = 0f; // 暂停游戏
    }

    public void StartGame()
    {
        if (startPanel != null) startPanel.SetActive(false);
        gameStarted = true;
        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool HasGameStarted()
    {
        return gameStarted;
    }

    public void UpdateHealth(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    public void UpdateDistance(float currentDistance)
    {
        if (distanceText != null)
            distanceText.text = $"Distance:\n {currentDistance:F1} / {targetDistance:F1} ";
    }
}
