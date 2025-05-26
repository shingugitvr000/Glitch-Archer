using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupTester : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P) && PopupManager.Instance != null)
        {
            //단순 알림 팝업
            PopupManager.Instance.ShowPopup(
                    "알림",
                    "게임이 저장되었습니다.",
                    () => Debug.Log("확인 버튼 클릭됨")
                );
        }

        if (Input.GetKeyDown(KeyCode.O) && PopupManager.Instance != null)
        {
            // 확인/취소 선택 팝업
            PopupManager.Instance.ShowConfirmPopup(
                "경고",
                "정말로 게임을 종료하시겠습니까?",
                () => Debug.Log("게임 종료 선택"),
                () => Debug.Log("취소 선택")
            );
        }
    }
}
