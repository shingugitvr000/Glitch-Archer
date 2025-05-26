using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PopupView : MonoBehaviour, IPopupView
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject popupPanel;

    // 이벤트 선언
    public event Action OnConfirmClicked;
    public event Action OnCancelClicked;
    public event Action OnCloseClicked;

    private void Awake()
    {
        // 버튼 리스너 설정
        confirmButton.onClick.AddListener(() => OnConfirmClicked?.Invoke());
        cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
        //closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

        // 초기 상태는 숨김
        popupPanel.SetActive(false);
    }

    // IPopupView 인터페이스 구현
    public void Show()
    {
        popupPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;             //커서 사용을 위해 임시로 잠금 해제
    }

    public void Hide()
    {
        popupPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    public void SetConfirmButtonVisible(bool isVisible)
    {
        confirmButton.gameObject.SetActive(isVisible);
    }

    public void SetCancelButtonVisible(bool isVisible)
    {
        cancelButton.gameObject.SetActive(isVisible);
    }

   //두 버튼 동시에 설정
    public void SetButtonsVisible(bool confirmIsVisible, bool cancleIsVisible)
    {
        confirmButton.gameObject.SetActive(confirmIsVisible);
        cancelButton.gameObject.SetActive(cancleIsVisible);

        RectTransform rect = confirmButton.GetComponent<RectTransform>();   

        if (confirmIsVisible && !cancleIsVisible)                                //확인 버튼 하나만 필요한 경우
        {
            rect.anchoredPosition = new Vector2(0f, -95f);                          //확인 버튼 위치, 크기 조절
            rect.sizeDelta = new Vector2(270f, 50f);
        }
        else
        {
            rect.anchoredPosition = new Vector2(95f, -95f);                          //확인 버튼 위치, 크기 조절
            rect.sizeDelta = new Vector2(160f, 50f);
        }
    }

    private void OnDestroy()
    {
        // 버튼 리스너 제거
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        //closeButton.onClick.RemoveAllListeners();
    }
}
