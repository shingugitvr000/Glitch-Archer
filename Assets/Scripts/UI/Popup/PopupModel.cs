using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 팝업에 표시될 데이터를 관리
public class PopupModel
{
    public string Title { get; set; }
    public string Message { get; set; }
    public bool HasConfirmButton { get; set; }
    public bool HasCancelButton { get; set; }
}
