using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Battle.Data;
using Battle.Units;
using Battle.Core.Manager;
using Battle.UIEvents;
using Battle.Skill;

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

            foreach (Transform child in UnitManager.Instance.TurnOrderPanel)
                Destroy(child.gameObject);

            List<UnitActionData> actions = GenerateActionQueue()
                .OrderByDescending(a => a.effectiveAgility)
                .ToList();

            StartCoroutine(ShowTurnIconsAndStart(actions));
        }

        private IEnumerator ShowTurnIconsAndStart(List<UnitActionData> sortedActions)
        {
            foreach (var act in sortedActions)
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

                yield return new WaitForSeconds(0.3f); // ✅ 간격 연출
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
                var unit = currentAction.playerUnit;

                // ✅ 기절 상태라면 턴 스킵
                if (unit.IsStunned())
                {
                    Debug.Log($"⏭️ {unit.unitName} 기절 상태로 턴 스킵");
                    UIDescriptionPanel.Instance.Show($"{unit.unitName}은(는) 기절 상태입니다. 턴을 건너뜁니다.");

                    yield return new WaitForSeconds(2f);

                    UIDescriptionPanel.Instance.Clear();
                    unit.ApplyRecovery(); // ✅ 회복 상태로 전환
                    EndCurrentTurn();
                    yield break;
                }


                UnitManager.Instance.SelectPlayer(unit);
                TileManager.Instance.HighlightPlayerMoveRange(unit);
                SkillManager.Instance.ShowSkills(unit);
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
            var unit = currentAction.playerUnit;

            if (!UnitManager.Instance.CanMoveTo(tile)) return;

            var skill = SkillManager.Instance.SelectedSkillData;
            bool usedMoveSkill = skill != null && skill.skillType == SkillType.IncreaseMoveRange;

            unit.MoveTo(tile, () =>
            {
                if (usedMoveSkill)
                {
                    if (unit.SkillPoint >= skill.spCost)
                    {
                        unit.UseSkillPoint(skill.spCost);
                        unit.ClearBoostedMoveRange();
                        SkillManager.Instance.DeselectSkill();

                        Debug.Log($"🌀 {unit.unitName} 이동거리 증가 스킬 사용: SP {skill.spCost} 소모");
                    }
                }

                EndCurrentTurn();
            });
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

            // ✅ 스텝 연출 Coroutine 실행
            StartCoroutine(HandleStepTransition());
        }

        private IEnumerator HandleStepTransition()
        {
            yield return new WaitForSeconds(0.5f); // 턴 종료 후 잠깐 대기

            UIDescriptionPanel.Instance.Show("🔁 새로운 Step을 시작합니다.");

            yield return new WaitForSeconds(1f); // 안내 메시지 표시 시간

            UIDescriptionPanel.Instance.Clear();

            // ✅ 모든 플레이어 유닛 SP 회복
            foreach (var unit in UnitManager.Instance.GetAlivePlayers())
            {
                unit.GainSkillPoint(15);
                Debug.Log($"🌀 {unit.unitName} 스텝 종료 후 SP +15 → 현재 SP: {unit.SkillPoint}");

                if (unit.IsRecovered())
                {
                    unit.RecoverStatus();
                    Debug.Log($"🔄 {unit.unitName}의 회복 상태가 초기화되었습니다.");
                }
            }

            StartNewStep(); // 스텝 본격 시작
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

        public void OnSkillSlotClicked(UISkillSlot skillSlot)
        {
            var skill = skillSlot.skillData;

            // ✅ 스킬이 이미 선택된 상태에서 다시 클릭해 해제하려는 경우
            bool isCancelling = SkillManager.Instance.SelectedSkillData == skill;

            if (isCancelling)
            {
                SkillManager.Instance.DeselectSkill();

                // 이동 거리 증가 스킬이었다면 롤백
                if (skill.skillType == SkillType.IncreaseMoveRange)
                {
                    currentAction.playerUnit.ClearBoostedMoveRange();
                    TileManager.Instance.ClearAllHighlights();
                    TileManager.Instance.HighlightPlayerMoveRange(currentAction.playerUnit);
                    Debug.Log($"↩️ {currentAction.playerUnit.unitName} 이동거리 증가 취소됨");
                }

                return;
            }

            // ✅ 새로 선택하는 경우
            SkillManager.Instance.SelectSkill(skillSlot);

            if (skill.skillType == SkillType.IncreaseMoveRange && skill.GetEffect() is ISkillEffect effect)
            {
                var user = currentAction.playerUnit;
                StartCoroutine(effect.Execute(user, null, () => { }));
            }
        }

    }

}
