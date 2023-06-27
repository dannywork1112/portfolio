using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _mask;

    private void Start()
    {
        Initalization();
    }
    public void Initalization()
    {
        SetText(string.Empty);
        SetCooldownMask(1f);
    }
    public void SetText(string text)
    {
        _nameText.SetText(text);
    }
    public void SetCooldownMask(float value)
    {
        _mask.fillAmount = value;
        if (value == 0)
            SetEnable(true);
        else
            SetEnable(false);
    }
    public void SetEnable(bool enabled)
    {
        _mask.enabled = !enabled;
    }
    public void SetColor(Color color)
    {
        _mask.color = color;
    }
}