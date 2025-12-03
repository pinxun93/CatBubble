using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    // --- 節點基礎資訊 ---
    [Header("Node Info")]
    public string nodeID;
    public Sprite backgroundSprite;

    // --- 角色圖片 ---
    [Header("Character Visuals")]
    public Sprite catSprite;

    // --- 對話文本 ---
    [Header("Text Content")]
    [TextArea(3, 10)]
    public string catDialogue;
    [TextArea(1, 5)]
    public string systemMessage;

    // --- 選項列表 ---
    [Header("Options")]
    public List<Option> options = new List<Option>();
}

[System.Serializable]
public struct Option
{
    [Header("Player Option")]
    public string optionText;

    [Header("Result & Next")]
    public string resultText;
    public string nextNodeID;
    public int scoreChange;
}