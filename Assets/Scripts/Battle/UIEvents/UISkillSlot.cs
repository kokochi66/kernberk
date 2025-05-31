using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UISkillSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI label;

    private Image background; // ← SkillSlot 자체에 붙은 Image
    private System.Action onClick;

    private void Awake()
    {
        // SkillSlot 루트에 붙은 Image를 자동 참조
        background = GetComponent<Image>();
    }

    public void Init(Sprite skillSprite, string description, System.Action clickAction)
    {
        icon.sprite = skillSprite;
        label.text = description;
        onClick = clickAction;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱️ [SkillSlot] 클릭됨: {label.text}");
        onClick?.Invoke();
    }

    public void SetSelected(bool isSelected)
    {
        background.color = isSelected ? new Color(1, 1, 1, 0.4f) : new Color(0, 0, 0, 0.2f);
    }
}
