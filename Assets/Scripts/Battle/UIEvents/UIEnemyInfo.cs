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
            Debug.Log("Enemy 위치: " + worldTarget.position);


            // 💡 반드시 적 유닛의 월드 좌표 기준으로 계산해야 함!
            if (trackedWorldTarget != null && mainCamera != null)
            {
                Vector3 worldPos = trackedWorldTarget.position + new Vector3(0, 1.5f, 0);
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                transform.position = screenPos; // ✅ 여기서 UI 자신의 위치를 지정
                Debug.Log("UI 위치: " + transform.position);

            }
        }

        void Update()
        {
            if (trackedStats != null)
            {
                float ratio = (float)trackedStats.CurrentHP / trackedStats.MaxHP;
                hpBarFillImage.fillAmount = ratio;
            }

            // 💡 적의 머리 위쪽 위치로 약간 올리기
            if (trackedWorldTarget != null && mainCamera != null)
            {
                Vector3 worldPos = trackedWorldTarget.position + new Vector3(0, -220f, 0); // 머리 위
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                transform.position = screenPos;
            }
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
