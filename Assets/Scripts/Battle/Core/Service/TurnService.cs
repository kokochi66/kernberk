using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Battle.Data;
using Battle.Units;
using Battle.Core.Manager;
using Battle.UIEvents;

namespace Battle.Core.Service
{
    /// <summary>
    /// 턴 순서 및 전투 흐름을 제어하는 서비스 클래스입니다.
    /// 연출, 유닛 처리, UI는 하위 계층에서 담당하며 이 클래스는 전투 흐름 중심의 로직을 담당합니다.
    /// </summary>
    public class TurnService : MonoBehaviour
    {
        public static TurnService Instance;

        [SerializeField] private BattleSetupData setupData;
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private GameObject defeatScreen;

        private bool isBattleActive = false;


        private Queue<UnitActionData> actionQueue = new Queue<UnitActionData>();
        public UnitActionData currentAction;
        private int currentTurnNo = 0;
        private UnitActionData selectedAction;


        public event System.Action<UnitActionData> OnTurnStarted;
        public event System.Action OnStepEnded;

        private void Start()
        {
            InitializeBattle();
            StartNewStep();
            OnStepEnded += HandleStepEnded;
        }



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeBattle()
        {
            List<HexTile> tiles = TileManager.Instance.GetAllTiles();
            UnitManager.Instance.InitializeUnits(setupData, tiles);
            isBattleActive = true;
            Debug.Log("[TurnService] 전투 초기화 완료");
        }



        public void StartNewStep()
        {
            Debug.Log("🔁 [TurnService] 새로운 스텝 시작");

            actionQueue.Clear();
            currentTurnNo = 1;

            var actions = GenerateActionQueue();
            var sorted = actions.OrderByDescending(a => a.effectiveAgility);

            foreach (Transform child in UnitManager.Instance.TurnOrderPanel)
                Destroy(child.gameObject);

            foreach (var act in sorted)
            {
                actionQueue.Enqueue(act);
                Debug.Log($"📥 큐에 추가: {(act.isAlly ? "아군" : "적군")} / 민첩: {act.effectiveAgility}");

                GameObject iconObj = Instantiate(
                    act.isAlly ? act.playerUnit.TurnInfoPrefab : act.enemyUnit.TurnInfoPrefab,
                    UnitManager.Instance.TurnOrderPanel
                );

                TurnInfoIcon icon = iconObj.GetComponent<TurnInfoIcon>();
                icon.actionData = act;

                iconObj.name = $"TurnInfoIcon_{(act.isAlly ? act.playerUnit.unitName : act.enemyUnit.unitName)}";
            }

            Debug.Log($"✅ 총 {actionQueue.Count}개의 액션이 큐에 등록됨");

            StartCoroutine(ProcessNextAction());
        }

        private List<UnitActionData> GenerateActionQueue()
        {
            List<UnitActionData> actions = new();

            var allTiles = TileManager.Instance.GetAllTiles();

            foreach (var p in UnitManager.Instance.GetPlayerUnits())
            {
                if (!p.stats.IsDead)
                    actions.Add(new UnitActionData(p, p.stats.Agility, true));
            }

            foreach (var e in UnitManager.Instance.GetEnemyUnits())
            {
                if (!e.stats.IsDead)
                {
                    List<HexTile> attackTiles = allTiles
                        .Where(tile => tile.tileX == 1 || tile.tileX == 2)
                        .ToList();

                    actions.Add(new UnitActionData(e, e.stats.Agility, false, attackTiles));

                    Debug.Log($"[TurnService] 적 액션 등록: {e.unitName} / 타일 수: {attackTiles.Count}");
                }
            }

            Debug.Log($"⚙️ [TurnService] 액션 큐 {actions.Count}개 생성됨");
            return actions;
        }


        private IEnumerator ProcessNextAction()
        {
            if (actionQueue.Count == 0)
            {
                Debug.Log("🛑 [TurnService] 스텝 종료 - 모든 유닛의 턴이 종료됨");
                OnStepEnded?.Invoke();
                yield break;
            }

            currentTurnNo++;
            currentAction = actionQueue.Dequeue();

            Debug.Log($"🎯 [TurnService] 턴 시작 - {(currentAction.isAlly ? "아군" : "적군")} / 민첩: {currentAction.effectiveAgility}");

            yield return new WaitForSeconds(0.2f);
            OnTurnStarted?.Invoke(currentAction);

            if (currentAction.isAlly)
            {
                UnitManager.Instance.SelectPlayer(currentAction.playerUnit);
                TileManager.Instance.HighlightPlayerMoveRange(currentAction.playerUnit); // ✅ 추가 필요
                SkillManager.Instance.ShowSkills(currentAction.playerUnit);
            }
            else
            {
                ExecuteEnemyTurn(currentAction);
            }
        }


