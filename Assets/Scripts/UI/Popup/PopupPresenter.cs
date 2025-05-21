using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PopupPresenter
{
    private readonly IPopupView view;
    private readonly PopupModel model;

    // �ݹ� ����� �ʵ�
    private Action onConfirmCallback;
    private Action onCancelCallback;

    public PopupPresenter(IPopupView view)
    {
        this.view = view;
        this.model = new PopupModel();

        // View �̺�Ʈ ������ ���
        this.view.OnConfirmClicked += HandleConfirm;
        this.view.OnCancelClicked += HandleCancel;
        this.view.OnCloseClicked += HandleClose;
    }

    // �˾� ǥ�� �޼���
    public void ShowPopup(string title, string message, bool hasConfirm = true,
                         bool hasCancel = true, Action onConfirm = null, Action onCancel = null)
    {
        // Model ������Ʈ
        model.Title = title;
        model.Message = message;
        model.HasConfirmButton = hasConfirm;
        model.HasCancelButton = hasCancel;

        // �ݹ� ����
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;

        // View ������Ʈ
        UpdateView();

        // �˾� ǥ��
        view.Show();
    }

    // �˾� ���� �޼���
    public void HidePopup()
    {
        view.Hide();
    }

    // View ������Ʈ
    private void UpdateView()
    {
        view.SetTitle(model.Title);
        view.SetMessage(model.Message);
        view.SetConfirmButtonVisible(model.HasConfirmButton);
        view.SetCancelButtonVisible(model.HasCancelButton);
    }

    // �̺�Ʈ �ڵ鷯
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

    // ���ҽ� ����
    public void Cleanup()
    {
        view.OnConfirmClicked -= HandleConfirm;
        view.OnCancelClicked -= HandleCancel;
        view.OnCloseClicked -= HandleClose;
    }
}