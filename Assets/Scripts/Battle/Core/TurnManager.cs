using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Battle.Units;

namespace Battle.Core
{
    /// 
    /// 턴 순서를 관리하는 전투 전용 매니저 클래스.
    /// 각 유닛의 민첩도를 기준으로 턴 큐를 생성하고, 순차적으로 턴을 처리한다.
    /// 
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance;

        // 현재 턴에 수행할 유닛 액션들을 저장하는 큐
        private Queue<UnitActionData> actionQueue = new Queue<UnitActionData>();

        // 현재 진행 중인 유닛 액션
        public UnitActionData currentAction;

        // 현재 스텝에서 몇 번째 턴인지
        private int currentTurnNo = 0;

        /// 
        /// 유닛 턴이 시작될 때 호출됨 (BattleManager 또는 전투 로직에서 구독 가능)
        /// 
        public event System.Action<UnitActionData> OnTurnStarted;

        /// 
        /// 현재 스텝에 있는 모든 턴이 끝났을 때 호출됨
        /// 
        public event System.Action OnStepEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartNewStep()
        {
            Debug.Log("🔁 [TurnManager] 새로운 스텝 시작");

            actionQueue.Clear();
            currentTurnNo = 1;

            var actions = UnitManager.Instance.GenerateActionQueue(); // ✅ 이 한 줄로 유닛 액션 큐 준비

            var sorted = actions.OrderByDescending(a => a.effectiveAgility);
            foreach (var act in sorted)
            {
                actionQueue.Enqueue(act);
                Debug.Log($"📥 큐에 추가: {(act.isAlly ? "아군" : "적군")} / 민첩: {act.effectiveAgility}");
            }

            Debug.Log($"✅ 총 {actionQueue.Count}개의 액션이 큐에 등록됨");

            StartCoroutine(ProcessNextAction());
        }



        /// 
        /// 다음 유닛의 턴을 시작한다. 큐가 비었다면 스텝 종료를 알린다.
        /// 
        private IEnumerator ProcessNextAction()
        {
            if (actionQueue.Count == 0)
            {
                Debug.Log("🛑 [TurnManager] 스텝 종료 - 모든 유닛의 턴이 종료됨");
                OnStepEnded?.Invoke();
                yield break;
            }

            currentTurnNo++;
            currentAction = actionQueue.Dequeue();

            Debug.Log($"🎯 [TurnManager] 턴 시작 - {(currentAction.isAlly ? "아군" : "적군")} / 민첩: {currentAction.effectiveAgility}");

            yield return new WaitForSeconds(0.2f); // 부드러운 연출용 대기
            OnTurnStarted?.Invoke(currentAction);
        }

        /// 
        /// 현재 유닛의 턴을 종료하고 다음 턴을 실행한다.
        /// 외부에서 전투가 끝났거나 행동이 끝났을 때 호출해야 함.
        /// 
        public void EndCurrentTurn()
        {
            Debug.Log("➡️ [TurnManager] 현재 턴 종료 → 다음 턴으로 진행");
            StartCoroutine(ProcessNextAction());
        }
    }
}