        public void EndCurrentTurn()
        {
            Debug.Log("➡️ [TurnService] 현재 턴 종료 → 다음 턴으로 진행");

            if (UnitManager.Instance.TurnOrderPanel.childCount > 0)
            {
                Transform firstIcon = UnitManager.Instance.TurnOrderPanel.GetChild(0);
                Destroy(firstIcon.gameObject);
            }

            TileManager.Instance.ClearAllHighlights();
            DeselectAction();                        // ✅ 추가
            UnitManager.Instance.DeselectAllUnits(); // ✅ 유닛 선택 해제
            SkillManager.Instance.DeselectSkill();

            StartCoroutine(ProcessNextAction());
        }

        public void MoveSelectedPlayerTo(HexTile tile)
        {
            if (!currentAction.isAlly) return;
            if (!UnitManager.Instance.CanMoveTo(tile)) return;
            currentAction.playerUnit.MoveTo(tile, () => EndCurrentTurn());
        }

        public void ExecutePlayerSkill()
        {
            if (!currentAction.isAlly)
            {
                Debug.LogWarning("[TurnService] 현재 턴은 플레이어 턴이 아닙니다.");
                return;
            }

            var attacker = currentAction.playerUnit;
            var target = UnitManager.Instance.GetSelectedEnemyUnit();

            if (target == null)
            {
                Debug.LogWarning("[TurnService] 선택된 적 유닛이 없습니다.");
                return;
            }

            SkillManager.Instance.UseSkillOn(
                attacker,
                target,
                onComplete: () => EndCurrentTurn(),
                onSkillFailed: () =>
                {
                    TileManager.Instance.HighlightPlayerMoveRange(attacker); // 실패시 이동 범위 복구
                }
            );
        }


        public void ExecuteEnemyTurn(UnitActionData action)
        {
            if (action.enemyUnit.stats.IsDead)
            {
                EndCurrentTurn();
                return;
            }

            StartCoroutine(EnemyAttackRoutine(action));
        }

        private IEnumerator EnemyAttackRoutine(UnitActionData action)
        {
            var attackTiles = action.attackTiles;

            foreach (var tile in attackTiles)
                tile.SetState(HexTileState.EnemyAttackPreview);

            yield return new WaitForSeconds(0.5f);

            foreach (var player in UnitManager.Instance.GetAlivePlayers())
            {
                if (attackTiles.Contains(player.CurrentTile))
                    player.ReceiveAttack(action.enemyUnit.stats.Attack);
            }

            foreach (var tile in attackTiles)
                tile.ResetState();

            EndCurrentTurn();


        }

        public void SelectAction(UnitActionData action)
        {
            DeselectAction(); // 이전 액션 해제
            selectedAction = action;

            Debug.Log($"[TurnService] 액션 선택됨: {(action.isAlly ? "아군" : "적군")} / 민첩 {action.effectiveAgility}");

            // ✅ 적일 경우 공격 범위 표시
            if (!action.isAlly && action.attackTiles != null)
            {
                TileManager.Instance.HighlightEnemyAttackPreview(action.attackTiles);
                Debug.Log($"[TurnService] 적 공격 범위 하이라이트 표시 (타일 {action.attackTiles.Count}개)");
            }
        }


        public void DeselectAction()
        {
            if (selectedAction != null)
            {
                Debug.Log("[TurnService] 액션 선택 해제");

                if (!selectedAction.isAlly && selectedAction.attackTiles != null)
                {
                    TileManager.Instance.PopHighlightLayer(); // ✅ 공격 범위 제거
                }

                selectedAction = null;
            }
        }




        private void HandleStepEnded()
        {
            if (!isBattleActive) return;

            if (UnitManager.Instance.AllEnemiesDefeated())
            {
                EndBattle(true);
                return;
            }

            if (UnitManager.Instance.AllPlayersDefeated())
            {
                EndBattle(false);
                return;
            }

            StartNewStep();
        }

        private void EndBattle(bool playerWon)
        {
            isBattleActive = false;
            Debug.Log(playerWon ? "🎉 승리!" : "💀 패배!");

            if (playerWon)
                victoryScreen?.SetActive(true);
            else
                defeatScreen?.SetActive(true);
        }



    }

}
