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
        /// 스킬 사용을 실행한다. 이 메서드는 연출 포함 데미지 적용 등을 모두 처리한다.
        /// </summary>
        /// <param name="skillIndex">선택된 스킬 인덱스</param>
        /// <param name="attacker">공격하는 유닛</param>
        /// <param name="target">공격 대상 유닛</param>
        public void ExecuteSkill(int skillIndex, BaseUnit attacker, BaseUnit target)
        {
            if (attacker == null || target == null)
            {
                Debug.LogWarning("[SkillManager] ❌ 잘못된 스킬 대상");
                return;
            }

            Debug.Log($"[SkillManager] ✨ 스킬 실행: {attacker.unitName} → {target.unitName}, Index: {skillIndex}");

            // 간단한 데미지 공식 예시
            int damage = GetSkillDamage(skillIndex);

            // 연출 시작
            StartCoroutine(PlaySkillRoutine(attacker, target, damage));
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

        private IEnumerator PlaySkillRoutine(BaseUnit attacker, BaseUnit target, int damage)
        {
            // (1) 이펙트 재생 또는 애니메이션 등
            Debug.Log("[SkillManager] 🔥 스킬 이펙트 연출 시작");
            yield return new WaitForSeconds(0.3f);

            // (2) 데미지 적용
            target.ReceiveAttack(damage);

            // (3) 후처리
            Debug.Log("[SkillManager] ✅ 스킬 적용 완료");

            // (4) 다음 턴으로 넘기기 (BattleManager 호출)
            TurnManager.Instance.EndCurrentTurn();
        }
    }
}
