using UnityEngine;

/// <summary>
/// 個別のブロックセルのデータと表示を管理するクラス
/// </summary>
public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    public void SetColor(Color color)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.color = color;
    }
    
    public Color GetColor()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        return spriteRenderer.color;
    }
}

