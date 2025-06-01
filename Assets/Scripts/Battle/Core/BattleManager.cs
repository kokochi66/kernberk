using UnityEngine;
using Battle.Units;
using Battle.UIEvents;
using Battle.Data;
using System.Collections.Generic;

namespace Battle.Core
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;

        public BattleSetupData battleSetupData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            Debug.Log("🚩 [BattleManager] Start() 호출됨");
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            Debug.Log("⚔️ [BattleManager] InitializeBattle() - 전투 초기화 시작");
            BattleFlowManager.Instance.StartBattle(battleSetupData);
            Debug.Log("✅ [BattleManager] 전투 초기화 완료");
        }

        public void OnClickAttack()
        {
            if (!TurnManager.Instance.currentAction.isAlly)
            {
                Debug.Log("⛔ [BattleManager] 적 턴 중이므로 공격 불가");
                return;
            }

            EnemyUnit target = UISelectorManager.Instance.selectedEnemyUnit;
            if (target == null)
            {
                Debug.Log("❌ [BattleManager] 선택된 적 유닛이 없음 - 공격 취소");
                return;
            }

            Debug.Log($"🔥 [BattleManager] 공격 시도: {TurnManager.Instance.currentAction.playerUnit.unitName} → {target.unitName}");

            SkillManager.Instance.UseSelectedSkillOn(target, () =>
            {
                Debug.Log("🔚 [BattleManager] 스킬 연출 완료 → 턴 종료");
                TurnManager.Instance.EndCurrentTurn();
            });
        }


        public void OnEnemyUnitClicked(EnemyUnit enemy)
        {
            if (!TurnManager.Instance.currentAction.isAlly)
            {
                Debug.Log("⛔ [BattleManager] 적 턴 중이므로 선택 불가");
                return;
            }

            if (UISelectorManager.Instance.IsSelected(enemy))
            {
                Debug.Log($"🔽 [BattleManager] 적 유닛 선택 해제: {enemy.unitName}");
                UISelectorManager.Instance.DeselectEnemy();
            }
            else
            {
                Debug.Log($"🔼 [BattleManager] 적 유닛 선택됨: {enemy.unitName}");
                UISelectorManager.Instance.Select(enemy);
            }
        }

        public void OnPlayerIconClicked(PlayerUnit unit)
        {
            Debug.Log($"👆 [BattleManager] 플레이어 아이콘 클릭됨: {unit.unitName}");
            UnitManager.Instance.SelectPlayer(unit);
        }


    }
}
