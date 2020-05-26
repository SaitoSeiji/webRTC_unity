using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NCMB;

public class SelectRoomScript : AbstractUIScript_button
{

    [SerializeField] MatchingNCMB _myMatchingNCMB;
    [SerializeField] UnityEvent _buttonClickEvent;

    protected override List<ButtonData> CreateMyButtonData()
    {
        var result= new List<ButtonData>();
        foreach(var host in _myMatchingNCMB._serchObjList)
        {
            var hostName = NCMB_RTC.Get_hostName(host);
            var add = new ButtonData(hostName,CreateClickEvent(host));
            result.Add(add);
        }
        return result;
    }

    UnityEvent CreateClickEvent(NCMBObject obj)
    {
        UnityEvent ue = new UnityEvent();
        ue.AddListener(()=> {
            _myMatchingNCMB.SelectNCMB(obj);
            _buttonClickEvent?.Invoke();
        });
        return ue;
    }
}
