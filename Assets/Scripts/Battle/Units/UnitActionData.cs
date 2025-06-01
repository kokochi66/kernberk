using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Battle.Units;
public class UnitActionData
{
    public PlayerUnit playerUnit;
    public EnemyUnit enemyUnit;
    public int effectiveAgility; // 실제 민첩도 (깎인 값 포함)
    public bool isAlly; // 아군/적 구분
    public List<HexTile> attackTiles;       // 적 유닛일 경우 공격 범위

    public UnitActionData(PlayerUnit unit, int baseAgility, bool isAlly)
    {
        this.playerUnit = unit;
        this.effectiveAgility = baseAgility;
        this.isAlly = isAlly;
    }

        public UnitActionData(EnemyUnit unit, int baseAgility, bool isAlly, List<HexTile> attackTiles = null)
    {
        this.enemyUnit = unit;
        this.effectiveAgility = baseAgility;
        this.isAlly = isAlly;
        this.attackTiles = attackTiles;
    }
}
