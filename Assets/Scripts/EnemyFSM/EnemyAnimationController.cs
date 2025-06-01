using UnityEngine;

namespace EnemyAI
{
    public class EnemyAnimationController : MonoBehaviour
    {
        private EnemyFSM enemy;
        private Animator animator;

        [Header("애니메이션 설정")]
        public float smoothTime = 0.1f;
        public bool useVRMCompatibility = false;

        // 현재 애니메이션 값들
        private float currentSpeed = 0f;
        private float targetSpeed = 0f;

        public void Initialize(EnemyFSM enemyFSM)
        {
            enemy = enemyFSM;
            animator = enemy.Anim;

            if (animator == null)
            {
                Debug.LogWarning($"[{enemy.name}] 애니메이터를 찾을 수 없습니다!");
            }
            else
            {
                CheckAnimatorParameters();
            }
        }

        void Update()
        {
            if (animator == null) return;

            UpdateAnimationParameters();
        }

        private void UpdateAnimationParameters()
        {
            // 속도 업데이트 (블렌드 트리가 자동으로 Idle/Walk/Sprint 처리)
            if (enemy.Agent != null)
            {
                targetSpeed = enemy.Agent.velocity.magnitude;
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, smoothTime * 10f * Time.deltaTime);

            // Speed만 설정하면 블렌드 트리가 모든 이동 애니메이션 처리
            SetFloat("Speed", currentSpeed);
        }

        // 안전한 파라미터 설정 메서드들
        public void SetFloat(string paramName, float value)
        {
            if (animator == null) return;

            try
            {
                if (HasParameter(paramName))
                {
                    animator.SetFloat(paramName, value);
                }
            }
            catch (System.Exception)
            {
                // 에러 무시
            }
        }

        public void SetBool(string paramName, bool value)
        {
            if (animator == null) return;

            try
            {
                if (HasParameter(paramName))
                {
                    animator.SetBool(paramName, value);
                }
            }
            catch (System.Exception)
            {
                // 에러 무시
            }
        }

        public void SetTrigger(string paramName)
        {
            if (animator == null) return;

            try
            {
                if (HasParameter(paramName))
                {
                    animator.SetTrigger(paramName);
                    if (enemy.showDebugInfo)
                        Debug.Log($"[{enemy.name}] 트리거 실행: {paramName}");
                }
            }
            catch (System.Exception)
            {
                // 에러 무시
            }
        }

        private bool HasParameter(string paramName)
        {
            if (animator == null) return false;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }

        private void CheckAnimatorParameters()
        {
            if (!enemy.showDebugInfo) return;

            Debug.Log($"[{enemy.name}] 애니메이터 파라미터 확인:");

            // 필수 파라미터 체크 (3개만)
            string[] requiredParams = { "Speed", "InCombat", "Fire" };

            foreach (string param in requiredParams)
            {
                bool exists = HasParameter(param);
                Debug.Log($"  - {param}: {(exists ? "✓" : "✗")}");
            }
        }

        // VRM 호환성 설정
        public void SetVRMCompatibility(bool enabled)
        {
            useVRMCompatibility = enabled;

            if (enabled)
            {
                smoothTime = 0.2f; // VRM은 더 부드럽게
                Debug.Log($"[{enemy.name}] VRM 호환성 모드 활성화");
            }
        }
    }
}