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

        // �̺�Ʈ ����
        view.OnDungeonSelected += HandleDungeonSelected;
        view.OnStageSelected += HandleStageSelected;
        view.OnEnterDungeonClicked += HandleEnterDungeonClicked;
        view.OnBackButtonClicked += HandleBackButtonClicked;
    }

    private void HandleDungeonSelected(int dungeonId)
    {
        // ���� ���� ���� ����
        model.SelectedDungeonId = dungeonId;
        // �ʿ��� �߰� ���� (��: �������� ��� ������Ʈ ��)
    }

    private void HandleStageSelected(int stageId)
    {
        // �������� ���� ���� ����
    }

    private void HandleEnterDungeonClicked()
    {
        // ���� ���� ���� ����
    }

    private void HandleBackButtonClicked()
    {
        // �ڷΰ��� ���� ����
    }
}
