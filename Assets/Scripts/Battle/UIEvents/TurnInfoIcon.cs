namespace Battle.UIEvents
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Battle.Core.Service;

    public class TurnInfoIcon : MonoBehaviour, IPointerClickHandler
    {
        public UnitActionData actionData;
        public bool isClicked = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!TurnService.Instance.IsPlayerTurnActive) return; // ✅

            if (!actionData.isAlly)
            {
                if (isClicked)
                {
                    TurnService.Instance.DeselectAction();
                    this.isClicked = false;
                }
                else
                {
                    TurnService.Instance.SelectAction(actionData);
                    this.isClicked = true;
                }
            }
        }


    }
}
