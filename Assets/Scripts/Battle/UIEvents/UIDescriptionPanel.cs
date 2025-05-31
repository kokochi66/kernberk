using TMPro;
using UnityEngine;

public class UIDescriptionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;

    public static UIDescriptionPanel Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Show(string message)
    {
        descriptionText.text = message;
    }

    public void Clear()
    {
        descriptionText.text = "";
    }
}
