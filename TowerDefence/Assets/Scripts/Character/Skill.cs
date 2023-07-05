using System.Collections.Generic;
using UnityEngine;

public struct SkillData
{
    public int ID;
}

public interface ISkill
{
    public bool Useable { get; set; }
    public void Initialization();
    public void OnUpdate();
    public void OnUse();
}

public class Skill : ISkill
{
    private bool _useable;

    public bool Useable
    {
        get => _useable;
        set => _useable = value;
    }

    public void Initialization()
    {

    }
    public void OnUpdate()
    {

    }
    public void OnUse()
    {
        
    }

}

public class SkillController : MonoBehaviour
{
    private List<ISkill> _skills;

    public void OnUpdate()
    {
        for (int i = 0; i < _skills.Count; i++)
        {
            var skill = _skills[i];
            skill.OnUpdate();
        }
    }
}