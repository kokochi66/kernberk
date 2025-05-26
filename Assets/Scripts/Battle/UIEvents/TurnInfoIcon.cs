namespace Battle.UIEvents
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Battle.Core; // BattleManager 참조

    public class TurnInfoIcon : MonoBehaviour, IPointerClickHandler
    {
        public UnitActionData actionData;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("🖱️ UI 아이콘 클릭됨!");
            BattleManager.Instance.PreviewUnitActionRange(actionData);
        }
    }
}
