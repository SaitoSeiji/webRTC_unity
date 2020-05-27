using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SerchRoomScript : AbstractUIScript_onclick
{
    [SerializeField] MatchingNCMB _myMatchingNCMB;
    
    [SerializeField] InputField _RoomNameField;
    int minNameLength=4;
    [SerializeField] UnityEvent _succsessOnclick;
    bool clickedIgnore = false;

    protected override void ChengeState_toActive()
    {
        base.ChengeState_toActive();
        clickedIgnore = false;
    }

    public override void OnclickAction()
    {
        if (clickedIgnore) return;
        if (CheckNameField())
        {
            _myMatchingNCMB.SerchNCMB(_RoomNameField.text,additionalAct:()=> _succsessOnclick?.Invoke());
            clickedIgnore = true;
        }
        else
        {
            Debug.LogWarning("4文字以上の部屋名を入力してください");
        }
    }


    bool CheckNameField()
    {
        if ( _RoomNameField.text.Length < minNameLength)
        {
            return false;
        }
        return true;
    }
}
