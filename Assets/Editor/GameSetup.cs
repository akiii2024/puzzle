using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ゲームのセットアップを一括で行うエディタスクリプト
/// </summary>
public class GameSetup
{
    [MenuItem("Tools/Setup Puzzle Game")]
    public static void SetupGame()
    {
        // 確認ダイアログ
        if (!EditorUtility.DisplayDialog("ゲームセットアップ", 
            "現在のシーンにゲームオブジェクトを自動生成します。\n既存のオブジェクトは上書きされません。", 
            "実行", "キャンセル"))
        {
            return;
        }
        
        SetupBlockPrefab();
        SetupGameObjects();
        SetupUI();
        SetupCamera();
        
        EditorUtility.DisplayDialog("セットアップ完了", 
            "ゲームのセットアップが完了しました！\n\n" +
            "BlockプレハブにSpriteを自動設定しました。\n" +
            "必要に応じて各コンポーネントのパラメータを調整してください。", 
            "OK");
    }
    
    /// <summary>
    /// ブロックプレハブの設定
    /// </summary>
    private static void SetupBlockPrefab()
    {
        string prefabPath = "Assets/Prefabs/Block.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        // スプライトアセットを作成または取得
        Sprite blockSprite = CreateOrLoadBlockSprite();
        
        if (prefab == null)
        {
            // プレハブが存在しない場合は作成
            GameObject blockObj = new GameObject("Block");
            SpriteRenderer sr = blockObj.AddComponent<SpriteRenderer>();
            blockObj.AddComponent<Block>();
            
            // スプライトを設定
            sr.sprite = blockSprite;
            sr.color = Color.white;
            
            // プレハブとして保存
            string prefabDir = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            prefab = PrefabUtility.SaveAsPrefabAsset(blockObj, prefabPath);
            Object.DestroyImmediate(blockObj);
            
            Debug.Log("Blockプレハブを作成しました: " + prefabPath);
        }
        else
        {
            // 既存のプレハブにBlockコンポーネントがあるか確認
            Block block = prefab.GetComponent<Block>();
            if (block == null)
            {
                prefab.AddComponent<Block>();
            }
            
            // スプライトを設定
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null && blockSprite != null)
            {
                sr.sprite = blockSprite;
                EditorUtility.SetDirty(prefab);
            }
            
            AssetDatabase.SaveAssets();
        }
    }
    
    /// <summary>
    /// ブロック用のスプライトアセットを作成または取得
    /// </summary>
    private static Sprite CreateOrLoadBlockSprite()
    {
        string spritePath = "Assets/Sprites/BlockSprite.asset";
        
        // 既にスプライトアセットが存在する場合は読み込む
        Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (existingSprite != null)
        {
            return existingSprite;
        }
        
        // Spritesフォルダが存在しない場合は作成
        string spritesDir = "Assets/Sprites";
        if (!AssetDatabase.IsValidFolder(spritesDir))
        {
            AssetDatabase.CreateFolder("Assets", "Sprites");
        }
        
        // テクスチャを作成（白い四角形に枠線を追加）
        int size = 64; // スプライトのサイズ
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        // 白い背景に黒い枠線を描画
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                // 枠線（2ピクセル幅）
                if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                {
                    pixels[index] = new Color(0.2f, 0.2f, 0.2f, 1f); // 濃いグレーの枠線
                }
                else
                {
                    pixels[index] = Color.white; // 白い背景
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // スプライトを作成
        Sprite sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, size, size), 
            new Vector2(0.5f, 0.5f), // ピボットを中央に
            size // pixelsPerUnit
        );
        
        // スプライトをアセットとして保存
        AssetDatabase.CreateAsset(sprite, spritePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Block用のスプライトを作成しました: " + spritePath);
        
        return sprite;
    }
    
    /// <summary>
    /// ゲームオブジェクトのセットアップ
    /// </summary>
    private static void SetupGameObjects()
    {
        // GameManager
        GameObject gameManagerObj = FindOrCreateGameObject("GameManager");
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerObj.AddComponent<GameManager>();
        }
        
        // Board
        GameObject boardObj = FindOrCreateGameObject("Board");
        Board board = boardObj.GetComponent<Board>();
        if (board == null)
        {
            board = boardObj.AddComponent<Board>();
        }
        
        // Boardの設定
        GameObject blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Block.prefab");
        if (blockPrefab != null)
        {
            SerializedObject boardSO = new SerializedObject(board);
            boardSO.FindProperty("blockPrefab").objectReferenceValue = blockPrefab;
            boardSO.FindProperty("boardParent").objectReferenceValue = boardObj.transform;
            boardSO.ApplyModifiedProperties();
        }
        
        // BlockSpawner
        GameObject spawnerObj = FindOrCreateGameObject("BlockSpawner");
        BlockSpawner spawner = spawnerObj.GetComponent<BlockSpawner>();
        if (spawner == null)
        {
            spawner = spawnerObj.AddComponent<BlockSpawner>();
        }
        
        // BlockController
        GameObject controllerObj = FindOrCreateGameObject("BlockController");
        BlockController controller = controllerObj.GetComponent<BlockController>();
        if (controller == null)
        {
            controller = controllerObj.AddComponent<BlockController>();
        }
        
        // 参照の設定（リフレクションを使用）
        SetComponentReference(gameManager, "board", board);
        SetComponentReference(gameManager, "spawner", spawner);
        SetComponentReference(gameManager, "blockController", controller);
        
        SetComponentReference(controller, "board", board);
        SetComponentReference(controller, "spawner", spawner);
        SetComponentReference(controller, "blockPrefab", blockPrefab);
        
        Debug.Log("ゲームオブジェクトのセットアップが完了しました");
    }
    
    /// <summary>
    /// UIのセットアップ
    /// </summary>
    private static void SetupUI()
    {
        // Canvasの作成
        GameObject canvasObj = FindOrCreateGameObject("Canvas");
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // EventSystemの作成
        GameObject eventSystemObj = FindOrCreateGameObject("EventSystem");
        if (eventSystemObj.GetComponent<UnityEngine.EventSystems.EventSystem>() == null)
        {
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // UIManager
        GameObject uiManagerObj = FindOrCreateGameObject("UIManager");
        UIManager uiManager = uiManagerObj.GetComponent<UIManager>();
        if (uiManager == null)
        {
            uiManager = uiManagerObj.AddComponent<UIManager>();
        }
        uiManagerObj.transform.SetParent(canvasObj.transform, false);
        
        // スコア表示用のText
        GameObject scoreTextObj = FindOrCreateGameObject("ScoreText", canvasObj.transform);
        TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
        {
            scoreText = scoreTextObj.AddComponent<TextMeshProUGUI>();
        }
        scoreText.text = "Score: 0";
        scoreText.fontSize = 24;
        scoreText.color = Color.white;
        RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(10, -30);
        scoreRect.sizeDelta = new Vector2(200, 30);
        
        // ライン数表示用のText
        GameObject linesTextObj = FindOrCreateGameObject("LinesText", canvasObj.transform);
        TextMeshProUGUI linesText = linesTextObj.GetComponent<TextMeshProUGUI>();
        if (linesText == null)
        {
            linesText = linesTextObj.AddComponent<TextMeshProUGUI>();
        }
        linesText.text = "Lines: 0";
        linesText.fontSize = 24;
        linesText.color = Color.white;
        RectTransform linesRect = linesTextObj.GetComponent<RectTransform>();
        linesRect.anchorMin = new Vector2(0, 1);
        linesRect.anchorMax = new Vector2(0, 1);
        linesRect.anchoredPosition = new Vector2(10, -70);
        linesRect.sizeDelta = new Vector2(200, 30);
        
        // ゲームオーバーパネル
        GameObject gameOverPanelObj = FindOrCreateGameObject("GameOverPanel", canvasObj.transform);
        Image panelImage = gameOverPanelObj.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = gameOverPanelObj.AddComponent<Image>();
        }
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = gameOverPanelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        gameOverPanelObj.SetActive(false);
        
        // 最終スコア表示
        GameObject finalScoreTextObj = FindOrCreateGameObject("FinalScoreText", gameOverPanelObj.transform);
        TextMeshProUGUI finalScoreText = finalScoreTextObj.GetComponent<TextMeshProUGUI>();
        if (finalScoreText == null)
        {
            finalScoreText = finalScoreTextObj.AddComponent<TextMeshProUGUI>();
        }
        finalScoreText.text = "Final Score: 0";
        finalScoreText.fontSize = 36;
        finalScoreText.color = Color.white;
        finalScoreText.alignment = TextAlignmentOptions.Center;
        RectTransform finalScoreRect = finalScoreTextObj.GetComponent<RectTransform>();
        finalScoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalScoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalScoreRect.anchoredPosition = new Vector2(0, 50);
        finalScoreRect.sizeDelta = new Vector2(400, 50);
        
        // 最終ライン数表示
        GameObject finalLinesTextObj = FindOrCreateGameObject("FinalLinesText", gameOverPanelObj.transform);
        TextMeshProUGUI finalLinesText = finalLinesTextObj.GetComponent<TextMeshProUGUI>();
        if (finalLinesText == null)
        {
            finalLinesText = finalLinesTextObj.AddComponent<TextMeshProUGUI>();
        }
        finalLinesText.text = "Lines Cleared: 0";
        finalLinesText.fontSize = 36;
        finalLinesText.color = Color.white;
        finalLinesText.alignment = TextAlignmentOptions.Center;
        RectTransform finalLinesRect = finalLinesTextObj.GetComponent<RectTransform>();
        finalLinesRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalLinesRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalLinesRect.anchoredPosition = new Vector2(0, 0);
        finalLinesRect.sizeDelta = new Vector2(400, 50);
        
        // リスタートボタン
        GameObject restartButtonObj = FindOrCreateGameObject("RestartButton", gameOverPanelObj.transform);
        Button restartButton = restartButtonObj.GetComponent<Button>();
        if (restartButton == null)
        {
            restartButton = restartButtonObj.AddComponent<Button>();
        }
        Image buttonImage = restartButtonObj.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = restartButtonObj.AddComponent<Image>();
        }
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        GameObject buttonTextObj = FindOrCreateGameObject("Text", restartButtonObj.transform);
        TextMeshProUGUI buttonText = buttonTextObj.GetComponent<TextMeshProUGUI>();
        if (buttonText == null)
        {
            buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        }
        buttonText.text = "Restart";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        RectTransform buttonRect = restartButtonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -80);
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // UIManagerへの参照設定
        SerializedObject uiManagerSO = new SerializedObject(uiManager);
        uiManagerSO.FindProperty("scoreText").objectReferenceValue = scoreText;
        uiManagerSO.FindProperty("linesText").objectReferenceValue = linesText;
        uiManagerSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanelObj;
        uiManagerSO.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
        uiManagerSO.FindProperty("finalLinesText").objectReferenceValue = finalLinesText;
        uiManagerSO.FindProperty("restartButton").objectReferenceValue = restartButton;
        uiManagerSO.ApplyModifiedProperties();
        
        // GameManagerへの参照設定
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            SerializedObject gameManagerSO = new SerializedObject(gameManager);
            gameManagerSO.FindProperty("uiManager").objectReferenceValue = uiManager;
            gameManagerSO.ApplyModifiedProperties();
        }
        
        Debug.Log("UIのセットアップが完了しました");
    }
    
    /// <summary>
    /// カメラの設定
    /// </summary>
    private static void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        
        // カメラをボードの中央に配置（10x20のボードなので、中央は(5, 10)）
        mainCamera.transform.position = new Vector3(5, 10, -10);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 12f; // ボード全体が見えるように
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        
        Debug.Log("カメラの設定が完了しました");
    }
    
    /// <summary>
    /// ゲームオブジェクトを検索または作成
    /// </summary>
    private static GameObject FindOrCreateGameObject(string name, Transform parent = null)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
        }
        return obj;
    }
    
    /// <summary>
    /// コンポーネントの参照を設定（リフレクション使用）
    /// </summary>
    private static void SetComponentReference(MonoBehaviour component, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(component);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}

