using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEntry
{
    public string Character { get; set; }
    public string Text { get; set; }
    public Sprite Portrait { get; set; }
}

public interface IDialogueView
{
    void Show();
    void Hide();
    void SetDialogue(string character, string text, Sprite portrait = null);
    void ShowNextDialogue();

    event Action OnDialogueFinished;
    event Action OnNextDialogueRequested;
}

