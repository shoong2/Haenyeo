using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class TabButton : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Color selectedBg = new(0.36f, 0.25f, 0.15f);
    [SerializeField] private Color selectedText = Color.white;
    [SerializeField] private Color normalBg = Color.clear;
    [SerializeField] private Color normalText = new(0.36f, 0.25f, 0.15f);

    private Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(SetVisual);
        SetVisual(toggle.isOn);
    }

    void SetVisual(bool isOn)
    {
        background.color = isOn ? selectedBg : normalBg;
        label.color = isOn ? selectedText : normalText;
    }
}
