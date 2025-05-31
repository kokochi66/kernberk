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
            if (!actionData.isAlly)
            {
                if (UISelector.Instance.IsSelected(actionData))
                {
                    UISelector.Instance.DeselectAction();
                }
                else
                {
                    UISelector.Instance.Select(actionData); // 또는 EnemyAction
                }
            }
        }

    }
}
