using UnityEngine;

/// <summary>
/// 現在のブロックの操作（移動、回転、落下、衝突判定）
/// </summary>
public class BlockController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private BlockSpawner spawner;
    [SerializeField] private GameObject blockPrefab;
    
    [Header("Settings")]
    [SerializeField] private float fallInterval = 1f;
    [SerializeField] private float moveInterval = 0.1f;
    
    private TetrominoData currentTetromino;
    private Vector2Int currentPosition;
    private float fallTimer;
    private float moveTimer;
    private bool isActive = false;
    private GameObject[] previewBlocks; // 現在のブロックのプレビュー表示用
    
    public bool IsActive => isActive;
    
    private void Awake()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
        }
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<BlockSpawner>();
        }
        // blockPrefabはInspectorで設定するか、Boardから取得
        if (blockPrefab == null && board != null)
        {
            // BoardコンポーネントからblockPrefabを取得する方法を試す
            var boardType = board.GetType();
            var blockPrefabField = boardType.GetField("blockPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (blockPrefabField != null)
            {
                blockPrefab = blockPrefabField.GetValue(board) as GameObject;
            }
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        HandleInput();
        HandleFall();
        UpdatePreview();
    }
    
    private void OnDestroy()
    {
        ClearPreview();
    }
    
    /// <summary>
    /// 新しいブロックを開始
    /// </summary>
    public void StartNewBlock(TetrominoData tetromino)
    {
        ClearPreview();
        
        currentTetromino = tetromino;
        currentPosition = spawner.SpawnPosition;
        fallTimer = 0f;
        moveTimer = 0f;
        isActive = true;
        
        if (!IsValidPosition())
        {
            // ゲームオーバー
            isActive = false;
            if (FindFirstObjectByType<GameManager>() != null)
            {
                FindFirstObjectByType<GameManager>().GameOver();
            }
        }
        else
        {
            CreatePreview();
        }
    }
    
    /// <summary>
    /// 入力処理
    /// </summary>
    private void HandleInput()
    {
        // 左右移動
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (moveTimer <= 0f)
            {
                TryMove(Vector2Int.left);
                moveTimer = moveInterval;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (moveTimer <= 0f)
            {
                TryMove(Vector2Int.right);
                moveTimer = moveInterval;
            }
        }
        else
        {
            moveTimer = 0f;
        }
        
        // 回転
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryRotate();
        }
        
        // 高速落下
        if (Input.GetKey(KeyCode.DownArrow))
        {
            fallTimer += Time.deltaTime * 10f; // 10倍速で落下
        }
        
        // ハードドロップ（即座に落下）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        
        moveTimer -= Time.deltaTime;
    }
    
    /// <summary>
    /// 落下処理
    /// </summary>
    private void HandleFall()
    {
        fallTimer += Time.deltaTime;
        
        if (fallTimer >= fallInterval)
        {
            if (!TryMove(Vector2Int.down))
            {
                // ブロックを固定
                LockBlock();
            }
            fallTimer = 0f;
        }
    }
    
    /// <summary>
    /// 移動を試みる
    /// </summary>
    private bool TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = currentPosition + direction;
        Vector2Int[] newPositions = GetWorldPositions(newPosition, currentTetromino.positions);
        
        if (board.IsValidPosition(newPositions))
        {
            currentPosition = newPosition;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 回転を試みる
    /// </summary>
    private void TryRotate()
    {
        if (currentTetromino == null) return;
        
        Vector2Int[] rotated = RotatePositions(currentTetromino.positions, currentTetromino.pivotIndex);
        Vector2Int[] worldPositions = GetWorldPositions(currentPosition, rotated);
        
        if (board.IsValidPosition(worldPositions))
        {
            currentTetromino.positions = rotated;
        }
    }
    
    /// <summary>
    /// 位置を90度回転
    /// </summary>
    private Vector2Int[] RotatePositions(Vector2Int[] positions, int pivotIndex)
    {
        if (positions.Length == 0) return positions;
        
        Vector2Int pivot = positions[pivotIndex];
        Vector2Int[] rotated = new Vector2Int[positions.Length];
        
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2Int relative = positions[i] - pivot;
            // 90度時計回りに回転: (x, y) -> (y, -x)
            Vector2Int rotatedRelative = new Vector2Int(relative.y, -relative.x);
            rotated[i] = pivot + rotatedRelative;
        }
        
        return rotated;
    }
    
    /// <summary>
    /// ハードドロップ（即座に最下部まで落下）
    /// </summary>
    private void HardDrop()
    {
        while (TryMove(Vector2Int.down))
        {
            // 落下し続ける
        }
        LockBlock();
    }
    
    /// <summary>
    /// ブロックをボードに固定
    /// </summary>
    private void LockBlock()
    {
        Vector2Int[] worldPositions = GetWorldPositions();
        board.PlaceBlocks(worldPositions, currentTetromino.color);
        isActive = false;
        ClearPreview();
        
        // ライン消去をチェック
        if (FindFirstObjectByType<GameManager>() != null)
        {
            FindFirstObjectByType<GameManager>().CheckLines();
        }
    }
    
    /// <summary>
    /// ワールド座標での位置を取得
    /// </summary>
    private Vector2Int[] GetWorldPositions()
    {
        return GetWorldPositions(currentPosition, currentTetromino.positions);
    }
    
    private Vector2Int[] GetWorldPositions(Vector2Int offset, Vector2Int[] relativePositions)
    {
        Vector2Int[] worldPositions = new Vector2Int[relativePositions.Length];
        for (int i = 0; i < relativePositions.Length; i++)
        {
            worldPositions[i] = offset + relativePositions[i];
        }
        return worldPositions;
    }
    
    /// <summary>
    /// 現在の位置が有効かチェック
    /// </summary>
    private bool IsValidPosition()
    {
        return board.IsValidPosition(GetWorldPositions());
    }
    
    /// <summary>
    /// 現在のブロックの位置を取得（デバッグ用）
    /// </summary>
    public Vector2Int[] GetCurrentWorldPositions()
    {
        if (!isActive) return new Vector2Int[0];
        return GetWorldPositions();
    }
    
    /// <summary>
    /// 現在のブロックの色を取得
    /// </summary>
    public Color GetCurrentColor()
    {
        if (currentTetromino == null) return Color.white;
        return currentTetromino.color;
    }
    
    /// <summary>
    /// プレビューブロックを作成
    /// </summary>
    private void CreatePreview()
    {
        if (currentTetromino == null || blockPrefab == null) return;
        
        ClearPreview();
        
        Vector2Int[] worldPositions = GetWorldPositions();
        previewBlocks = new GameObject[worldPositions.Length];
        
        for (int i = 0; i < worldPositions.Length; i++)
        {
            GameObject blockObj = Instantiate(blockPrefab, transform);
            blockObj.transform.position = new Vector3(worldPositions[i].x, worldPositions[i].y, 0);
            
            Block block = blockObj.GetComponent<Block>();
            if (block != null)
            {
                block.SetColor(currentTetromino.color);
            }
            else
            {
                SpriteRenderer sr = blockObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = currentTetromino.color;
                }
            }
            
            previewBlocks[i] = blockObj;
        }
    }
    
    /// <summary>
    /// プレビューブロックを更新
    /// </summary>
    private void UpdatePreview()
    {
        if (!isActive || currentTetromino == null) return;
        
        Vector2Int[] worldPositions = GetWorldPositions();
        
        if (previewBlocks == null || previewBlocks.Length != worldPositions.Length)
        {
            CreatePreview();
            return;
        }
        
        for (int i = 0; i < worldPositions.Length && i < previewBlocks.Length; i++)
        {
            if (previewBlocks[i] != null)
            {
                previewBlocks[i].transform.position = new Vector3(worldPositions[i].x, worldPositions[i].y, 0);
            }
        }
    }
    
    /// <summary>
    /// プレビューブロックをクリア
    /// </summary>
    private void ClearPreview()
    {
        if (previewBlocks != null)
        {
            foreach (var block in previewBlocks)
            {
                if (block != null)
                {
                    Destroy(block);
                }
            }
            previewBlocks = null;
        }
    }
}

