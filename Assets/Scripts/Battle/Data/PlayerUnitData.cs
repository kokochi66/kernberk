using UnityEngine;
using Battle.Units;
using System.Collections.Generic;

namespace Battle.Data
{
    [CreateAssetMenu(fileName = "NewPlayerUnitData", menuName = "Battle/Player Unit Data")]
    public class PlayerUnitData : ScriptableObject
    {
        [Header("기본 정보")]
        public string unitName;
        [TextArea]
        public string description;

        [Header("유닛 프리팹")]
        public GameObject prefab;                 // 실제 전투 유닛 (PlayerUnit)

        [Header("스탯")]
        public UnitStats stats;
    }
}
