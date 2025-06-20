using UnityEngine;
using System.Collections;

namespace Battle.Skill
{
    // 기본 공격 스킬
    [CreateAssetMenu(menuName = "Battle/Skill Effects/Attack")]
    public class AttackSkillEffect : ScriptableObject, ISkillEffect
    {
        public int damage;
        public int spGain;

        public IEnumerator Execute(PlayerUnit user, EnemyUnit target, System.Action onComplete)
        {
            target.ReceiveAttack(damage);
            user.GainSkillPoint(spGain);

            yield return null; // 이펙트 연출이 있다면 여기서 기다림
            onComplete?.Invoke();
        }
    }
}
