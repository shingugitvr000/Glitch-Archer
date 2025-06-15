using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float lifetime = 2f;           // 표시 시간
    public float moveSpeed = 2f;          // 위로 올라가는 속도
    public float fadeSpeed = 2f;          // 페이드 아웃 속도

    private TextMeshProUGUI damageText;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Camera playerCamera;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 메인 카메라 찾기
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
    }

    private void Start()
    {
        // 수명 후 자동 삭제
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // 위로 올라가는 애니메이션
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * 100f * Time.deltaTime;

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
        damageText.text = Mathf.RoundToInt(damage).ToString();

        // 크리티컬과 폭발에 따른 색상 및 크기 설정
        if (isCritical && isExplosion)
        {
            // 크리티컬 + 폭발
            damageText.color = Color.magenta;
            damageText.fontSize = 36;
            damageText.text = "CRIT " + damageText.text + "!";
            damageText.fontStyle = FontStyles.Bold;
        }
        else if (isCritical)
        {
            // 크리티컬만
            damageText.color = Color.red;
            damageText.fontSize = 32;
            damageText.text = "CRIT " + damageText.text;
            damageText.fontStyle = FontStyles.Bold;
        }
        else if (isExplosion)
        {
            // 폭발만
            damageText.color = Color.yellow;
            damageText.fontSize = 24;
            damageText.text = damageText.text + " BOOM";
        }
        else
        {
            // 일반 데미지
            damageText.color = Color.white;
            damageText.fontSize = 20;
            damageText.fontStyle = FontStyles.Normal;
        }

        // 약간의 랜덤 위치 오프셋 (겹치지 않게)
        float randomX = Random.Range(-50f, 50f);
        float randomY = Random.Range(-20f, 20f);
        rectTransform.anchoredPosition += new Vector2(randomX, randomY);
    }
}