using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle.Units;

namespace Battle.Core
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance;
        private UnitSkill selectedSkill;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SelectSkill(UnitSkill skill)
        {
            selectedSkill = skill;
            Debug.Log($"[SkillManager] 스킬 선택됨: {skill.skillName}");
        }


        /// <summary>
        /// 선택된 스킬을 사용하여 대상 유닛에게 연출과 함께 데미지를 가합니다.
        /// </summary>
        /// <param name="skill">사용할 스킬</param>
        /// <param name="attacker">공격 유닛</param>
        /// <param name="target">피격 유닛</param>
        public void ExecuteSkill(UnitSkill skill, BaseUnit attacker, BaseUnit target)
        {
            if (attacker == null || target == null || skill == null)
            {
                Debug.LogWarning("[SkillManager] ❌ 잘못된 스킬 또는 유닛 정보");
                return;
            }

            Debug.Log($"[SkillManager] ✨ 스킬 실행: {attacker.unitName} → {target.unitName} / {skill.skillName}");

            StartCoroutine(PlaySkillRoutine(skill, target));
        }


        private int GetSkillDamage(int index)
        {
            switch (index)
            {
                case 0: return 10;  // 기본 공격
                case 1: return 6;   // 스킬1
                case 2: return 20;  // 스킬2
                default: return 0;
            }
        }

        private IEnumerator PlaySkillRoutine(UnitSkill skill, BaseUnit target)
        {
            Debug.Log("[SkillManager] 🔥 스킬 이펙트 연출 시작");
            yield return new WaitForSeconds(0.3f);

            target.ReceiveAttack(skill.damage);

            Debug.Log("[SkillManager] ✅ 스킬 적용 완료");

            TurnManager.Instance.EndCurrentTurn();

            selectedSkill = null;
        }


        public void UseSelectedSkillOn(BaseUnit target)
        {
            if (selectedSkill == null)
            {
                Debug.LogWarning("[SkillManager] 스킬이 선택되지 않았습니다.");
                return;
            }

            if (target == null || target.stats.IsDead)
            {
                Debug.LogWarning("[SkillManager] 잘못된 대상입니다.");
                return;
            }

            Debug.Log($"[SkillManager] {selectedSkill.skillName} 사용됨 → 대상: {target.unitName}, 피해: {selectedSkill.damage}");

            StartCoroutine(PlaySkillRoutine(selectedSkill, target));
        }

    }
}
