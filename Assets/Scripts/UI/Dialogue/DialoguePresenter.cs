using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePresenter
{
    private IDialogueView view;
    private DialogueModel model;

    public DialoguePresenter(IDialogueView view, DialogueModel model)
    {
        this.view = view;
        this.model = model;

        // 뷰 이벤트 구독
        view.OnNextDialogueRequested += HandleNextDialogue;
        view.OnDialogueFinished += HandleDialogueFinished;
    }

    // 대화 대기열에 대화 추가
    public void AddDialogue(string character, string text, Sprite portrait = null)
    {
        model.DialogueQueue.Enqueue(new DialogueEntry
        {
            Character = character,
            Text = text,
            Portrait = portrait
        });

        // 대화 큐에 항목이 있고 현재 대화가 없는 경우 첫 번째 대화 표시
        if (model.DialogueQueue.Count > 0 && string.IsNullOrEmpty(model.CurrentDialogueText))
        {
            ShowNextDialogue();
        }
    }

    // 다음 대화 표시
    private void ShowNextDialogue()
    {
        // 대기열에 대화가 있는 경우
        if (model.DialogueQueue.Count > 0)
        {
            var nextDialogue = model.DialogueQueue.Dequeue();

            // 모델 업데이트
            model.CurrentCharacter = nextDialogue.Character;
            model.CurrentDialogueText = nextDialogue.Text;
            model.CurrentPortrait = nextDialogue.Portrait;

            // 뷰에 대화 설정
            view.SetDialogue(
                nextDialogue.Character,
                nextDialogue.Text,
                nextDialogue.Portrait
            );

            // 뷰 표시
            view.Show();
        }
        else
        {
            // 대기열에 더 이상 대화가 없으면 뷰 숨김
            model.CurrentCharacter = null;
            model.CurrentDialogueText = null;
            model.CurrentPortrait = null;
            view.Hide();
        }
    }

    // 다음 대화 요청 처리
    private void HandleNextDialogue()
    {
        ShowNextDialogue();
    }

    // 대화 종료 처리
    private void HandleDialogueFinished()
    {
        // 필요한 추가 로직 구현 (예: 대화 종료 후 이벤트 트리거 등)
        view.Hide();
    }

    // 대화 시작
    public void StartDialogue()
    {
        ShowNextDialogue();
    }

    // 대화 취소 또는 강제 종료
    public void CancelDialogue()
    {
        // 대기열 비우기
        model.DialogueQueue.Clear();

        // 현재 대화 초기화
        model.CurrentCharacter = null;
        model.CurrentDialogueText = null;
        model.CurrentPortrait = null;

        // 뷰 숨김
        view.Hide();
    }
}