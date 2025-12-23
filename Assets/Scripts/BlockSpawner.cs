using UnityEngine;

/// <summary>
/// テトリミノの生成とランダム選択
/// </summary>
public class BlockSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector2Int spawnPosition = new Vector2Int(4, 18);
    
    // テトリミノの形状定義（各形状は4つのブロック位置で構成）
    private static readonly Vector2Int[][] TETROMINO_SHAPES = new Vector2Int[][]
    {
        // I型
        new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) },
        // O型
        new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        // T型
        new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
        // S型
        new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        // Z型
        new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
        // J型
        new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
        // L型
        new Vector2Int[] { new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) }
    };
    
    // 各テトリミノの色
    private static readonly Color[] TETROMINO_COLORS = new Color[]
    {
        new Color(0f, 1f, 1f),      // I - シアン
        new Color(1f, 1f, 0f),      // O - イエロー
        new Color(1f, 0f, 1f),      // T - マゼンタ
        new Color(0f, 1f, 0f),      // S - グリーン
        new Color(1f, 0f, 0f),      // Z - レッド
        new Color(0f, 0f, 1f),      // J - ブルー
        new Color(1f, 0.5f, 0f)     // L - オレンジ
    };
    
    public Vector2Int SpawnPosition => spawnPosition;
    
    /// <summary>
    /// ランダムなテトリミノを生成
    /// </summary>
    public TetrominoData SpawnRandomTetromino()
    {
        int index = Random.Range(0, TETROMINO_SHAPES.Length);
        Vector2Int[] shape = new Vector2Int[TETROMINO_SHAPES[index].Length];
        
        // 形状をコピー
        for (int i = 0; i < TETROMINO_SHAPES[index].Length; i++)
        {
            shape[i] = TETROMINO_SHAPES[index][i];
        }
        
        return new TetrominoData
        {
            positions = shape,
            color = TETROMINO_COLORS[index],
            pivotIndex = 1 // 回転の中心点（大体中央）
        };
    }
}

/// <summary>
/// テトリミノのデータ構造
/// </summary>
public class TetrominoData
{
    public Vector2Int[] positions;
    public Color color;
    public int pivotIndex; // 回転の中心となるブロックのインデックス
}

