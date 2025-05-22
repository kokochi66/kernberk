using UnityEngine;
public class UnitActionData
{
    public MonoBehaviour unit; // PlayerUnit or EnemyUnit
    public int effectiveAgility; // 실제 민첩도 (깎인 값 포함)
    public bool isAlly; // 아군/적 구분

    public UnitActionData(MonoBehaviour unit, int baseAgility, bool isAlly)
    {
        this.unit = unit;
        this.effectiveAgility = baseAgility;
        this.isAlly = isAlly;
    }
}
