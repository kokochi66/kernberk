using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Battle.Units;
using Battle.Core.Service;

public class UISkillSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI label;

    private Image background;
    public UnitSkillData skillData;
    private bool isSelected = false;

    private void Awake()
    {
        background = GetComponent<Image>();
    }

    public void Init(UnitSkillData data)
    {
        skillData = data;
        icon.sprite = data.icon;
        label.text = data.description;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!TurnService.Instance.IsPlayerTurnActive) return; // ✅ 턴이 아닐 경우 무시

        TurnService.Instance.OnSkillSlotClicked(this);
        SetSelected(this.isSelected);
    }


    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        background.color = isSelected
            ? new Color(1f, 1f, 1f, 0.4f)
            : new Color(0f, 0f, 0f, 0.2f);
    }

}
