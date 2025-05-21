using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DialogueView : MonoBehaviour, IDialogueView
{
    public event Action OnDialogueFinished;
    public event Action OnNextDialogueRequested;

    // UI �ؽ�Ʈ, �̹��� �� ����
    // [SerializeField] private Text characterNameText;
    // [SerializeField] private Text dialogueText;
    // [SerializeField] private Image portraitImage;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetDialogue(string character, string text, Sprite portrait = null)
    {
        // characterNameText.text = character;
        // dialogueText.text = text;
        // portraitImage.sprite = portrait;
    }

    public void ShowNextDialogue()
    {
        // ���� ��ȭ ��ư�̳� Ŭ�� �̺�Ʈ���� ȣ��
        OnNextDialogueRequested?.Invoke();
    }

    // ��ȭ ���� �޼��� (��: ��ȭ �Ϸ� ��ư���� ȣ��)
    public void FinishDialogue()
    {
        OnDialogueFinished?.Invoke();
    }
}
