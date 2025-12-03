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
    public TMP_Text catText;
    public TMP_Text systemText;
    public Transform optionContainer;

    // --- 計時器 UI ---
    [Header("Timer UI")]
    public Image timerFillImage;
    public float maxTime = 10f;

    [Header("Prefabs")]
    public GameObject optionPrefab;

    // --- 數據與狀態 ---
    [Header("Data & Settings")]
    public List<DialogueNode> allNodes;
    public float typewriterSpeed = 0.05f;
    private Dictionary<string, DialogueNode> nodeDictionary;
    private int relationshipScore = 0;
    private bool isTyping = false;
    private Coroutine countdownCoroutine;

    // ==========================================================
    // MARK: - Unity Life Cycles
    // ==========================================================

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        if (allNodes != null && allNodes.Count > 0)
        {
            nodeDictionary = allNodes.ToDictionary(node => node.nodeID, node => node);
        }

        // ★★★ 核心修正：使用整數值替代列舉類型 ★★★
        if (timerFillImage != null)
        {
            try
            {
                // 1. 強制將 Image Type 設為 Filled (值 = 3)
                // Image.Type.Filled 的內部值是 3
                timerFillImage.type = (Image.Type)3;

                // 2. 強制設定填充方法為 Radial 360 (值 = 2)
                // Image.FillMethod.Radial360 的內部值是 2
                timerFillImage.fillMethod = (Image.FillMethod)2;

                // 3. 強制設定填充起點為 Top (值 = 2)
                // Image.FillOrigin.Top 的內部值是 2
                timerFillImage.fillOrigin = 2; // 注意：這裡直接使用 int，而不是 (int)Image.FillOrigin.Top
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("無法在 Awake 中設定 Image 類型，可能不是標準 Image 組件：" + e.Message);
            }
        }
    }

    void Start()
    {
        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(false);
        }

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
        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
        }

        // 停止舊的計時器
        StopTimer();

        if (nodeID.Contains("END"))
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

        // 1. 更新 UI 圖片和文本
        if (nodeToLoad.backgroundSprite != null) { backgroundImage.sprite = nodeToLoad.backgroundSprite; }
        if (nodeToLoad.catSprite != null) { catImage.sprite = nodeToLoad.catSprite; catImage.enabled = true; }
        else { catImage.enabled = false; }

        systemText.text = nodeToLoad.systemMessage;

        // 2. 清除舊選項
        foreach (Transform child in optionContainer) { Destroy(child.gameObject); }

        // 3. 開始文本顯示 (打字機效果)
        StartCoroutine(TypewriterEffect(nodeToLoad.catDialogue, nodeToLoad.options));
    }

    // 玩家點擊選項按鈕時呼叫
    public void OnOptionSelected(Option selectedOption)
    {
        // 停止計時器
        StopTimer();

        relationshipScore += selectedOption.scoreChange;
        systemText.text = selectedOption.resultText;
        LoadNode(selectedOption.nextNodeID);
    }

    // ==========================================================
    // MARK: - Timer Logic
    // ==========================================================

    private void StopTimer()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator StartCountdown()
    {
        float currentTime = maxTime;
        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(true);
        }

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            // 更新 UI 視覺效果 (讓時鐘會動)
            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = currentTime / maxTime;
            }

            yield return null;
        }

        // 時間歸零，觸發超時
        TriggerTimeout();
    }

    private void TriggerTimeout()
    {
        // 禁用所有選項 (防止超時後還能點擊)
        SetOptionsInteractable(false);

        // 假設超時後導向一個通用的超時節點
        Debug.Log("時間到！未在 10 秒內選擇，導向超時處理節點。");
        // 可以在數據中創建一個 "TIMEOUT_NODE" 來處理超時
        LoadNode("TIMEOUT_NODE");
    }

    // ==========================================================
    // MARK: - Typewriter & Options Logic
    // ==========================================================

    private IEnumerator TypewriterEffect(string textToType, List<Option> options)
    {
        isTyping = true;
        catText.text = "";
        SetOptionsInteractable(false); // 禁用按鈕，直到打字完成

        foreach (char letter in textToType.ToCharArray())
        {
            catText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        // 打字完成，載入選項
        LoadOptionsUI(options);

        // 選項出現後，開始計時
        countdownCoroutine = StartCoroutine(StartCountdown());
    }

    private void LoadOptionsUI(List<Option> options)
    {
        if (options == null || options.Count == 0) return;

        foreach (var option in options)
        {
            GameObject buttonObj = Instantiate(optionPrefab, optionContainer);

            OptionButton optionScript = buttonObj.GetComponent<OptionButton>();
            if (optionScript != null)
            {
                // 使用 OptionButton 腳本來設置文本和點擊事件
                optionScript.Setup(option.optionText, () => OnOptionSelected(option));
            }
        }

        // 啟用所有選項按鈕
        SetOptionsInteractable(true);
    }

    // 設置選項按鈕是否可以互動
    private void SetOptionsInteractable(bool interactable)
    {
        OptionButton[] buttons = optionContainer.GetComponentsInChildren<OptionButton>();
        foreach (OptionButton btn in buttons)
        {
            btn.SetInteractable(interactable);
        }
    }

    // ==========================================================
    // MARK: - Ending
    // ==========================================================

    private void TriggerEnding(bool isGoodEnding)
    {
        StopTimer();
        systemText.text = isGoodEnding ? "恭喜！新的家人。" : "錯過的緣分。";
        // 這裡可以加入場景切換或結局畫面顯示
    }
}