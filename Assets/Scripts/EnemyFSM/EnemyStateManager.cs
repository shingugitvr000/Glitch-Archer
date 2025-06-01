using UnityEngine;

namespace EnemyAI
{
    public class EnemyStateManager : MonoBehaviour
    {
        private EnemyFSM enemy;
        private EnemyState currentState;
        private EnemyState previousState;
        private float stateStartTime;

        public EnemyState CurrentState => currentState;
        public EnemyState PreviousState => previousState;
        public float StateTime => Time.time - stateStartTime;

        public void Initialize(EnemyFSM enemyFSM)
        {
            enemy = enemyFSM;
        }

        public void UpdateCurrentState()
        {
            currentState?.UpdateState(enemy);
        }

        public void ChangeState(EnemyState newState)
        {
            if (newState == null)
            {
                Debug.LogError($"[{enemy.name}] 새로운 상태가 null입니다!");
                return;
            }

            if (currentState == newState) return;

            // 현재 상태에서 전환 가능한지 확인
            if (currentState != null && !currentState.CanTransitionTo(newState, enemy))
            {
                if (enemy.showDebugInfo)
                    Debug.Log($"[{enemy.name}] {currentState.stateName}에서 {newState.stateName}로 전환할 수 없습니다.");
                return;
            }

            // 이전 상태 종료
            if (currentState != null)
            {
                currentState.ExitState(enemy);
                previousState = currentState;
            }

            // 새 상태 시작
            currentState = newState;
            stateStartTime = Time.time;
            currentState.EnterState(enemy);

            // 애니메이션 파라미터 설정
            currentState.SetAnimationParameters(enemy);

            // NavMesh Agent 속도 설정
            if (enemy.Agent != null)
            {
                enemy.Agent.speed = currentState.GetMoveSpeed();
                enemy.Agent.angularSpeed = currentState.GetTurnSpeed();
            }
        }

        public void ForceChangeState(EnemyState newState)
        {
            if (newState == null) return;

            // 강제 전환 (조건 무시)
            if (currentState != null)
            {
                currentState.ExitState(enemy);
                previousState = currentState;
            }

            currentState = newState;
            stateStartTime = Time.time;
            currentState.EnterState(enemy);
            currentState.SetAnimationParameters(enemy);
        }

        public void DrawCurrentStateGizmos()
        {
            currentState?.DrawGizmos(enemy);
        }

        public float GetStateTime()
        {
            return StateTime;
        }

        // 이전 상태로 돌아가기
        public void ReturnToPreviousState()
        {
            if (previousState != null)
            {
                ChangeState(previousState);
            }
        }

        // 디버그 정보
        public string GetStateInfo()
        {
            if (currentState == null) return "No State";

            return $"Current: {currentState.stateName} ({StateTime:F1}s)";
        }
    }
}