using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDungeonSelectView
{
    void Show();
    void Hide();
    void UpdateDungeonList(List<DungeonInfo> dungeons);
    void UpdateStageList(List<DungeonStageInfo> stages);
    void SetDungeonInfo(DungeonInfo dungeon);
    void SetStageInfo(DungeonStageInfo stage);
    void HighlightDungeon(int dungeonId);
    void HighlightStage(int stageId);

    event Action<int> OnDungeonSelected;
    event Action<int> OnStageSelected;
    event Action OnEnterDungeonClicked;
    event Action OnBackButtonClicked;
}