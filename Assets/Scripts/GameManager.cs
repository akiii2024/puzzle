using UnityEngine;

/// <summary>
/// ゲーム全体の管理、スコア計算、ゲームオーバー判定
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private BlockSpawner spawner;
    [SerializeField] private BlockController blockController;
    [SerializeField] private UIManager uiManager;
    
    [Header("Game Settings")]
    [SerializeField] private float spawnDelay = 0.5f;
    
    private int score = 0;
    private int linesCleared = 0;
    private bool isGameOver = false;
    private float spawnTimer = 0f;
    
    // スコア計算（1ライン: 100点、2ライン: 300点、3ライン: 500点、4ライン: 800点）
    private readonly int[] SCORE_VALUES = { 0, 100, 300, 500, 800 };
    
    private void Awake()
    {
        // 参照が設定されていない場合は自動検索
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
        }
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<BlockSpawner>();
        }
        if (blockController == null)
        {
            blockController = FindFirstObjectByType<BlockController>();
        }
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }
    }
    
    private void Start()
    {
        StartGame();
    }
    
    private void Update()
    {
        if (isGameOver) return;
        
        // 新しいブロックを生成
        if (!blockController.IsActive)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnDelay)
            {
                SpawnNextBlock();
                spawnTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// ゲーム開始
    /// </summary>
    public void StartGame()
    {
        isGameOver = false;
        score = 0;
        linesCleared = 0;
        spawnTimer = 0f;
        
        if (board != null)
        {
            board.ClearBoard();
        }
        
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.UpdateLines(linesCleared);
            uiManager.HideGameOver();
        }
        
        SpawnNextBlock();
    }
    
    /// <summary>
    /// 次のブロックを生成
    /// </summary>
    private void SpawnNextBlock()
    {
        if (spawner == null || blockController == null) return;
        
        TetrominoData newTetromino = spawner.SpawnRandomTetromino();
        blockController.StartNewBlock(newTetromino);
    }
    
    /// <summary>
    /// ライン消去をチェック
    /// </summary>
    public void CheckLines()
    {
        if (board == null || isGameOver) return;
        
        int clearedLines = board.ClearLines();
        
        if (clearedLines > 0)
        {
            linesCleared += clearedLines;
            AddScore(clearedLines);
            
            if (uiManager != null)
            {
                uiManager.UpdateScore(score);
                uiManager.UpdateLines(linesCleared);
            }
        }
        
        // ゲームオーバー判定
        if (board.IsGameOver())
        {
            GameOver();
        }
    }
    
    /// <summary>
    /// スコアを加算
    /// </summary>
    private void AddScore(int lines)
    {
        if (lines > 0 && lines < SCORE_VALUES.Length)
        {
            score += SCORE_VALUES[lines];
        }
    }
    
    /// <summary>
    /// ゲームオーバー
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;
        
        if (uiManager != null)
        {
            uiManager.ShowGameOver(score, linesCleared);
        }
    }
    
    /// <summary>
    /// ゲームをリスタート
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }
}

