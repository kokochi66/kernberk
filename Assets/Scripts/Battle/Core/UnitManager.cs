using System.Collections.Generic;
using UnityEngine;
using Battle.Units;

namespace Battle.Core
{
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance;

        private List<PlayerUnit> playerUnits = new List<PlayerUnit>();
        private List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

        public IReadOnlyList<PlayerUnit> PlayerUnits => playerUnits;
        public IReadOnlyList<EnemyUnit> EnemyUnits => enemyUnits;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 초기 플레이어 유닛 세팅
        /// </summary>
        public void RegisterPlayerUnits(List<PlayerUnit> units)
        {
            playerUnits = new List<PlayerUnit>(units);
        }

        /// <summary>
        /// 초기 적 유닛 세팅
        /// </summary>
        public void RegisterEnemyUnits(List<EnemyUnit> units)
        {
            enemyUnits = new List<EnemyUnit>(units);
        }

        /// <summary>
        /// 죽은 유닛을 리스트에서 제거합니다.
        /// </summary>
        public void RemoveDeadUnits()
        {
            playerUnits.RemoveAll(p => p.stats.IsDead);
            enemyUnits.RemoveAll(e => e.stats.IsDead);
        }

        /// <summary>
        /// 현재 전투가 종료되었는지 확인합니다.
        /// </summary>
        public bool IsBattleOver()
        {
            return playerUnits.Count == 0 || enemyUnits.Count == 0;
        }

        /// <summary>
        /// 현재 살아 있는 플레이어 유닛 목록 반환
        /// </summary>
        public List<PlayerUnit> GetAlivePlayers()
        {
            return playerUnits.FindAll(p => !p.stats.IsDead);
        }

        /// <summary>
        /// 현재 살아 있는 적 유닛 목록 반환
        /// </summary>
        public List<EnemyUnit> GetAliveEnemies()
        {
            return enemyUnits.FindAll(e => !e.stats.IsDead);
        }

        /// <summary>
        /// 특정 타일에 위치한 유닛 찾기
        /// </summary>
        public BaseUnit GetUnitAtTile(HexTile tile)
        {
            foreach (var unit in playerUnits)
                if (unit.CurrentTile == tile) return unit;

            foreach (var unit in enemyUnits)
                if (unit.CurrentTile == tile) return unit;

            return null;
        }

        public void InitializeUnits()
        {
            Debug.Log("[UnitManager] 유닛 초기화 시작");

            Vector2Int[] spawnPositions = new Vector2Int[]
            {
        new(2, 1), new(2, 3), new(4, 1), new(4, 3)
            };

            for (int i = 0; i < spawnPositions.Length && i < playerUnits.Count; i++)
            {
                Vector2Int pos = spawnPositions[i];
                HexTile tile = allTiles.FirstOrDefault(t => t.tileX == pos.x && t.tileY == pos.y);
                if (tile != null)
                {
                    PlayerUnit unit = playerUnits[i];
                    unit.SetCurrentTile(tile);

                    GameObject uiObj = Instantiate(playerInfoPrefab, playerInfoPanel);
                    var uiPlayerInfo = uiObj.GetComponent<UIPlayerInfo>();
                    uiPlayerInfo.Init(unit.stats);

                    RectTransform rt = uiObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(-290f + (i * 180f), -45f);
                }
            }

            Debug.Log("[UnitManager] 유닛 초기화 완료");
        }

        public List<UnitActionData> GenerateActionQueue()
        {
            List<UnitActionData> actions = new();

            foreach (var p in playerUnits)
                if (!p.stats.IsDead)
                    actions.Add(new UnitActionData(p, p.stats.Agility, true));

            foreach (var e in enemyUnits)
                if (!e.stats.IsDead)
                    actions.Add(new UnitActionData(e, e.stats.Agility, false));

            Debug.Log($"⚙️ [UnitManager] 액션 큐 {actions.Count}개 생성됨");
            return actions;
        }


    }
}
