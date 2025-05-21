using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueModel
{
    // ���� ��ȭ ����
    public string CurrentCharacter { get; set; }
    public string CurrentDialogueText { get; set; }
    public Sprite CurrentPortrait { get; set; }

    // ��ȭ ��⿭
    public Queue<DialogueEntry> DialogueQueue { get; set; } = new Queue<DialogueEntry>();
}
