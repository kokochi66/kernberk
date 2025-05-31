using UnityEngine;
using System.Collections.Generic;
using Battle.Units;

namespace Battle.Data
{
    [CreateAssetMenu(fileName = "NewEnemyUnitData", menuName = "Battle/Enemy Unit Data")]
    public class EnemyUnitData : ScriptableObject
    {
        public string unitName;
        [TextArea]
        public string description;

        public GameObject prefab;               // EnemyUnit 프리팹
        public UnitStats stats;

        public string attackPattern;            // "front_2", "cone", etc
        public Sprite icon;                     // 턴 인디케이터에서 사용할 아이콘
    }
}
