using UnityEngine;

public class WeaponCooldownUI : MonoBehaviour
{
    [SerializeField] private CooldownItemUI[] _weaponCooldowns;

    public void AddWeapon(int index, string text)
    {
        var item = _weaponCooldowns[index];
        item.SetText(text);
    }
    public void SetCooldownColor(int index, float value, Color color)
    {
        var item = _weaponCooldowns[index];
        item.SetCooldownMask(value);
        item.SetColor(color);
    }
    public void SetCooldown(int index, float value)
    {
        var item = _weaponCooldowns[index];
        item.SetCooldownMask(value);
    }
    public void SetColor(int index, Color color)
    {
        var item = _weaponCooldowns[index];
        item.SetColor(color);
    }
}
