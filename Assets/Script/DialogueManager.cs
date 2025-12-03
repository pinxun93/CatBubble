using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    // --- 單例模式 ---
    public static DialogueManager Instance;

    // --- UI 引用 ---
    [Header("UI References")]
    public Image backgroundImage;
    public Image catImage;
    public TMP_Text catText; // 小貓發言文本
    public TMP_Text systemText; // 系統訊息文本
    public Transform optionContainer; // 選項按鈕的父物件

    [Header("Prefabs")]
    public GameObject optionPrefab; // 選項按鈕 Prefab

    // --- 數據與狀態 ---
    [Header("Data & Settings")]
    public List<DialogueNode> allNodes;
    public float typewriterSpeed = 0.05f; // 打字機速度 (秒/字)
    private Dictionary<string, DialogueNode> nodeDictionary;
    private int relationshipScore = 0; // 追蹤親密度
    private bool isTyping = false;

    // ==========================================================
    // MARK: - Unity Life Cycles
    // ==========================================================

    void Awake()
    {
        // 設置單例
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        // 將 List 轉換為 Dictionary，方便用 ID 查找
        if (allNodes != null && allNodes.Count > 0)
        {
            nodeDictionary = allNodes.ToDictionary(node => node.nodeID, node => node);
        }
    }

    void Start()
    {
        // 遊戲開始，載入第一個節點 (請確保您有 ID 為 "Node_1" 的 ScriptableObject)
        if (nodeDictionary.ContainsKey("Node_1"))
        {
            LoadNode("Node_1");
        }
        else
        {
            Debug.LogError("找不到起始節點 'Node_1'，請檢查 ScriptableObject 資源！");
        }
    }

    // ==========================================================
    // MARK: - Dialogue Flow Logic
    // ==========================================================

    public void LoadNode(string nodeID)
    {
        // 如果正在打字，則先停止舊的協程
        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
        }

        if (nodeID.Contains("END")) // 判斷是否為結局 ID
        {
            TriggerEnding(nodeID == "FINAL_GOOD_END");
            return;
        }

        if (!nodeDictionary.ContainsKey(nodeID))
        {
            Debug.LogError("找不到 Node ID: " + nodeID);
            return;
        }

        DialogueNode nodeToLoad = nodeDictionary[nodeID];

        // 1. 更新背景和角色圖片
        if (nodeToLoad.backgroundSprite != null)
        {
            backgroundImage.sprite = nodeToLoad.backgroundSprite;
        }
        if (nodeToLoad.catSprite != null)
        {
            catImage.sprite = nodeToLoad.catSprite;
            catImage.enabled = true;
        }
        else
        {
            catImage.enabled = false;
        }

        // 2. 更新系統訊息
        systemText.text = nodeToLoad.systemMessage;

        // 3. 清除舊選項
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }

        // 4. 開始文本顯示 (打字機效果)
        StartCoroutine(TypewriterEffect(nodeToLoad.catDialogue, nodeToLoad.options));
    }

    // 玩家點擊選項按鈕時呼叫
    public void OnOptionSelected(Option selectedOption)
    {
        // 1. 更新親密度分數
        relationshipScore += selectedOption.scoreChange;

        // 2. 顯示即時回覆
        systemText.text = selectedOption.resultText;

        // 3. 載入下一個節點
        LoadNode(selectedOption.nextNodeID);
    }

    // ==========================================================
    // MARK: - Typewriter Effect (打字機效果)
    // ==========================================================

    // 協程：逐字顯示文本
    private IEnumerator TypewriterEffect(string textToType, List<Option> options)
    {
        isTyping = true;
        catText.text = ""; // 清空文本

        // 禁用所有選項，直到打字完成
        SetOptionsActive(false);

        foreach (char letter in textToType.ToCharArray())
        {
            catText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        // 打字完成，載入選項
        LoadOptionsUI(options);
    }

    // 實例化選項按鈕
    private void LoadOptionsUI(List<Option> options)
    {
        if (options == null || options.Count == 0) return;

        foreach (var option in options)
        {
            // 創建按鈕實例
            GameObject buttonObj = Instantiate(optionPrefab, optionContainer);

            // 設置按鈕文本
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = option.optionText;
            }

            // 加入點擊監聽器
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                // 使用 Lambda 函數將當前 Option 結構體傳遞給 OnOptionSelected
                btn.onClick.AddListener(() => OnOptionSelected(option));
            }
        }
        SetOptionsActive(true);
    }

    // 控制選項容器的啟用/禁用
    private void SetOptionsActive(bool isActive)
    {
        // 確保選項容器存在且不為空
        if (optionContainer != null)
        {
            optionContainer.gameObject.SetActive(isActive);
        }
    }

    // ==========================================================
    // MARK: - Ending
    // ==========================================================

    private void TriggerEnding(bool isGoodEnding)
    {
        // 這裡可以加入場景切換邏輯 (例如：使用 SceneManager.LoadScene)
        Debug.Log("遊戲結束！親密度分數: " + relationshipScore);

        if (isGoodEnding)
        {
            Debug.Log("恭喜獲得：新的家人！");
        }
        else
        {
            Debug.Log("結局：錯過的緣分。");
        }
        // 這裡可以停止所有 UI 顯示並鎖定輸入
    }
}