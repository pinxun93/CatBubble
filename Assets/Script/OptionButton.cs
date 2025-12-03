using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionButton : MonoBehaviour
{
    private Button button;
    private TMP_Text buttonText;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
        // 確保按鈕被禁用，直到選項文本被打完
        SetInteractable(false);
    }

    public void Setup(string text, UnityEngine.Events.UnityAction onClickAction)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }

        // 清除舊的監聽器並添加新的
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClickAction);

        SetInteractable(true);
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }
}