using System.Collections;
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
        [SerializeField] private Transform PlayerUnitPanel;
        [SerializeField] private RectTransform EnemyPanel;
        [SerializeField] private RectTransform PlayerInfoPanel;
        [SerializeField] private RectTransform EnemyInfoPanel;
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
        public PlayerUnit GetUnitAtTile(HexTile tile)
        {
            foreach (var unit in playerUnits)
                if (unit.CurrentTile == tile) return unit;
            return null;
        }

        public void InitializeUnits(BattleSetupData battleSetupData)
        {
            Debug.Log("[UnitManager] 유닛 초기화 시작");

            // ✅ 모든 타일 정보 가져오기
            List<HexTile> allTiles = TileManager.Instance.GetAllTiles();

            // ▶ 아군 유닛 생성 및 배치
            List<PlayerUnit> playerUnits = new();
            for (int i = 0; i < battleSetupData.playerUnitDataList.Count; i++)
            {
                PlayerUnitData unitData = battleSetupData.playerUnitDataList[i];
                PlayerUnit unit = Instantiate(unitData.prefab, PlayerUnitPanel).GetComponent<PlayerUnit>();
                unit.Init(unitData); // 스탯, 스킬 주입

                Vector2Int spawnPos = battleSetupData.PlayerSpawnPositions[i];
                HexTile tile = allTiles.FirstOrDefault(t => t.tileX == spawnPos.x && t.tileY == spawnPos.y);
                if (tile != null)
                {
                    unit.SetCurrentTile(tile);
                }
                else
                {
                    Debug.LogWarning($"❌ [PlayerUnit] 유효한 스폰 타일이 없습니다: {spawnPos}");
                }

                // ✅ Player UI 생성 및 연결
                GameObject uiObj = Instantiate(unitData.infoUIPrefab, PlayerInfoPanel);
                UIPlayerInfo infoUI = uiObj.GetComponent<UIPlayerInfo>();
                infoUI.Init(unit.stats);
                unit.PlayerInfoUI = infoUI;

                // UI 위치 배치 (예시 위치 기준)
                RectTransform rt = uiObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-290f + (i * 180f), -45f);

                // ✅ 격자 필드 위 유닛 외형 표시
                GameObject fieldSprite = Instantiate(unitData.fieldSpritePrefab, tile.transform);
                unit.FieldSpriteInstance = fieldSprite;




                playerUnits.Add(unit);
            }

            // ▶ 적 유닛 생성 및 배치
            List<EnemyUnit> enemyUnits = new();
            for (int i = 0; i < battleSetupData.enemyUnitDataList.Count; i++)
            {
                EnemyUnitData unitData = battleSetupData.enemyUnitDataList[i];
                EnemyUnit unit = Instantiate(unitData.prefab, EnemyPanel).GetComponent<EnemyUnit>();
                unit.Init(unitData); // 스탯, 이름, 공격패턴

                // ✅ 적 UI 생성 및 연결
                GameObject uiObj = Instantiate(unitData.infoUIPrefab, EnemyInfoPanel); // ⬅ 이 prefab은 UnitManager가 들고 있어야 함
                UIEnemyInfo infoUI = uiObj.GetComponent<UIEnemyInfo>();
                infoUI.Bind(unit.stats, unit.transform, uiObj);
                unit.EnemyInfoUI = infoUI;

                enemyUnits.Add(unit);
            }

            // ✅ 리스트 등록
            RegisterPlayerUnits(playerUnits);
            RegisterEnemyUnits(enemyUnits);

            Debug.Log("[UnitManager] 유닛 초기화 완료");
        }


        public List<UnitActionData> GenerateActionQueue()
        {
            List<UnitActionData> actions = new();

            foreach (var p in playerUnits)
                if (!p.stats.IsDead)
                    actions.Add(new UnitActionData(p, p.stats.Agility, true));

            foreach (var e in enemyUnits)
            {
                if (!e.stats.IsDead)
                {
                    // 🟥 tileX가 1 또는 2인 모든 타일을 공격 범위로 지정
                    List<HexTile> attackTiles = TileManager.Instance
                        .GetAllTiles()
                        .Where(tile => tile.tileX == 1 || tile.tileX == 2)
                        .ToList();

                    actions.Add(new UnitActionData(e, e.stats.Agility, false, attackTiles));
                }
            }


            Debug.Log($"⚙️ [UnitManager] 액션 큐 {actions.Count}개 생성됨");
            return actions;
        }

        ///
        /// 현재 선택된 플레이어 유닛이 해당 타일로 이동 가능한지 확인
        ///
        public bool CanMoveTo(HexTile tile)
        {
            return tile.State == HexTileState.Movable;
        }


        ///
        /// 현재 선택된 플레이어 유닛을 반환
        ///
        public PlayerUnit GetSelectedPlayer()
        {
            // 간단히 첫 번째 살아있는 유닛을 선택된 유닛으로 간주
            return playerUnits.FirstOrDefault(p => !p.stats.IsDead);
        }

        public void SelectPlayer(PlayerUnit unit)
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
            UISelectorManager.Instance.Select(unit);
            TileManager.Instance.HighlightPlayerMoveRange(unit);
            // 선택된 유닛 관련 상태 저장 또는 UI 업데이트 추가 가능
        }


        public void ExecuteEnemyTurn(UnitActionData action)
        {
            if (action.enemyUnit.stats.IsDead) return;

            StartCoroutine(EnemyAttackRoutine(action));
        }


        private IEnumerator EnemyAttackRoutine(UnitActionData action)
        {
            var attackTiles = action.attackTiles;

            // 1. 타일 빨간색 표시
            foreach (var tile in attackTiles)
                tile.SetState(HexTileState.EnemyAttackPreview);

            // 2. 1초 대기 (연출용)
            yield return new WaitForSeconds(0.5f);

            // 3. 피해 처리
            foreach (var player in playerUnits)
            {
                if (!player.stats.IsDead && attackTiles.Contains(player.CurrentTile))
                {
                    player.ReceiveAttack(action.enemyUnit.stats.Attack);
                }
            }

            // 4. 타일 리셋
            foreach (var tile in attackTiles)
                tile.ResetState();

            // 5. 턴 종료
            TurnManager.Instance.EndCurrentTurn();
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
