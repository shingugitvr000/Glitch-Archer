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

        // �� �̺�Ʈ ����
        view.OnNextDialogueRequested += HandleNextDialogue;
        view.OnDialogueFinished += HandleDialogueFinished;
    }

    // ��ȭ ��⿭�� ��ȭ �߰�
    public void AddDialogue(string character, string text, Sprite portrait = null)
    {
        model.DialogueQueue.Enqueue(new DialogueEntry
        {
            Character = character,
            Text = text,
            Portrait = portrait
        });

        // ��ȭ ť�� �׸��� �ְ� ���� ��ȭ�� ���� ��� ù ��° ��ȭ ǥ��
        if (model.DialogueQueue.Count > 0 && string.IsNullOrEmpty(model.CurrentDialogueText))
        {
            ShowNextDialogue();
        }
    }

    // ���� ��ȭ ǥ��
    private void ShowNextDialogue()
    {
        // ��⿭�� ��ȭ�� �ִ� ���
        if (model.DialogueQueue.Count > 0)
        {
            var nextDialogue = model.DialogueQueue.Dequeue();

            // �� ������Ʈ
            model.CurrentCharacter = nextDialogue.Character;
            model.CurrentDialogueText = nextDialogue.Text;
            model.CurrentPortrait = nextDialogue.Portrait;

            // �信 ��ȭ ����
            view.SetDialogue(
                nextDialogue.Character,
                nextDialogue.Text,
                nextDialogue.Portrait
            );

            // �� ǥ��
            view.Show();
        }
        else
        {
            // ��⿭�� �� �̻� ��ȭ�� ������ �� ����
            model.CurrentCharacter = null;
            model.CurrentDialogueText = null;
            model.CurrentPortrait = null;
            view.Hide();
        }
    }

    // ���� ��ȭ ��û ó��
    private void HandleNextDialogue()
    {
        ShowNextDialogue();
    }

    // ��ȭ ���� ó��
    private void HandleDialogueFinished()
    {
        // �ʿ��� �߰� ���� ���� (��: ��ȭ ���� �� �̺�Ʈ Ʈ���� ��)
        view.Hide();
    }

    // ��ȭ ����
    public void StartDialogue()
    {
        ShowNextDialogue();
    }

    // ��ȭ ��� �Ǵ� ���� ����
    public void CancelDialogue()
    {
        // ��⿭ ����
        model.DialogueQueue.Clear();

        // ���� ��ȭ �ʱ�ȭ
        model.CurrentCharacter = null;
        model.CurrentDialogueText = null;
        model.CurrentPortrait = null;

        // �� ����
        view.Hide();
    }
}