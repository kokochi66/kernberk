using UnityEngine;
using Battle.Units;
using System.Collections;

public class UnitSkill : MonoBehaviour
{
    public string skillName;      // 스킬 이름
    public string description;    // 스킬 설명
    public int damage;            // 기본 데미지
    public Sprite icon;           // UI에 표시할 아이콘
    public int spCost;            // sp 소모량

    public UnitSkill(string name, string description, int damage, Sprite icon, int spCost)
    {
        this.skillName = name;
        this.description = description;
        this.damage = damage;
        this.icon = icon;
        this.spCost = spCost;
    }
}
