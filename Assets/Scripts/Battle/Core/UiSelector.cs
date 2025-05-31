using System.Collections.Generic;
using UnityEngine;

namespace Battle.UIEvents
{
    /// <summary>
    /// UI 선택 상태를 관리하는 중앙 컨트롤러.
    /// 현재 선택된 오브젝트들의 활성/비활성 상태를 관리하고,
    /// 타입에 따라 적절한 동작을 트리거할 수 있게 합니다.
    /// </summary>
    public class UISelector : MonoBehaviour
    {
        public static UISelector Instance;

        private object currentSelection;

        public enum SelectionType
        {
            None,
            Skill,
            Tile,
            Enemy,
            PlayerUnit
        }

        private SelectionType currentSelectionType = SelectionType.None;

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
        /// 오브젝트 선택 시 호출되는 함수
        /// </summary>
        public void Select(object obj, SelectionType type)
        {
            Deselect(); // 이전 선택 해제

            currentSelection = obj;
            currentSelectionType = type;

            // 선택에 따라 다른 동작 수행 가능
            switch (type)
            {
                case SelectionType.Skill:
                    Debug.Log($"[Selector] 스킬 선택: {obj}");
                    break;
                case SelectionType.Tile:
                    Debug.Log($"[Selector] 타일 선택: {obj}");
                    break;
                case SelectionType.Enemy:
                    Debug.Log($"[Selector] 적 선택: {obj}");
                    break;
                case SelectionType.PlayerUnit:
                    Debug.Log($"[Selector] 플레이어 유닛 선택: {obj}");
                    break;
            }
        }

        /// <summary>
        /// 현재 선택된 항목을 해제합니다.
        /// </summary>
        public void Deselect()
        {
            if (currentSelection != null)
            {
                Debug.Log("[Selector] 선택 해제");
                currentSelection = null;
                currentSelectionType = SelectionType.None;
            }
        }

        /// <summary>
        /// 현재 선택된 오브젝트 반환
        /// </summary>
        public object GetSelection() => currentSelection;

        public SelectionType GetSelectionType() => currentSelectionType;

        public bool HasSelection() => currentSelection != null;
    }
}
