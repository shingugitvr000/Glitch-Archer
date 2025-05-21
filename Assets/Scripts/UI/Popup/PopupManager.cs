using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private PopupView popupViewPrefab;

    private static PopupManager instance;
    private PopupPresenter popupPresenter;
    private PopupView popupView;

    public static PopupManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PopupManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("PopupManager");
                    instance = obj.AddComponent<PopupManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 팝업 뷰 생성 및 초기화
        if (popupViewPrefab != null)
        {
            popupView = Instantiate(popupViewPrefab, transform);
            popupPresenter = new PopupPresenter(popupView);
        }
    }

    // 팝업 표시를 위한 공개 메서드
    public void ShowPopup(string title, string message, Action onConfirm = null)
    {
        popupPresenter.ShowPopup(title, message, true, false, onConfirm, null);
    }

    // 확인/취소 버튼이 있는 팝업
    public void ShowConfirmPopup(string title, string message,
                                Action onConfirm = null, Action onCancel = null)
    {
        popupPresenter.ShowPopup(title, message, true, true, onConfirm, onCancel);
    }

    private void OnDestroy()
    {
        if (popupPresenter != null)
        {
            popupPresenter.Cleanup();
        }
    }
}
// 사용 예시
////public void ShowSimplePopup()
////{
////    // 단순 알림 팝업
////    PopupManager.Instance.ShowPopup(
////        "알림",
////        "게임이 저장되었습니다.",
////        () => Debug.Log("확인 버튼 클릭됨")
////    );
////}

////public void ShowConfirmPopup()
////{
////    // 확인/취소 선택 팝업
////    PopupManager.Instance.ShowConfirmPopup(
////        "경고",
////        "정말로 게임을 종료하시겠습니까?",
////        () => Debug.Log("게임 종료 선택"),
////        () => Debug.Log("취소 선택")
////    );
////}