using UnityEngine;

namespace EnemyAI
{
    // 모든 적 상태의 베이스 클래스
    public abstract class EnemyState : ScriptableObject
    {
        [Header("기본 설정")]
        public string stateName = "State";
        [TextArea(2, 4)]
        public string description = "상태 설명";

        [Header("전환 설정")]
        public float minStateTime = 0.1f;       // 최소 상태 유지 시간
        public bool canInterrupt = true;        // 다른 상태로 중단 가능한지

        // 상태 진입 시 호출
        public virtual void EnterState(EnemyFSM enemy)
        {
            if (Application.isPlaying)
                Debug.Log($"[{enemy.name}] {stateName} 상태 진입");
        }

        // 매 프레임 실행
        public abstract void UpdateState(EnemyFSM enemy);

        // 상태 종료 시 호출
        public virtual void ExitState(EnemyFSM enemy)
        {
            if (Application.isPlaying)
                Debug.Log($"[{enemy.name}] {stateName} 상태 종료");
        }

        // 다른 상태로 전환 가능한지 확인
        public virtual bool CanTransitionTo(EnemyState nextState, EnemyFSM enemy)
        {
            // 최소 시간이 지났거나, 중단 가능한 상태인지 확인
            return enemy.StateManager.GetStateTime() >= minStateTime || canInterrupt;
        }

        // 상태별 애니메이션 파라미터 설정
        public virtual void SetAnimationParameters(EnemyFSM enemy)
        {
            // 기본적으로 아무것도 하지 않음 - 각 상태에서 오버라이드
        }

        // 상태별 이동 속도 반환
        public virtual float GetMoveSpeed()
        {
            return 2f; // 기본 속도
        }

        // 상태별 회전 속도 반환
        public virtual float GetTurnSpeed()
        {
            return 360; // 기본 회전 속도
        }

        // Gizmo 그리기 (에디터에서만)
        public virtual void DrawGizmos(EnemyFSM enemy)
        {
            // 기본적으로 아무것도 그리지 않음
        }
    }
}