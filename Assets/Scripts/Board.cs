using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲームボード（グリッド）の管理、ブロック配置、ライン消去ロジック
/// </summary>
public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    [SerializeField] private Transform boardParent;
    [SerializeField] private GameObject blockPrefab;
    
    private Block[,] grid;
    private Color[,] colorGrid;
    
    public int Width => width;
    public int Height => height;
    
    private void Awake()
    {
        grid = new Block[height, width];
        colorGrid = new Color[height, width];
        
        if (boardParent == null)
        {
            boardParent = transform;
        }
    }
    
    /// <summary>
    /// 指定位置が有効かどうかをチェック
    /// </summary>
    public bool IsValidPosition(Vector2Int[] positions)
    {
        foreach (var pos in positions)
        {
            if (pos.x < 0 || pos.x >= width || pos.y < 0)
            {
                return false;
            }
            
            if (pos.y < height && grid[pos.y, pos.x] != null)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// ブロックをボードに配置
    /// </summary>
    public void PlaceBlocks(Vector2Int[] positions, Color color)
    {
        foreach (var pos in positions)
        {
            if (pos.y >= height) continue;
            
            if (grid[pos.y, pos.x] == null && blockPrefab != null)
            {
                GameObject blockObj = Instantiate(blockPrefab, boardParent);
                blockObj.transform.position = new Vector3(pos.x, pos.y, 0);
                Block block = blockObj.GetComponent<Block>();
                if (block == null)
                {
                    block = blockObj.AddComponent<Block>();
                }
                block.SetColor(color);
                grid[pos.y, pos.x] = block;
                colorGrid[pos.y, pos.x] = color;
            }
        }
    }
    
    /// <summary>
    /// ラインが揃っているかチェックし、消去する
    /// </summary>
    public int ClearLines()
    {
        List<int> linesToClear = new List<int>();
        
        // 消去するラインを検出
        for (int y = 0; y < height; y++)
        {
            bool isFull = true;
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] == null)
                {
                    isFull = false;
                    break;
                }
            }
            
            if (isFull)
            {
                linesToClear.Add(y);
            }
        }
        
        // ラインを消去
        foreach (int line in linesToClear)
        {
            ClearLine(line);
        }
        
        // ブロックを下に落とす
        if (linesToClear.Count > 0)
        {
            DropBlocks(linesToClear);
        }
        
        return linesToClear.Count;
    }
    
    /// <summary>
    /// 指定されたラインを消去
    /// </summary>
    private void ClearLine(int line)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[line, x] != null)
            {
                Destroy(grid[line, x].gameObject);
                grid[line, x] = null;
            }
        }
    }
    
    /// <summary>
    /// 消去されたラインの上にあるブロックを下に落とす
    /// </summary>
    private void DropBlocks(List<int> clearedLines)
    {
        clearedLines.Sort();
        
        foreach (int clearedLine in clearedLines)
        {
            // 消去されたラインより上にあるブロックを1つ下に移動
            for (int y = clearedLine + 1; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[y, x] != null)
                    {
                        grid[y - 1, x] = grid[y, x];
                        grid[y, x] = null;
                        colorGrid[y - 1, x] = colorGrid[y, x];
                        
                        if (grid[y - 1, x] != null)
                        {
                            grid[y - 1, x].transform.position = new Vector3(x, y - 1, 0);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// ゲームオーバー判定（最上部にブロックがあるか）
    /// </summary>
    public bool IsGameOver()
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[height - 1, x] != null)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// ボードをクリア（リスタート用）
    /// </summary>
    public void ClearBoard()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] != null)
                {
                    Destroy(grid[y, x].gameObject);
                    grid[y, x] = null;
                }
            }
        }
    }
}

