using UnityEngine;
using System.Collections.Generic;
using Battle.Units;
using Battle.Core.Manager;
using System.Linq;


[CreateAssetMenu(menuName = "Battle/Enemy/HexPattern (By Position)")]
public class EnemyAttackPattern : ScriptableObject
{
    public string patternName;
    public int damage;

    // ✅ 기존 HexTile 리스트 대신 좌표 정보로 변경
    public List<Vector2Int> tilePositions = new();
    public TargetingType targetingType = TargetingType.FixedTiles;

    public PatternTriggerType triggerType = PatternTriggerType.Always;
    public int triggerValue;

    public enum PatternTriggerType
    {
        Always,
        HpBelowPercent,
        HpAbovePercent,
        TurnCountEquals
    }

    public enum TargetingType
    {
        FixedTiles,         // 기존: 절대 좌표
        RightmostPlayerUnit // 가장 오른쪽 유닛 기준
    }


    public bool ShouldActivate(EnemyUnit unit, EnemyPatternContext context)
    {
        int hpPercent = Mathf.RoundToInt((unit.stats.CurrentHP / (float)unit.stats.MaxHP) * 100);
        return triggerType switch
        {
            PatternTriggerType.Always => true,
            PatternTriggerType.HpBelowPercent => hpPercent <= triggerValue,
            PatternTriggerType.HpAbovePercent => hpPercent >= triggerValue,
            PatternTriggerType.TurnCountEquals => context.turnCount == triggerValue,
            _ => false
        };
    }

    // ✅ 런타임에서 실제 타일 정보로 변환
    public List<HexTile> ResolveTiles(EnemyPatternContext context)
    {
        List<HexTile> result = new();

        switch (targetingType)
        {
            case TargetingType.FixedTiles:
                foreach (var pos in tilePositions)
                {
                    var tile = TileManager.Instance.GetTileAt(pos.x, pos.y);
                    if (tile != null) result.Add(tile);
                }
                break;

            case TargetingType.RightmostPlayerUnit:
                var target = context.playerUnits
                    .OrderByDescending(p => p.CurrentTile.tileX)
                    .FirstOrDefault();

                if (target != null)
                {
                    Vector2Int origin = new(target.CurrentTile.tileX, target.CurrentTile.tileY);
                    foreach (var offset in tilePositions)
                    {
                        var tile = TileManager.Instance.GetTileAt(origin.x + offset.x, origin.y + offset.y);
                        if (tile != null) result.Add(tile);
                    }
                }
                break;
        }

        return result;
    }

}
