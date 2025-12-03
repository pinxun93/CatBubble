using UnityEngine;
using System.Collections.Generic;

// 允許 Unity 編輯器在 Assets -> Create 選單中創建這個腳本
[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    // --- 節點基礎資訊 ---
    [Header("Node Info")]
    public string nodeID;           // 唯一的 ID，用於流程跳轉
    public Sprite backgroundSprite; // 場景背景圖片

    // --- 角色圖片 ---
    [Header("Character Visuals")]
    public Sprite catSprite;        // 該節點對話時，小貓要顯示的圖片

    // --- 對話文本 ---
    [Header("Text Content")]
    [TextArea(3, 10)]
    public string catDialogue;      // 小貓的發言 (將使用打字機效果)
    [TextArea(1, 5)]
    public string systemMessage;    // 系統訊息（例如：即時回覆或提示）

    // --- 選項列表 ---
    [Header("Options")]
    public List<Option> options = new List<Option>();
}

// 必須加上 [System.Serializable] 才能在 Inspector 視窗中顯示
[System.Serializable]
public struct Option
{
    [Header("Player Option")]
    public string optionText;       // 玩家點擊的按鈕文字

    [Header("Result & Next")]
    public string resultText;       // 選擇後顯示的即時回覆
    public string nextNodeID;       // 選擇此選項後，要跳轉到的下一個 Node ID
    public int scoreChange;         // 親密度分數增減值 (範例用)
}