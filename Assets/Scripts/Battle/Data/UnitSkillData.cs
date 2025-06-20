using UnityEngine;
using System.Collections;
using Battle.Skill;

namespace Battle.Units
{
    public class UnitSkillData : ScriptableObject
    {
        public string skillName;
        public SkillType skillType = SkillType.Attack;
        [TextArea] public string description;
        public Sprite icon;

        public int spCost;

        [Header("스킬 효과")]
        public ScriptableObject effect; // ISkillEffect 구현체
        public ISkillEffect GetEffect() => effect as ISkillEffect;


    }


    // // 이동거리 증가 스킬
    // [CreateAssetMenu(menuName = "Battle/Skill Effects/IncreaseMoveRange")]
    // public class IncreaseMoveRangeEffect : ScriptableObject, ISkillEffect
    // {
    //     public int rangeBoost;
    //     public float duration;

    //     public void Execute(PlayerUnit user, EnemyUnit target, System.Action onComplete)
    //     {
    //         user.BoostMoveRange(rangeBoost, duration); // 이건 PlayerUnit에서 처리
    //         onComplete?.Invoke();
    //     }
    // }

}
