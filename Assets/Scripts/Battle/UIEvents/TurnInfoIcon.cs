namespace Battle.UIEvents
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Battle.Core; // BattleManager 참조

    public class TurnInfoIcon : MonoBehaviour, IPointerClickHandler
    {
        public UnitActionData actionData;
        public bool isClicked = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!this.actionData.isAlly && !this.isClicked)
            {
                Debug.Log("🖱️ UI 아이콘 클릭됨!");
                BattleManager.Instance.OnEnemyIconClicked(actionData);
                this.isClicked = true;
            }
            else if (this.isClicked)
            {
                BattleManager.Instance.OnEnemyIconUnclicked();
                this.isClicked = false;
            }

        }
    }
}
