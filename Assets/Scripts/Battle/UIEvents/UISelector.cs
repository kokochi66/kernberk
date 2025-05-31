using System.Collections.Generic;
using UnityEngine;
using Battle.Core;

namespace Battle.UIEvents
{
    /// 
    /// UI 선택 상태를 관리하는 중앙 컨트롤러.
    /// 현재 선택된 오브젝트들의 활성/비활성 상태를 관리하고,
    /// 타입에 따라 적절한 동작을 트리거할 수 있게 합니다.
    /// 
    public class UISelector : MonoBehaviour
    {
        public static UISelector Instance;

        public UnitActionData selectedActionData { get; private set; }
        public PlayerUnit selectedPlayerUnit { get; private set; }
        public EnemyUnit selectedEnemyUnit { get; private set; }
        public UnitSkill selectedSkill { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Select(UnitActionData action)
        {
            selectedActionData = action;
            Debug.Log($"[Selector] 액션 선택: {(action.isAlly ? "아군" : "적군")} / 민첩 {action.effectiveAgility}");

            if (!action.isAlly && action.attackTiles != null)
            {
                TileManager.Instance.HighlightEnemyAttackPreview(action.attackTiles);
            }
        }

        public void Select(PlayerUnit player)
        {
            selectedPlayerUnit = player;
            Debug.Log($"[Selector] 플레이어 선택: {player.unitName}");
        }

        public void Select(EnemyUnit enemy)
        {
            selectedEnemyUnit = enemy;
            Debug.Log($"[Selector] 적 선택: {enemy.unitName}");
        }

        public void Select(UnitSkill skill)
        {
            selectedSkill = skill;
            Debug.Log($"[Selector] 스킬 선택: {skill.skillName}");
        }

        public void DeselectAction()
        {
            if (selectedActionData != null)
            {
                if (!selectedActionData.isAlly)
                {
                    TileManager.Instance.PopHighlightLayer();
                }

                selectedActionData = null;
                Debug.Log("[Selector] 액션 선택 해제");
            }
        }

        public void DeselectPlayer()
        {
            selectedPlayerUnit = null;
            Debug.Log("[Selector] 플레이어 선택 해제");
        }

        public void DeselectEnemy()
        {
            selectedEnemyUnit = null;
            Debug.Log("[Selector] 적 유닛 선택 해제");
        }

        public void DeselectSkill()
        {
            selectedSkill = null;
            Debug.Log("[Selector] 스킬 선택 해제");
        }


        public void DeselectAll()
        {
            selectedActionData = null;
            selectedPlayerUnit = null;
            selectedEnemyUnit = null;
            selectedSkill = null;
            Debug.Log("[Selector] 모든 선택 해제");
        }

        public bool IsSelected(UnitActionData action)
        {
            return selectedActionData == action;
        }

        public bool IsSelected(PlayerUnit player)
        {
            return selectedPlayerUnit == player;
        }

        public bool IsSelected(EnemyUnit enemy)
        {
            return selectedEnemyUnit == enemy;
        }

        public bool IsSelected(UnitSkill skill)
        {
            return selectedSkill == skill;
        }

    }

}
