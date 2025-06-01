namespace Battle.UIEvents
{
    using UnityEngine;
    using UnityEngine.UI;
    using Battle.Units;

    public class UIEnemyInfo : MonoBehaviour
    {
        public Image hpBarFillImage;
        private UnitStats trackedStats;
        private Transform trackedWorldTarget;

        private Camera mainCamera;

        public void Bind(UnitStats stats, Transform worldTarget, GameObject uiGO)
        {

            trackedStats = stats;
            trackedWorldTarget = worldTarget;
            mainCamera = Camera.main;


            // 💡 반드시 적 유닛의 월드 좌표 기준으로 계산해야 함!
            if (trackedWorldTarget != null && mainCamera != null)
            {
                Vector3 worldPos = trackedWorldTarget.position + new Vector3(0, -220f, 0);
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                bool isInvalid = float.IsNaN(screenPos.x) || float.IsNaN(screenPos.y) || float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y);

                Debug.Log($"🧪 [UI Pos Debug] target={trackedWorldTarget.name}, worldPos={worldPos}, screenPos={screenPos}, isInvalid={isInvalid}");

                transform.position = screenPos; // ✅ 여기서 UI 자신의 위치를 지정
            }
        }

        void Update()
        {

        }

        public void UpdateHPBar()
        {
            if (trackedStats != null && hpBarFillImage != null)
            {
                float ratio = (float)trackedStats.CurrentHP / trackedStats.MaxHP;
                hpBarFillImage.fillAmount = ratio;
            }
        }
    }



}
