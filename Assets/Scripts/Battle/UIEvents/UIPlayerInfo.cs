namespace Battle.UIEvents
{
    using UnityEngine;
    using UnityEngine.UI;
    using Battle.Units;

    public class UIPlayerInfo : MonoBehaviour
    {
        public Image hpBarFillImage;
        private UnitStats trackedStats;

        public void Init(UnitStats stats)
        {
            trackedStats = stats;
            UpdateHPBar();
        }

        public void UpdateHPBar()
        {
            if (trackedStats != null && hpBarFillImage != null)
            {
                float ratio = (float)trackedStats.CurrentHP / trackedStats.MaxHP;
                hpBarFillImage.fillAmount = ratio;

                Debug.Log($"[UI] 체력바 갱신: {trackedStats.CurrentHP} / {trackedStats.MaxHP} (비율: {ratio})");
            }
            else
            {
                Debug.LogWarning("[UI] 체력바 갱신 실패: trackedStats 또는 hpBarFillImage가 null임");
            }
        }

    }

}
