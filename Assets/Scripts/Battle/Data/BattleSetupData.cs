using System.Collections.Generic;
using UnityEngine;
using Battle.Units;

namespace Battle.Data
{
    public class BattleSetupData
    {
        public List<PlayerUnit> PlayerUnits { get; private set; }
        public List<EnemyUnit> EnemyUnits { get; private set; }
        public List<Vector2Int> PlayerSpawnPositions { get; private set; }
        public List<Vector2Int> EnemySpawnPositions { get; private set; }

        public GameObject PlayerInfoPrefab { get; private set; }
        public Transform PlayerInfoPanel { get; private set; }

        public BattleSetupData(GameObject playerInfoPrefab, Transform playerInfoPanel)
        {
            PlayerInfoPrefab = playerInfoPrefab;
            PlayerInfoPanel = playerInfoPanel;

            PlayerUnits = GenerateInitialPlayers();
            EnemyUnits = GenerateInitialEnemies();
            PlayerSpawnPositions = new List<Vector2Int> { new(2, 1), new(2, 3), new(4, 1) };
            EnemySpawnPositions = new List<Vector2Int> { new(5, 2), new(6, 3) };
        }

        private List<PlayerUnit> GenerateInitialPlayers()
        {
            var unitPrefab = Resources.Load<GameObject>("Units/PlayerUnit");
            var list = new List<PlayerUnit>();

            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.Instantiate(unitPrefab);
                var unit = go.GetComponent<PlayerUnit>();
                unit.unitName = $"플레이어 유닛 {i + 1}";
                unit.stats = new UnitStats(30 + i * 5, 3 + i, 3, 25 - i * 2);
                list.Add(unit);
            }
            return list;
        }

        private List<EnemyUnit> GenerateInitialEnemies()
        {
            var enemyPrefab = Resources.Load<GameObject>("Units/EnemyUnit");
            var list = new List<EnemyUnit>();

            for (int i = 0; i < 2; i++)
            {
                var go = GameObject.Instantiate(enemyPrefab);
                var unit = go.GetComponent<EnemyUnit>();
                unit.unitName = $"적 유닛 {i + 1}";
                unit.stats = new UnitStats(20, 4, 2, 15);
                list.Add(unit);
            }
            return list;
        }
    }
}
