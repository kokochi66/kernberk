using UnityEngine;

namespace Battle.Units
{
    [CreateAssetMenu(fileName = "NewUnitSkillData", menuName = "Battle/Unit Skill Data")]
    public class UnitSkillData : ScriptableObject
    {
        [Header("기본 정보")]
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("전투 수치")]
        public int damage;
        public int spCost;     // 스킬 사용 시 소비되는 SP
        public int spGain;     // 스킬 사용 시 회복되는 SP
    }
}
