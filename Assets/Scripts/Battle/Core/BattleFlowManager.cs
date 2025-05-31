using System.Collections;
using UnityEngine;

namespace Battle.Core
{
    /// <summary>
    /// 전투의 전체적인 흐름(시작, 종료, 중단 등)을 담당하는 클래스입니다.
    /// TurnManager, UnitManager, SkillManager 등과 상호작용하여 전투의 전반적인 사이클을 관리합니다.
    /// </summary>
    public class BattleFlowManager : MonoBehaviour
    {
        public static BattleFlowManager Instance;

        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private GameObject defeatScreen;

        private bool isBattleActive = false;

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
        /// 전투를 시작하는 진입점입니다.
        /// </summary>
        public void StartBattle()
        {
            isBattleActive = true;
            Debug.Log("⚔️ 전투 시작!");

            // 전투 준비 완료 시 스텝 시작
            TurnManager.Instance.OnStepEnded += HandleStepEnded;
            TurnManager.Instance.OnTurnStarted += HandleTurnStarted;

            UnitManager.Instance.InitializeUnits();
            TurnManager.Instance.StartNewStep();
        }

        /// <summary>
        /// 한 턴이 시작될 때 호출되는 핸들러입니다.
        /// </summary>
        private void HandleTurnStarted(UnitActionData action)
        {
            if (!isBattleActive) return;

            Debug.Log($"🎯 턴 시작: {action.unit.unitName} ({(action.isAlly ? "아군" : "적군")})");

            if (action.unit.stats.IsDead)
            {
                Debug.Log("☠️ 유닛이 사망하여 턴 건너뜀");
                TurnManager.Instance.EndCurrentTurn();
                return;
            }

            // 유닛별 턴 행동 시작 (플레이어나 적에 따라 분기 가능)
            if (action.isAlly)
            {
                UnitManager.Instance.SelectPlayer(action.unit);
            }
            else
            {
                StartCoroutine(UnitManager.Instance.ExecuteEnemyTurn(action.unit));
            }
        }

        /// <summary>
        /// 한 스텝이 종료되었을 때 호출되는 핸들러입니다.
        /// </summary>
        private void HandleStepEnded()
        {
            if (!isBattleActive) return;

            Debug.Log("🔁 스텝 종료됨. 다음 스텝 준비");

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

            TurnManager.Instance.StartNewStep();
        }

        /// <summary>
        /// 전투 종료 처리
        /// </summary>
        private void EndBattle(bool playerWon)
        {
            isBattleActive = false;
            Debug.Log(playerWon ? "🎉 승리!" : "💀 패배!");

            TurnManager.Instance.OnStepEnded -= HandleStepEnded;
            TurnManager.Instance.OnTurnStarted -= HandleTurnStarted;

            if (playerWon)
                victoryScreen?.SetActive(true);
            else
                defeatScreen?.SetActive(true);
        }
    }
}
