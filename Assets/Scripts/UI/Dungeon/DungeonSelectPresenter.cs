using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DungeonSelectPresenter
{
    private IDungeonSelectView view;
    private DungeonSelectModel model;

    public DungeonSelectPresenter(IDungeonSelectView view, DungeonSelectModel model)
    {
        this.view = view;
        this.model = model;

        // 이벤트 구독
        view.OnDungeonSelected += HandleDungeonSelected;
        view.OnStageSelected += HandleStageSelected;
        view.OnEnterDungeonClicked += HandleEnterDungeonClicked;
        view.OnBackButtonClicked += HandleBackButtonClicked;
    }

    private void HandleDungeonSelected(int dungeonId)
    {
        // 던전 선택 로직 구현
        model.SelectedDungeonId = dungeonId;
        // 필요한 추가 로직 (예: 스테이지 목록 업데이트 등)
    }

    private void HandleStageSelected(int stageId)
    {
        // 스테이지 선택 로직 구현
    }

    private void HandleEnterDungeonClicked()
    {
        // 던전 입장 로직 구현
    }

    private void HandleBackButtonClicked()
    {
        // 뒤로가기 로직 구현
    }
}
