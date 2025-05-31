using System.Collections.Generic;
using UnityEngine;
using Battle.Units;

namespace Battle.Data
{
    [CreateAssetMenu(fileName = "NewBattleSetup", menuName = "Battle/Battle Setup Data")]
    public class BattleSetupData : ScriptableObject
    {
        public List<PlayerUnitData> playerUnitDataList;
        public List<EnemyUnitData> enemyUnitDataList;
        public List<Vector2Int> PlayerSpawnPositions;
        public List<Vector2Int> EnemySpawnPositions;

    }
}
