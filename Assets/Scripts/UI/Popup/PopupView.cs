using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupView : MonoBehaviour, IPopupView
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text messageText;
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
        closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

        // 초기 상태는 숨김
        popupPanel.SetActive(false);
    }

    // IPopupView 인터페이스 구현
    public void Show()
    {
        popupPanel.SetActive(true);
    }

    public void Hide()
    {
        popupPanel.SetActive(false);
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

    private void OnDestroy()
    {
        // 버튼 리스너 제거
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }
}
