using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IPopupView
{
    // �˾� ǥ��/����
    void Show();
    void Hide();

    // UI ������Ʈ �޼���
    void SetTitle(string title);
    void SetMessage(string message);
    void SetConfirmButtonVisible(bool isVisible);
    void SetCancelButtonVisible(bool isVisible);

    // �̺�Ʈ - UI���� �߻��ϴ� �׼�
    event Action OnConfirmClicked;
    event Action OnCancelClicked;
    event Action OnCloseClicked;
}