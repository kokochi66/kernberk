// PlayerUnitData.cs
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
        [TextArea] public string description;

        [Header("프리팹")]
        public GameObject prefab; // PlayerUnit 프리팹

        [Header("UI 관련")]
        public GameObject infoUIPrefab;
        public GameObject turnIconPrefab;
        public GameObject fieldSpritePrefab;

        [Header("스탯")]
        public UnitStats stats;

        [Header("스킬 목록 (데이터 기반)")]
        public List<UnitSkillData> skillDataList;
    }
}
