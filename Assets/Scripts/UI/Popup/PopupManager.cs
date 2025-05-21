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

        // �˾� �� ���� �� �ʱ�ȭ
        if (popupViewPrefab != null)
        {
            popupView = Instantiate(popupViewPrefab, transform);
            popupPresenter = new PopupPresenter(popupView);
        }
    }

    // �˾� ǥ�ø� ���� ���� �޼���
    public void ShowPopup(string title, string message, Action onConfirm = null)
    {
        popupPresenter.ShowPopup(title, message, true, false, onConfirm, null);
    }

    // Ȯ��/��� ��ư�� �ִ� �˾�
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
// ��� ����
////public void ShowSimplePopup()
////{
////    // �ܼ� �˸� �˾�
////    PopupManager.Instance.ShowPopup(
////        "�˸�",
////        "������ ����Ǿ����ϴ�.",
////        () => Debug.Log("Ȯ�� ��ư Ŭ����")
////    );
////}

////public void ShowConfirmPopup()
////{
////    // Ȯ��/��� ���� �˾�
////    PopupManager.Instance.ShowConfirmPopup(
////        "���",
////        "������ ������ �����Ͻðڽ��ϱ�?",
////        () => Debug.Log("���� ���� ����"),
////        () => Debug.Log("��� ����")
////    );
////}