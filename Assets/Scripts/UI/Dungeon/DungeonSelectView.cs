using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSelectView : MonoBehaviour, IDungeonSelectView
{
    public event Action<int> OnDungeonSelected;
    public event Action<int> OnStageSelected;
    public event Action OnEnterDungeonClicked;
    public event Action OnBackButtonClicked;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateDungeonList(List<DungeonInfo> dungeons)
    {
        // ���� ����� UI�� ������Ʈ�ϴ� ����
    }

    public void UpdateStageList(List<DungeonStageInfo> stages)
    {
        // �������� ����� UI�� ������Ʈ�ϴ� ����
    }

    public void SetDungeonInfo(DungeonInfo dungeon)
    {
        // ���� ������ �����ϰ� ǥ���ϴ� ����
    }

    public void SetStageInfo(DungeonStageInfo stage)
    {
        // �������� ������ �����ϰ� ǥ���ϴ� ����
    }

    public void HighlightDungeon(int dungeonId)
    {
        // Ư�� ������ �����ϴ� ����
    }

    public void HighlightStage(int stageId)
    {
        // Ư�� ���������� �����ϴ� ����
    }
}