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
        public static BattleSetupData battleSetupData;

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
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            // 초기 유닛 배치 및 UI 세팅
            BattleFlowManager.Instance.StartBattle(battleSetupData);
        }

        public void OnTileClicked(HexTile tile)
        {
            if (!TurnManager.Instance.currentAction.isAlly) return;
            if (!UnitManager.Instance.CanMoveTo(tile)) return;

            UISelector.Instance.DeselectAll();
            UnitManager.Instance.MoveSelectedPlayerTo(tile);
        }

        public void OnClickAttack()
        {
            if (!TurnManager.Instance.currentAction.isAlly) return;

            EnemyUnit target = UISelector.Instance.selectedEnemyUnit;
            if (target == null) return;

            SkillManager.Instance.UseSelectedSkillOn(target);
            UISelector.Instance.DeselectAll();
            TurnManager.Instance.EndCurrentTurn();
        }

        public void OnEnemyUnitClicked(EnemyUnit enemy)
        {
            if (!TurnManager.Instance.currentAction.isAlly) return;

            if (UISelector.Instance.IsSelected(enemy))
            {
                UISelector.Instance.DeselectEnemy();
            }
            else
            {
                UISelector.Instance.Select(enemy);
            }
        }

        public void OnPlayerIconClicked(PlayerUnit unit)
        {
            UnitManager.Instance.SelectPlayer(unit);
        }

        public void OnSkillSlotClicked(UnitSkill skill)
        {
            SkillManager.Instance.SelectSkill(skill);
        }
    }
}
