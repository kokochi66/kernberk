using UnityEngine;
using Battle.Units;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Battle.UIEvents;

namespace Battle.Core
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;
        [SerializeField] private List<PlayerUnit> playerUnits = new List<PlayerUnit>();
        private int selectedPlayerIndex = 0;

        public HexTile[] allTiles;
        public EnemyUnit currentEnemy;
        [SerializeField] private Transform turnInfoPanelParent;
        [SerializeField] private GameObject turnInfoPrefabPlayer;
        [SerializeField] private GameObject turnInfoPrefabEnemy;

        [SerializeField] public GameObject playerInfoPrefab;
        [SerializeField] public Transform playerInfoPanel;


        private List<GameObject> spawnedTurnIcons = new List<GameObject>();

        public bool IsPlayerTurn { get; private set; } = true;

        private enum TurnState { PlayerTurn, EnemyTurn }
        private TurnState currentTurn = TurnState.PlayerTurn;
        private Queue<UnitActionData> actionQueue = new Queue<UnitActionData>();
        private UnitActionData currentAction;
        private bool isProcessingTurn = false;
        private int currentTurnNo = 0;
        private List<HexTile> movePreviewTiles = new List<HexTile>();
        private List<HexTile> attackPreviewTiles = new List<HexTile>();

        private UIPlayerInfo uiPlayerInfo;
        private EnemyUnit selectedEnemy;
        public PlayerUnit SelectedPlayer => playerUnits[selectedPlayerIndex];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (allTiles == null || allTiles.Length == 0)
                allTiles = FindObjectsOfType<HexTile>();

            // 최대 3 유닛 배치
            Vector2Int[] spawnPositions = new Vector2Int[]
            {
                new(2, 1), new(2, 3), new(4, 1), new(4, 3)
            };

            for (int i = 0; i < spawnPositions.Length; i++)
            {
                Vector2Int pos = spawnPositions[i];
                HexTile tile = allTiles.FirstOrDefault(t => t.tileX == pos.x && t.tileY == pos.y);
                if (tile != null)
                {
                    var unit = playerUnits[i]; // 또는 Instantiate
                    unit.SetCurrentTile(tile);

                    GameObject uiObj = Instantiate(playerInfoPrefab, playerInfoPanel);
                    var uiPlayerInfo = uiObj.GetComponent<UIPlayerInfo>();
                    uiPlayerInfo.Init(unit.stats);

                    // ✅ UI 위치 밀어주기
                    RectTransform rt = uiObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(-290f + (i * 180f), -45f);  // spacing은 상황에 따라 조정
                }
            }



            SelectPlayer(0); // 첫 번째 플레이어 선택
            StartCoroutine(StartStep());
        }


        public void SelectPlayer(int index)
        {
            if (index < 0 || index >= playerUnits.Count)
            {
                Debug.LogWarning("❌ 유효하지 않은 플레이어 인덱스");
                return;
            }

            selectedPlayerIndex = index;
            Debug.Log($"[BattleManager] 유닛 선택됨: {SelectedPlayer.unitName} (Index: {index})");

            // 이동 가능한 타일 하이라이트
            HighlightMoveableTiles(SelectedPlayer);
        }


        public void OnTileClicked(HexTile tile)
        {
            if (!IsPlayerTurn) return;
            if (SelectedPlayer == null || tile.IsOccupied()) return;

            // 💡 이동 가능 범위 확인
            if (!IsAdjacent(SelectedPlayer.CurrentTile, tile))
            {
                Debug.Log("❌ 인접한 타일이 아닙니다. 이동 불가");
                return;
            }

            // 💡 하이라이트 제거
            ClearAllPreviewTiles();
            DeselectEnemy(); // ✅ 공격 후 선택 해제

            SelectedPlayer.MoveTo(tile, () =>
            {
                IsPlayerTurn = false;
                StartCoroutine(ProcessNextAction());
            });
        }

        public void OnClickAttack()
        {
            if (!IsPlayerTurn) return;

            // 공격 실행
            if (currentEnemy != null)
            {
                int atk = SelectedPlayer.stats.Attack;
                currentEnemy.ReceiveAttack(atk);
            }

            DeselectEnemy(); // ✅ 공격 후 선택 해제
            IsPlayerTurn = false;
            StartCoroutine(ProcessNextAction());
        }


        public void GenerateStepQueue()
        {
            int agilityPenaltyPerAction = 10;

            actionQueue.Clear();
            List<UnitActionData> actions = new List<UnitActionData>();

            foreach (var p in playerUnits)
            {
                actions.Add(new UnitActionData(p, p.stats.Agility, true));
            }


            // 적 유닛
            if (currentEnemy != null && !currentEnemy.stats.IsDead)
            {
                // TODO 유닛을 타겟팅할 수 있도록 파라미터 추가 필요
                var full = CalculateAttackTiles(null, "front_2");
                var reduced = CalculateAttackTiles(null, "back_2");

                actions.Add(new UnitActionData(currentEnemy, currentEnemy.stats.Agility, false, full));
            }

            var sorted = actions.OrderByDescending(a => a.effectiveAgility);
            foreach (var act in sorted)
                actionQueue.Enqueue(act);

            Debug.Log("✅ 행동 큐 생성 완료:");
            foreach (var a in actionQueue)
                Debug.Log($" - {(a.isAlly ? "Player" : "Enemy")} / 민첩: {a.effectiveAgility}");
        }

        private IEnumerator StartStep()
        {
            Debug.Log("▶ 스텝 시작");
            currentTurnNo = 1;

            GenerateStepQueue();
            GenerateTurnUIQueue(actionQueue.ToList());

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ProcessNextAction());
        }

        private IEnumerator ProcessNextAction()
        {
            if (actionQueue.Count == 0)
            {
                Debug.Log("✅ 스텝 종료 → 다음 스텝 시작");
                yield return StartCoroutine(StartStep());
                yield break;
            }


            // ClearAllPreviewTiles();
            // DeselectEnemy(); // ✅ 새 턴 시작 전 항상 초기화
            RemoveLeftmostTurnIcon(currentTurnNo == 1);
            isProcessingTurn = true;
            currentTurnNo++;

            currentAction = actionQueue.Dequeue();
            Debug.Log($"🎯 현재 턴: {(currentAction.isAlly ? "플레이어" : "적")} (Agility: {currentAction.effectiveAgility})");

            yield return new WaitForSeconds(0.3f);

            if (currentAction.isAlly)
            {
                IsPlayerTurn = true;

                int selectedIndex = playerUnits.IndexOf((PlayerUnit)currentAction.unit);
                if (selectedIndex >= 0)
                    SelectPlayer(selectedIndex);  // selectedPlayerIndex를 내부에서 설정하도록
                else
                    Debug.LogWarning("⚠️ 선택된 유닛이 playerUnits 리스트에 없음");
            }
            else
            {
                IsPlayerTurn = false;
                EnemyUnit enemy = (EnemyUnit)currentAction.unit;

                if (!enemy.stats.IsDead && currentAction.attackTiles != null)
                {
                    // 1. 공격 타일 표시
                    foreach (var tile in currentAction.attackTiles)
                        tile.Highlight(Color.red);

                    yield return new WaitForSeconds(0.5f);

                    // 2. 데미지 적용 (여러 유닛에)
                    foreach (var player in playerUnits)
                    {
                        if (player == null || player.stats.IsDead) continue;

                        if (currentAction.attackTiles.Contains(player.CurrentTile))
                        {
                            StartCoroutine(player.FlashRed());
                            yield return new WaitForSeconds(0.2f);

                            player.ReceiveAttack(enemy.stats.Attack);
                        }
                    }

                    // 3. 타일 원래대로
                    foreach (var tile in currentAction.attackTiles)
                        tile.ResetHighlight();
                }

                yield return new WaitForSeconds(0.3f);
                StartCoroutine(ProcessNextAction());
            }
        }



        private IEnumerator ExecuteEnemyAttack(EnemyUnit enemy)
        {
            Debug.Log("💀 적 턴 시작");

            var attackTiles = enemy.GetAttackTiles(SelectedPlayer.CurrentTile, allTiles);
            foreach (var tile in attackTiles)
                tile.Highlight(Color.red);

            yield return new WaitForSeconds(0.5f);

            if (attackTiles.Contains(SelectedPlayer.CurrentTile))
            {
                StartCoroutine(SelectedPlayer.FlashRed());
                yield return new WaitForSeconds(0.2f);
                SelectedPlayer.ReceiveAttack(enemy.stats.Attack);
            }

            foreach (var tile in attackTiles)
                tile.ResetHighlight();

            yield return new WaitForSeconds(0.3f);
        }

        public void GenerateTurnUIQueue(List<UnitActionData> orderedActions)
        {
            Debug.Log($"[UI] 🔁 턴 UI 아이콘 {orderedActions.Count}개 생성 시도");

            foreach (var icon in spawnedTurnIcons)
                Destroy(icon);
            spawnedTurnIcons.Clear();

            float spacing = 120f;

            for (int i = 0; i < orderedActions.Count; i++)
            {
                var data = orderedActions[i];
                GameObject prefab = data.isAlly ? turnInfoPrefabPlayer : turnInfoPrefabEnemy;
                GameObject icon = Instantiate(prefab, turnInfoPanelParent);
                icon.GetComponent<TurnInfoIcon>().actionData = data;
                RectTransform rt = icon.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.anchoredPosition = new Vector2(i * spacing, 0);
                spawnedTurnIcons.Add(icon);
                Debug.Log($"[UI] + 아이콘 추가됨: {(data.isAlly ? "Player" : "Enemy")} / 민첩: {data.effectiveAgility}");
            }

            Debug.Log($"[UI] ✅ 총 {spawnedTurnIcons.Count}개의 아이콘이 UI에 추가됨");
        }

        public void RemoveLeftmostTurnIcon(bool isFirstTurn)
        {
            if (spawnedTurnIcons.Count == 0)
                return;

            if (!isFirstTurn)
            {
                GameObject removed = spawnedTurnIcons[0];
                spawnedTurnIcons.RemoveAt(0);
                Destroy(removed);
                Debug.Log("[UI] 🔻 좌측 아이콘 제거됨");
            }

            RepositionTurnIcons();
        }

        private void RepositionTurnIcons()
        {
            float spacing = 120f;

            for (int i = 0; i < spawnedTurnIcons.Count; i++)
            {
                RectTransform rt = spawnedTurnIcons[i].GetComponent<RectTransform>();
                if (rt == null) continue;
                rt.anchoredPosition = new Vector2(i * spacing, 0);
            }

            Debug.Log("[UI] 🔄 아이콘 재정렬 완료");
        }

        public List<HexTile> CalculateAttackTiles(HexTile playerTile, string pattern)
        {
            List<HexTile> result = new List<HexTile>();

            if (pattern == "front_2")
            {
                // tileX = 4, 5 전체 줄 공격
                foreach (var tile in allTiles)
                {
                    if (tile.tileX == 4 || tile.tileX == 5)
                    {
                        result.Add(tile);
                    }
                }
            }
            else if (pattern == "back_2")
            {
                // tileX = 1, 2 전체 줄 공격
                foreach (var tile in allTiles)
                {
                    if (tile.tileX == 1 || tile.tileX == 2)
                    {
                        result.Add(tile);
                    }
                }
            }

            return result;
        }

        public void PreviewUnitActionRange(UnitActionData action)
        {
            ClearAttackTiles();

            if (action != null && action.attackTiles != null)
            {
                foreach (var tile in action.attackTiles)
                {
                    tile.Highlight(Color.red);
                    attackPreviewTiles.Add(tile);
                }
            }
        }


        private bool IsAdjacent(HexTile from, HexTile to)
        {
            int dx = to.tileX - from.tileX;
            int dy = to.tileY - from.tileY;

            // 짝수줄 vs 홀수줄 분기
            (int dx, int dy)[] offsets = (from.tileX % 2 == 0)
                ? new (int, int)[] { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) }  // Even column
                : new (int, int)[] { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) }; // Odd column

            foreach (var offset in offsets)
            {
                if (dx == offset.dx && dy == offset.dy)
                    return true;
            }

            return false;
        }

        private void HighlightMoveableTiles(PlayerUnit unit)
        {
            ClearMoveTiles();

            foreach (HexTile tile in allTiles)
            {
                if (!tile.IsOccupied() && IsAdjacent(unit.CurrentTile, tile))
                {
                    tile.Highlight(new Color(0.5f, 0.8f, 1f)); // 연파랑
                    movePreviewTiles.Add(tile);
                }
            }
        }

        private void ClearMoveTiles()
        {
            foreach (var tile in movePreviewTiles)
                tile.ResetHighlight();
            movePreviewTiles.Clear();
        }

        private void ClearAttackTiles()
        {
            foreach (var tile in attackPreviewTiles)
                tile.ResetHighlight();
            attackPreviewTiles.Clear();
        }

        private void ClearAllPreviewTiles()
        {
            ClearMoveTiles();
            ClearAttackTiles();
        }

        public void OnEnemyIconClicked(UnitActionData action)
        {
            ClearAttackTiles();
            PreviewUnitActionRange(action);
        }

        public void OnEnemyIconUnclicked()
        {
            ClearAttackTiles();
            HighlightMoveableTiles(SelectedPlayer);
        }

        public void OnEnemyUnitClicked(EnemyUnit enemy)
        {
            // 이미 선택된 적을 다시 클릭하면 해제
            if (selectedEnemy == enemy)
            {
                DeselectEnemy();
                return;
            }

            // 기존 적 선택 해제
            if (selectedEnemy != null)
            {
                selectedEnemy.ShowSelected(false);
            }

            selectedEnemy = enemy;
            selectedEnemy.ShowSelected(true);

            // 👉 필요 시 범위 표시 등 추가
            // PreviewUnitActionRange(...)
        }

        private void DeselectEnemy()
        {
            if (selectedEnemy != null)
            {
                selectedEnemy.ShowSelected(false);
                selectedEnemy = null;
            }
        }


    }
}
