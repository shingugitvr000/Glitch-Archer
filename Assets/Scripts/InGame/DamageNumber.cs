using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float lifetime = 2f;           // 표시 시간
    public float moveSpeed = 80f;         // 위로 올라가는 속도
    public float fadeSpeed = 1.5f;        // 페이드 아웃 속도

    private TextMeshProUGUI damageText;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // 수명 후 자동 삭제
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // 위로 천천히 올라가는 애니메이션
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        // 페이드 아웃
        if (canvasGroup != null)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
        }
    }

    // 데미지 설정 (외부에서 호출)
    public void SetDamage(float damage, bool isCritical = false, bool isExplosion = false)
    {
        if (damageText == null) return;

        // 데미지 텍스트 설정
        string damageString = Mathf.RoundToInt(damage).ToString();

        // 크리티컬과 폭발에 따른 색상 및 크기 설정
        if (isCritical && isExplosion)
        {
            // 크리티컬 + 폭발 - 최고 임팩트
            damageText.color = new Color(1f, 0.2f, 0.8f); // 핫핑크
            damageText.fontSize = 50;
            damageText.text = $"CRITICAL {damageString}";
            damageText.fontStyle = FontStyles.Bold | FontStyles.Italic;

            // 두꺼운 외곽선
            damageText.outlineWidth = 0.4f;
            damageText.outlineColor = Color.black;

            // 극강 펀치 애니메이션
            StartCoroutine(MegaPunchEffect());
        }
        else if (isCritical)
        {
            // 크리티컬만 - 강렬한 빨강
            damageText.color = new Color(1f, 0.1f, 0.1f); // 진한 빨강
            damageText.fontSize = 42;
            damageText.text = $"CRITICAL {damageString}";
            damageText.fontStyle = FontStyles.Bold | FontStyles.Italic;

            // 외곽선
            damageText.outlineWidth = 0.3f;
            damageText.outlineColor = Color.black;

            // 크리티컬 펀치 애니메이션
            StartCoroutine(CriticalPunchEffect());
        }
        else if (isExplosion)
        {
            // 폭발만 - 오렌지
            damageText.color = new Color(1f, 0.5f, 0f); // 오렌지
            damageText.fontSize = 36;
            damageText.text = $"{damageString} EXPLOSION";
            damageText.fontStyle = FontStyles.Bold;

            // 외곽선
            damageText.outlineWidth = 0.25f;
            damageText.outlineColor = Color.black;

            // 폭발 펀치 애니메이션
            StartCoroutine(ExplosionPunchEffect());
        }
        else
        {
            // 일반 데미지 - 데미지 크기에 따라 색상과 크기 조절
            SetNormalDamageStyle(damage);

            // 일반 펀치 애니메이션
            StartCoroutine(NormalPunchEffect());
        }

        // 약간의 랜덤 위치 오프셋 (겹치지 않게)
        float randomX = Random.Range(-40f, 40f);
        float randomY = Random.Range(-15f, 15f);
        rectTransform.anchoredPosition += new Vector2(randomX, randomY);
    }

    // ★ 일반 데미지 스타일 (데미지 크기에 따라 차별화)
    private void SetNormalDamageStyle(float damage)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();
        damageText.fontStyle = FontStyles.Bold;

        if (damage >= 100f)
        {
            // 고데미지 - 노란색, 큰 크기
            damageText.color = new Color(1f, 1f, 0.2f); // 밝은 노랑
            damageText.fontSize = 32;
            damageText.outlineWidth = 0.25f;
            damageText.outlineColor = Color.black;
        }
        else if (damage >= 50f)
        {
            // 중데미지 - 주황색, 중간 크기
            damageText.color = new Color(1f, 0.7f, 0.2f); // 주황
            damageText.fontSize = 28;
            damageText.outlineWidth = 0.2f;
            damageText.outlineColor = Color.black;
        }
        else if (damage >= 25f)
        {
            // 보통데미지 - 흰색, 기본 크기
            damageText.color = Color.white;
            damageText.fontSize = 24;
            damageText.outlineWidth = 0.15f;
            damageText.outlineColor = Color.black;
        }
        else
        {
            // 저데미지 - 회색, 작은 크기
            damageText.color = new Color(0.8f, 0.8f, 0.8f); // 연한 회색
            damageText.fontSize = 20;
            damageText.outlineWidth = 0.1f;
            damageText.outlineColor = Color.black;
        }
    }

    // ★ 극강 펀치 효과 (크리티컬 + 폭발)
    private System.Collections.IEnumerator MegaPunchEffect()
    {
        Vector3 targetScale = Vector3.one;

        // 1단계: 강력한 임팩트 (0.1초)
        rectTransform.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;

            // 폭발적으로 커지기 (오버스케일)
            float scale = Mathf.Lerp(0f, 2.2f, EaseOutBack(t));
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        // 2단계: 안정화 + 진동 (0.3초)
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;

            // 크기 안정화하면서 진동
            float baseScale = Mathf.Lerp(2.2f, 1.4f, t);
            float vibration = Mathf.Sin(elapsed * 40f) * 0.1f * (1f - t);
            rectTransform.localScale = Vector3.one * (baseScale + vibration);

            yield return null;
        }

        // 최종 크기
        rectTransform.localScale = targetScale * 1.4f;
    }

    // ★ 크리티컬 펀치 효과
    private System.Collections.IEnumerator CriticalPunchEffect()
    {
        Vector3 targetScale = Vector3.one;

        // 1단계: 빠른 펀치 (0.08초)
        rectTransform.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < 0.08f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.08f;

            // 빠르게 커지기
            float scale = Mathf.Lerp(0f, 1.8f, EaseOutBack(t));
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        // 2단계: 바운스 안정화 (0.2초)
        elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.2f;

            float scale = Mathf.Lerp(1.8f, 1.2f, EaseOutBounce(t));
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        rectTransform.localScale = targetScale * 1.2f;
    }

    // ★ 폭발 펀치 효과
    private System.Collections.IEnumerator ExplosionPunchEffect()
    {
        Vector3 targetScale = Vector3.one;

        // 1단계: 확산 펀치 (0.12초)
        rectTransform.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < 0.12f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.12f;

            float scale = Mathf.Lerp(0f, 1.6f, EaseOutCubic(t));
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        // 2단계: 약간 수축 (0.1초)
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;

            float scale = Mathf.Lerp(1.6f, 1.1f, t);
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        rectTransform.localScale = targetScale * 1.1f;
    }

    // ★ 일반 펀치 효과 (데미지 크기에 따라 차별화)
    private System.Collections.IEnumerator NormalPunchEffect()
    {
        Vector3 targetScale = Vector3.one;
        float maxScale = 1.0f;

        // 폰트 크기에 따라 최대 스케일 조절
        if (damageText.fontSize >= 32) maxScale = 1.3f;      // 고데미지
        else if (damageText.fontSize >= 28) maxScale = 1.2f; // 중데미지  
        else if (damageText.fontSize >= 24) maxScale = 1.1f; // 보통데미지
        else maxScale = 1.0f;                               // 저데미지

        // 심플한 펀치 효과
        rectTransform.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = Mathf.Lerp(0f, maxScale, EaseOutBack(t));
            rectTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        rectTransform.localScale = targetScale * maxScale;
    }

    // ★ 이징 함수들
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        }
        else
        {
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}