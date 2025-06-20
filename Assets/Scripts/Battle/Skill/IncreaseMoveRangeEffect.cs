using UnityEngine;
using System.Collections;
using Battle.Core.Manager;

namespace Battle.Skill
{
    [CreateAssetMenu(menuName = "Battle/Skill Effects/IncreaseMoveRange")]
    public class IncreaseMoveRangeEffect : ScriptableObject, ISkillEffect
    {
        public int rangeBoost;

        public IEnumerator Execute(PlayerUnit user, EnemyUnit target, System.Action onComplete)
        {
            user.BoostMoveRange(rangeBoost);

            TileManager.Instance.ClearAllHighlights();
            TileManager.Instance.HighlightPlayerMoveRange(user);

            Debug.Log($"🟢 {user.unitName} 이동거리 +{rangeBoost} 적용됨");

            yield return null;
            onComplete?.Invoke();
        }
    }

}
