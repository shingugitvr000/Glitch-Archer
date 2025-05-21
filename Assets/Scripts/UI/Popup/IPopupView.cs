using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IPopupView
{
    // 팝업 표시/숨김
    void Show();
    void Hide();

    // UI 업데이트 메서드
    void SetTitle(string title);
    void SetMessage(string message);
    void SetConfirmButtonVisible(bool isVisible);
    void SetCancelButtonVisible(bool isVisible);

    // 이벤트 - UI에서 발생하는 액션
    event Action OnConfirmClicked;
    event Action OnCancelClicked;
    event Action OnCloseClicked;
}