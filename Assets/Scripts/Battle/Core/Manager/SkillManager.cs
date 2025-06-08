using System;
using System.Collections;
using UnityEngine;
using Battle.Units;

namespace Battle.Core.Manager
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance;

        [SerializeField] private GameObject skillSlotPrefab;
        [SerializeField] private RectTransform PlayerSkillInfoPanel;


        private UISkillSlot selectedSkillSlot;
        public UISkillSlot SelectedSkillSlot => selectedSkillSlot;
        public UnitSkillData SelectedSkillData => selectedSkillSlot?.skillData;



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ShowSkills(PlayerUnit unit)
        {
            foreach (Transform child in PlayerSkillInfoPanel.transform)
                Destroy(child.gameObject);

            foreach (var skillData in unit.skills)
            {
                GameObject slot = Instantiate(skillSlotPrefab, PlayerSkillInfoPanel);
                var slotUI = slot.GetComponent<UISkillSlot>();
                slotUI.Init(skillData);
            }

            Debug.Log($"[SkillManager] {unit.unitName}의 스킬 {unit.skills.Count}개 표시 완료");
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

        public void UseSkillOn(
            PlayerUnit attacker,
            EnemyUnit target,
            Action onComplete,
            Action onSkillFailed = null)
        {
            UnitSkillData skillData = selectedSkillSlot.skillData;
            if (attacker == null || skillData == null)
            {
                Debug.LogWarning("[SkillManager] 스킬 또는 유닛이 null입니다.");
                onSkillFailed?.Invoke();
                return;
            }

            if (target == null || target.stats.IsDead)
            {
                Debug.LogWarning("[SkillManager] 잘못된 대상입니다.");
                onSkillFailed?.Invoke();
                return;
            }

            if (attacker.SkillPoint < skillData.spCost)
            {
                Debug.LogWarning($"[SkillManager] ❌ SP 부족! ({attacker.SkillPoint} / 필요: {skillData.spCost})");
                onSkillFailed?.Invoke();
                return;
            }

            Debug.Log($"[SkillManager] {skillData.skillName} 사용됨 → 대상: {target.unitName}, 피해: {skillData.damage}");

            StartCoroutine(PlaySkillRoutine(skillData, attacker, target, onComplete));
        }


        public void SelectSkill(UISkillSlot skillSlot)
        {
            DeselectSkill(); // 이전 선택 해제
            selectedSkillSlot = skillSlot;
            skillSlot.SetSelected(true);
            Debug.Log($"[SkillManager] 스킬 선택됨: {skillSlot.skillData.skillName}");
        }

        public void DeselectSkill()
        {
            if (selectedSkillSlot != null)
            {
                selectedSkillSlot.SetSelected(false);
                Debug.Log($"[SkillManager] 스킬 선택 해제: {selectedSkillSlot.skillData.skillName}");
                selectedSkillSlot = null;
            }
        }


    }
}
