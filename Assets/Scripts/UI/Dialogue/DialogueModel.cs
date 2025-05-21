using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueModel
{
    // 현재 대화 정보
    public string CurrentCharacter { get; set; }
    public string CurrentDialogueText { get; set; }
    public Sprite CurrentPortrait { get; set; }

    // 대화 대기열
    public Queue<DialogueEntry> DialogueQueue { get; set; } = new Queue<DialogueEntry>();
}
