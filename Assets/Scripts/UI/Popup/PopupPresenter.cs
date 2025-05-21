using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PopupPresenter
{
    private readonly IPopupView view;
    private readonly PopupModel model;

    // 콜백 저장용 필드
    private Action onConfirmCallback;
    private Action onCancelCallback;

    public PopupPresenter(IPopupView view)
    {
        this.view = view;
        this.model = new PopupModel();

        // View 이벤트 리스너 등록
        this.view.OnConfirmClicked += HandleConfirm;
        this.view.OnCancelClicked += HandleCancel;
        this.view.OnCloseClicked += HandleClose;
    }

    // 팝업 표시 메서드
    public void ShowPopup(string title, string message, bool hasConfirm = true,
                         bool hasCancel = true, Action onConfirm = null, Action onCancel = null)
    {
        // Model 업데이트
        model.Title = title;
        model.Message = message;
        model.HasConfirmButton = hasConfirm;
        model.HasCancelButton = hasCancel;

        // 콜백 저장
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;

        // View 업데이트
        UpdateView();

        // 팝업 표시
        view.Show();
    }

    // 팝업 숨김 메서드
    public void HidePopup()
    {
        view.Hide();
    }

    // View 업데이트
    private void UpdateView()
    {
        view.SetTitle(model.Title);
        view.SetMessage(model.Message);
        view.SetConfirmButtonVisible(model.HasConfirmButton);
        view.SetCancelButtonVisible(model.HasCancelButton);
    }

    // 이벤트 핸들러
    private void HandleConfirm()
    {
        onConfirmCallback?.Invoke();
        HidePopup();
    }

    private void HandleCancel()
    {
        onCancelCallback?.Invoke();
        HidePopup();
    }

    private void HandleClose()
    {
        HidePopup();
    }

    // 리소스 정리
    public void Cleanup()
    {
        view.OnConfirmClicked -= HandleConfirm;
        view.OnCancelClicked -= HandleCancel;
        view.OnCloseClicked -= HandleClose;
    }
}