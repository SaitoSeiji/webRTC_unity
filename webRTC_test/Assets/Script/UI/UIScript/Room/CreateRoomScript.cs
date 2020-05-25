using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CreateRoomScript : AbstractUIScript_onclick
{
    [SerializeField] InputField _hostNameField;
    [SerializeField] InputField _RoomNameField;
    [SerializeField] MatchingNCMB _myMatchingNCMB;
    int minNameLength;

    public override void OnclickAction()
    {
        if (CheckNameField())
        {
            _myMatchingNCMB.CreateNCMB(_RoomNameField.text,_hostNameField.text);
        }
        else
        {
            Debug.LogWarning("4文字以上のホスト名と部屋名を入力してください");
        }
    }

    bool CheckNameField()
    {
        if (_hostNameField.text.Length<4
            || _RoomNameField.text.Length < 4)
        {
            return false;
        }
        return true;
    }
}
