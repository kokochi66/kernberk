using System.Collections.Generic;
using UnityEngine;
using Battle.Units;
using Battle.UIEvents;
using Battle.Data;
using System.Linq;

namespace Battle.Core
{
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance;
        [SerializeField] public RectTransform PlayerInfoPanel;
        [SerializeField] public RectTransform EnemyInfoPanel;
        [SerializeField] public RectTransform TurnOrderPanel;


        private List<PlayerUnit> playerUnits = new List<PlayerUnit>();
        private List<EnemyUnit> enemyUnits = new List<EnemyUnit>();
        private List<HexTile> allTiles = new List<HexTile>();

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

        public bool AllEnemiesDefeated() => enemyUnits.All(e => e.stats.IsDead);

        public bool AllPlayersDefeated() => playerUnits.All(p => p.stats.IsDead);

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

        public void InitializeUnits(BattleSetupData battleSetupData)
        {
            Debug.Log("[UnitManager] 유닛 초기화 시작");

            List<PlayerUnit> playerUnits = new();
            for (int i = 0; i < battleSetupData.playerUnitDataList.Count; i++)
            {
                PlayerUnitData unitData = battleSetupData.playerUnitDataList[i];
                PlayerUnit unit = Instantiate(unitData.prefab).GetComponent<PlayerUnit>();
                unit.Init(unitData); // 유닛에게 데이터를 세팅해주는 함수 필요
                playerUnits.Add(unit);
            }

            List<EnemyUnit> enemyUnits = new();

            // 내부 리스트에 등록
            RegisterPlayerUnits(playerUnits);
            RegisterEnemyUnits(enemyUnits);

            List<HexTile> allTiles = TileManager.Instance.GetAllTiles();

            // ▶ 아군 배치
            for (int i = 0; i < playerUnits.Count; i++)
            {
                var unit = playerUnits[i];
                Vector2Int spawnPos = battleSetupData.PlayerSpawnPositions[i];
                HexTile tile = allTiles.FirstOrDefault(t => t.tileX == spawnPos.x && t.tileY == spawnPos.y);

                if (tile != null)
                {
                    unit.SetCurrentTile(tile);

                    // UI 생성
                    GameObject uiObj = GameObject.Instantiate(unit.infoUIPrefab, PlayerInfoPanel);
                    var uiPlayerInfo = uiObj.GetComponent<UIPlayerInfo>();
                    uiPlayerInfo.Init(unit.stats);

                    RectTransform rt = uiObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(-290f + (i * 180f), -45f);
                }
                else
                {
                    Debug.LogWarning($"❌ [UnitManager] 유효한 스폰 타일이 없습니다. ({spawnPos.x}, {spawnPos.y})");
                }
            }

            // ▶ 적군 배치
            for (int i = 0; i < enemyUnits.Count; i++)
            {
                var unit = enemyUnits[i];
                Vector2Int spawnPos = battleSetupData.EnemySpawnPositions[i];
                HexTile tile = allTiles.FirstOrDefault(t => t.tileX == spawnPos.x && t.tileY == spawnPos.y);

                if (tile != null)
                {
                    unit.SetCurrentTile(tile);
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

        ///
        /// 현재 선택된 플레이어 유닛이 해당 타일로 이동 가능한지 확인
        ///
        public bool CanMoveTo(HexTile tile)
        {
            PlayerUnit selected = GetSelectedPlayer();
            if (selected == null || tile.IsOccupied()) return false;

            return IsAdjacent(selected.CurrentTile, tile);
        }

        ///
        /// 현재 선택된 플레이어 유닛을 반환
        ///
        public PlayerUnit GetSelectedPlayer()
        {
            // 간단히 첫 번째 살아있는 유닛을 선택된 유닛으로 간주
            return playerUnits.FirstOrDefault(p => !p.stats.IsDead);
        }

        public void MoveSelectedPlayerTo(HexTile tile)
        {
            var selectedPlayer = playerUnits.FirstOrDefault(p => p != null && !p.stats.IsDead && p.CurrentTile != null);
            if (selectedPlayer != null)
            {
                selectedPlayer.MoveTo(tile, () =>
                {
                    TurnManager.Instance.EndCurrentTurn();
                });
            }
        }

        public void SelectPlayer(BaseUnit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[UnitManager] null 유닛이 선택 시도됨");
                return;
            }

            if (!playerUnits.Contains(unit))
            {
                Debug.LogWarning("[UnitManager] 유닛이 플레이어 목록에 없음");
                return;
            }

            Debug.Log($"[UnitManager] 유닛 선택: {unit.unitName}");
            // 선택된 유닛 관련 상태 저장 또는 UI 업데이트 추가 가능
        }


        public void ExecuteEnemyTurn(UnitActionData action)
        {
            if (action.unit.stats.IsDead) return;

            var attackTiles = action.attackTiles;

            foreach (var tile in attackTiles)
                tile.Highlight(Color.red);

            foreach (var player in playerUnits)
            {
                if (!player.stats.IsDead && attackTiles.Contains(player.CurrentTile))
                {
                    player.ReceiveAttack(action.unit.stats.Attack);
                }
            }

            foreach (var tile in attackTiles)
                tile.ResetHighlight();
        }


        private bool IsAdjacent(HexTile from, HexTile to)
        {
            int dx = to.tileX - from.tileX;
            int dy = to.tileY - from.tileY;

            (int dx, int dy)[] offsets = (from.tileX % 2 == 0)
                ? new (int, int)[] { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) }
                : new (int, int)[] { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) };

            foreach (var offset in offsets)
            {
                if (dx == offset.dx && dy == offset.dy)
                    return true;
            }

            return false;
        }

    }
}
