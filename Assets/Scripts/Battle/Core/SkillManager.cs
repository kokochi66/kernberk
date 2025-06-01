using System;
using System.Collections;
using UnityEngine;
using Battle.Units;

namespace Battle.Core
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 선택된 스킬을 사용하여 대상 유닛에게 연출과 함께 데미지를 가합니다.
        /// </summary>
        // public void ExecuteSkill(UnitSkillData skill, BaseUnit attacker, BaseUnit target)
        // {
        //     if (attacker == null || target == null || skill == null)
        //     {
        //         Debug.LogWarning("[SkillManager] ❌ 잘못된 스킬 또는 유닛 정보");
        //         return;
        //     }

        //     Debug.Log($"[SkillManager] ✨ 스킬 실행: {attacker.unitName} → {target.unitName} / {skill.skillName}");

        //     StartCoroutine(PlaySkillRoutine(skill, target));
        // }

        private IEnumerator PlaySkillRoutine(UnitSkillData skill, PlayerUnit attacker, EnemyUnit target, Action onComplete)
        {
            Debug.Log("[SkillManager] 🔥 스킬 이펙트 연출 시작");

            // ✅ SP 소모
            attacker.UseSkillPoint(skill.spCost);

            // ✅ 데미지
            target.ReceiveAttack(skill.damage);

            // ✅ SP 획득 (기본 공격 등)
            attacker.GainSkillPoint(skill.spGain);

            yield return new WaitForSeconds(0.5f); // 이펙트 딜레이
            Debug.Log("[SkillManager] ✅ 스킬 적용 완료");

            onComplete?.Invoke(); // 연출 끝난 뒤 콜백
        }




        public void UseSelectedSkillOn(EnemyUnit target, Action onComplete)
        {
            TileManager.Instance.ClearAllHighlights();
            var selectedSkillSlot = UISelectorManager.Instance.selectedSkill;
            var attacker = UISelectorManager.Instance.selectedPlayerUnit;

            if (selectedSkillSlot == null || attacker == null)
            {
                Debug.LogWarning("[SkillManager] 스킬 또는 유닛이 선택되지 않았습니다.");
                TileManager.Instance.HighlightPlayerMoveRange(attacker);
                return;
            }

            if (target == null || target.stats.IsDead)
            {
                Debug.LogWarning("[SkillManager] 잘못된 대상입니다.");
                TileManager.Instance.HighlightPlayerMoveRange(attacker);
                return;
            }

            var selectedSkill = selectedSkillSlot.skillData;

            // ✅ SP 부족 시 차단
            if (attacker.SkillPoint < selectedSkill.spCost)
            {
                Debug.LogWarning($"[SkillManager] ❌ SP 부족! ({attacker.SkillPoint} / 필요: {selectedSkill.spCost})");
                TileManager.Instance.HighlightPlayerMoveRange(attacker);
                return;
            }

            Debug.Log($"[SkillManager] {selectedSkill.skillName} 사용됨 → 대상: {target.unitName}, 피해: {selectedSkill.damage}");

            StartCoroutine(PlaySkillRoutine(selectedSkill, attacker, target, onComplete));
        }



    }
}
