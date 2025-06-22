using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Battle.Units;

public class UnitActionData
{
    public PlayerUnit playerUnit;
    public EnemyUnit enemyUnit;
    public int effectiveAgility;
    public bool isAlly;

    // ✅ 이제 attackTiles 제거
    public EnemyAttackPattern usedPattern;
    public List<HexTile> TargetTiles; // ✅ Context로부터 사전에 계산된 결과 저장
    public int Damage => usedPattern?.damage ?? 0;

    public UnitActionData(PlayerUnit unit, int baseAgility, bool isAlly)
    {
        this.playerUnit = unit;
        this.effectiveAgility = baseAgility;
        this.isAlly = isAlly;
    }
    public UnitActionData(EnemyUnit unit, int baseAgility, bool isAlly, EnemyAttackPattern pattern, List<HexTile> tiles)
    {
        this.enemyUnit = unit;
        this.effectiveAgility = baseAgility;
        this.isAlly = isAlly;
        this.usedPattern = pattern;
        this.TargetTiles = tiles;
    }
}
