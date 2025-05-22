using UnityEngine;
using Battle.Units;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace Battle.Core
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;
        public PlayerUnit playerUnit;
        public HexTile[] allTiles;
        public EnemyUnit currentEnemy; // Editor에서 드래그해서 지정


        public bool IsPlayerTurn { get; private set; } = true;
        public PlayerUnit SelectedPlayer { get; private set; }
        private enum TurnState
        {
            PlayerTurn,
            EnemyTurn
        }
        private TurnState currentTurn = TurnState.PlayerTurn;
        private Queue<UnitActionData> actionQueue = new Queue<UnitActionData>();
        private UnitActionData currentAction;
        private bool isProcessingTurn = false;

        private void Awake()
        {
            // 싱글톤 패턴
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
            {
                allTiles = FindObjectsOfType<HexTile>();
            }

            // 유닛을 특정 타일에 올림
            HexTile startTile = allTiles.FirstOrDefault(t => t.tileX == 1 && t.tileY == 2); // 예시 좌표
            if (startTile != null)
            {
                playerUnit.SetCurrentTile(startTile);
                SelectPlayer(playerUnit); // 초기 선택
                StartCoroutine(StartStep());
            }
            else
            {
                Debug.LogWarning("시작 타일을 찾을 수 없습니다.");
            }
        }

        public void SelectPlayer(PlayerUnit unit)
        {
            SelectedPlayer = unit;
            Debug.Log($"[BattleManager] 유닛 선택됨: {unit.name}");
        }

        private void StartPlayerTurn()
        {
            IsPlayerTurn = true;
            Debug.Log("[BattleManager] 플레이어 턴 시작");
        }

        public void OnTileClicked(HexTile tile)
        {
            if (!IsPlayerTurn) return;
            if (SelectedPlayer == null || tile.IsOccupied()) return;

            SelectedPlayer.MoveTo(tile, () =>
            {
                IsPlayerTurn = false;
                StartCoroutine(ProcessNextAction());
            });
        }

        public void OnClickAttack()
        {
            if (!IsPlayerTurn) return;

            int atk = SelectedPlayer.stats.Attack;
            currentEnemy.ReceiveAttack(atk);

            IsPlayerTurn = false;
            StartCoroutine(ProcessNextAction());
        }


        public void EndTurn()
        {
            if (currentTurn == TurnState.PlayerTurn)
            {
                currentTurn = TurnState.EnemyTurn;
                StartCoroutine(HandleEnemyTurn());
            }
        }

        private IEnumerator HandleEnemyTurn()
        {
            Debug.Log("💀 적 턴 시작");

            // 간단히 한 번만 공격하게 처리
            if (currentEnemy != null && !currentEnemy.stats.IsDead)
            {
                yield return new WaitForSeconds(0.5f);
                SelectedPlayer.ReceiveAttack(currentEnemy.stats.Attack);
            }

            yield return new WaitForSeconds(0.5f);

            currentTurn = TurnState.PlayerTurn;
            Debug.Log("🟢 플레이어 턴 시작");
        }

        public void GenerateStepQueue()
        {

            int agilityPenaltyPerAction = 10; // 행동당 민첩도 감소량

            actionQueue.Clear();

            List<UnitActionData> actions = new List<UnitActionData>();

            // 플레이어 유닛
            actions.Add(new UnitActionData(playerUnit, playerUnit.stats.Agility, true));
            actions.Add(new UnitActionData(playerUnit, playerUnit.stats.Agility - agilityPenaltyPerAction, true));

            // 적 유닛
            if (currentEnemy != null && !currentEnemy.stats.IsDead)
            {
                actions.Add(new UnitActionData(currentEnemy, currentEnemy.stats.Agility, false));
                actions.Add(new UnitActionData(currentEnemy, currentEnemy.stats.Agility - agilityPenaltyPerAction, false));
            }

            // 민첩도 높은 순으로 정렬
            var sorted = actions.OrderByDescending(a => a.effectiveAgility);

            foreach (var act in sorted)
            {
                actionQueue.Enqueue(act);
            }

            Debug.Log("✅ 행동 큐 생성 완료:");
            foreach (var a in actionQueue)
            {
                Debug.Log($" - {(a.isAlly ? "Player" : "Enemy")} / 민첩: {a.effectiveAgility}");
            }
        }

        private IEnumerator StartStep()
        {
            Debug.Log("▶ 스텝 시작");

            GenerateStepQueue(); // 민첩 기반 큐 구성
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

            isProcessingTurn = true;

            currentAction = actionQueue.Dequeue();
            Debug.Log($"🎯 현재 턴: {(currentAction.isAlly ? "플레이어" : "적")} (Agility: {currentAction.effectiveAgility})");

            yield return new WaitForSeconds(0.3f);

            if (currentAction.isAlly)
            {
                IsPlayerTurn = true;
                SelectPlayer((PlayerUnit)currentAction.unit);

                // ⚠️ 이 시점에서 유저가 Attack/Move 버튼을 누를 수 있게 UI 열어주기
            }
            else
            {
                IsPlayerTurn = false;
                EnemyUnit enemy = (EnemyUnit)currentAction.unit;

                if (!enemy.stats.IsDead)
                {
                    yield return new WaitForSeconds(0.5f);
                    playerUnit.ReceiveAttack(enemy.stats.Attack);
                }

                yield return new WaitForSeconds(0.5f);
                StartCoroutine(ProcessNextAction());
            }
        }
    }
}
