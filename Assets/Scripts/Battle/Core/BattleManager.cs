using UnityEngine;
using Battle.Units;
using System.Collections.Generic;

namespace Battle.Core
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;

        [Header("Unit Prefabs")]
        [SerializeField] private GameObject playerInfoPrefab;

        [Header("UI Panels")]
        [SerializeField] private Transform playerInfoPanel;
        [SerializeField] private Transform skillPanelParent;

        [Header("Icons")]
        [SerializeField] private Sprite attackIcon;
        [SerializeField] private Sprite skill1Icon;
        [SerializeField] private Sprite skill2Icon;

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
            UnitManager.Instance.InitializeUnits();
            BattleFlowManager.Instance.StartBattle();
        }

        public void OnTileClicked(HexTile tile)
        {
            if (!TurnManager.Instance.currentAction.isAlly) return;
            if (!UnitManager.Instance.CanMoveTo(tile)) return;

            UISelector.Instance.ClearAll();
            UnitManager.Instance.MoveSelectedPlayerTo(tile);
        }

        public void OnClickAttack()
        {
            if (!TurnManager.Instance..currentAction.isAlly()) return;

            var target = UISelector.Instance.SelectedEnemy;
            if (target == null) return;

            SkillManager.Instance.UseSelectedSkillOn(target);
            UISelector.Instance.ClearAll();
            TurnManager.Instance.EndCurrentTurn();
        }

        public void OnEnemyUnitClicked(EnemyUnit enemy)
        {
            if (!TurnManager.Instance..currentAction.isAlly()) return;

            if (UISelector.Instance.IsSelected(enemy))
            {
                UISelector.Instance.Deselect(enemy);
            }
            else
            {
                UISelector.Instance.SelectEnemy(enemy);
            }
        }

        public void OnPlayerIconClicked(PlayerUnit unit)
        {
            UnitManager.Instance.SelectPlayerUnit(unit);
        }

        public void OnSkillSlotClicked(int skillIndex)
        {
            SkillManager.Instance.SelectSkill(skillIndex);
        }

        public void OnEnemyIconClicked(UnitActionData enemyAction)
        {
            UISelector.Instance.PreviewEnemyAttack(enemyAction);
        }

        public void OnEnemyIconUnclicked()
        {
            UISelector.Instance.ClearEnemyPreview();
        }
    }
}
