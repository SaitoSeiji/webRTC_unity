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
    int minNameLength=4;
    [SerializeField] UnityEvent _succsessOnclick;
    
    public override void OnclickAction()
    {
        if (CheckNameField())
        {
            _myMatchingNCMB.CreateNCMB(_RoomNameField.text,_hostNameField.text);
            _succsessOnclick?.Invoke();
        }
        else
        {
            Debug.LogWarning("4文字以上のホスト名と部屋名を入力してください");
        }
    }

    bool CheckNameField()
    {
        if (_hostNameField.text.Length<minNameLength
            || _RoomNameField.text.Length < minNameLength)
        {
            return false;
        }
        return true;
    }
}
