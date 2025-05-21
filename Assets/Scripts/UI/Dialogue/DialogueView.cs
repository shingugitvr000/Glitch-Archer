using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DialogueView : MonoBehaviour, IDialogueView
{
    public event Action OnDialogueFinished;
    public event Action OnNextDialogueRequested;

    // UI 텍스트, 이미지 등 참조
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
        // 다음 대화 버튼이나 클릭 이벤트에서 호출
        OnNextDialogueRequested?.Invoke();
    }

    // 대화 종료 메서드 (예: 대화 완료 버튼에서 호출)
    public void FinishDialogue()
    {
        OnDialogueFinished?.Invoke();
    }
}
