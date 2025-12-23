using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI表示（スコア、ゲームオーバー）
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI linesText;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalLinesText;
    [SerializeField] private Button restartButton;
    
    private GameManager gameManager;
    
    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// スコアを更新
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    /// <summary>
    /// ライン数を更新
    /// </summary>
    public void UpdateLines(int lines)
    {
        if (linesText != null)
        {
            linesText.text = $"Lines: {lines}";
        }
    }
    
    /// <summary>
    /// ゲームオーバー画面を表示
    /// </summary>
    public void ShowGameOver(int finalScore, int finalLines)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
        
        if (finalLinesText != null)
        {
            finalLinesText.text = $"Lines Cleared: {finalLines}";
        }
    }
    
    /// <summary>
    /// ゲームオーバー画面を非表示
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// リスタートボタンがクリックされた
    /// </summary>
    private void OnRestartClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
}

