using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Battle.Units;
using Battle.Core.Manager;

public class UISkillSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI label;

    private Image background;
    public UnitSkillData skillData;

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
        SetSelected(true);
        SkillManager.Instance.SelectSkill(this);
    }

    public void SetSelected(bool isSelected)
    {
        background.color = isSelected ? new Color(1f, 1f, 1f, 0.4f) : new Color(0f, 0f, 0f, 0.2f);
    }
}
