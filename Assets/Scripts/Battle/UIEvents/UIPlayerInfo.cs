using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battle.Units;

namespace Battle.UIEvents
{
    public class UIPlayerInfo : MonoBehaviour
    {
        [Header("HP 관련")]
        public Image hpBarFillImage;
        public TextMeshProUGUI hpText;

        [Header("SP 관련")]
        public Image spBarFillImage;
        public TextMeshProUGUI spText;

        private UnitStats trackedStats;
        private int currentSP = 0;
        private int maxSP = 100;

        public void Init(UnitStats stats, int initialSP = 0, int maxSkillPoint = 100)
        {
            trackedStats = stats;
            currentSP = initialSP;
            maxSP = maxSkillPoint;

            UpdateHPBar();
            UpdateSkillPoint(currentSP);
        }

        public void UpdateHPBar()
        {
            if (trackedStats != null && hpBarFillImage != null)
            {
                float ratio = (float)trackedStats.CurrentHP / trackedStats.MaxHP;
                hpBarFillImage.fillAmount = ratio;

                if (hpText != null)
                    hpText.text = $"{trackedStats.CurrentHP}";

                Debug.Log($"[UI] 체력바 갱신: {trackedStats.CurrentHP} / {trackedStats.MaxHP} (비율: {ratio})");
            }
            else
            {
                Debug.LogWarning("[UI] 체력바 갱신 실패: trackedStats 또는 hpBarFillImage가 null임");
            }
        }

        public void UpdateSkillPoint(int sp)
        {
            currentSP = Mathf.Clamp(sp, 0, maxSP);

            if (spBarFillImage != null)
            {
                float ratio = (float)currentSP / maxSP;
                spBarFillImage.fillAmount = ratio;
            }

            if (spText != null)
                spText.text = $"{currentSP}";

            Debug.Log($"[UI] 스킬 포인트 갱신: {currentSP} / {maxSP}");
        }
    }
}
