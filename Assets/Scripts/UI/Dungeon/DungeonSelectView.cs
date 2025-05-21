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
        // 던전 목록을 UI에 업데이트하는 구현
    }

    public void UpdateStageList(List<DungeonStageInfo> stages)
    {
        // 스테이지 목록을 UI에 업데이트하는 구현
    }

    public void SetDungeonInfo(DungeonInfo dungeon)
    {
        // 던전 정보를 설정하고 표시하는 구현
    }

    public void SetStageInfo(DungeonStageInfo stage)
    {
        // 스테이지 정보를 설정하고 표시하는 구현
    }

    public void HighlightDungeon(int dungeonId)
    {
        // 특정 던전을 강조하는 구현
    }

    public void HighlightStage(int stageId)
    {
        // 특정 스테이지를 강조하는 구현
    }
}