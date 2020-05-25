using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using System.Linq;
using NCMB;
using System;

[DisallowMultipleComponent]
public class MatchingNCMB : MonoBehaviour
{
    [SerializeField] NCMB_RTC _signalingNCMB;
    public NCMB_RTC _SignalingNCMB { get { return _signalingNCMB; } }
    public List<NCMBObject> _serchObjList { get; private set; } = new List<NCMBObject>();
    [SerializeField] string _roomName;
    public bool _CreatedMyObj { get { return _SignalingNCMB._created; } }

    private void Awake()
    {
        _roomName= StringUtils.GeneratePassword(8);
    }

    void CreateNCMB(string roomName)
    {
        _signalingNCMB = new NCMB_RTC();
        var obj = _SignalingNCMB.CreateObject(roomName);
        var json = JsonConverter.ToJson(new NCMBStateData( NCMBStateData.MyNCMBstate.CREATEDROOM));
        obj= NCMB_RTC.SetJson_connectState(obj,json);
        _SignalingNCMB.SaveObject(obj);
    }

    void SerchNCMB(string roomName)
    {
        NCMB_RTC.GetObject(roomName, (List<NCMBObject> list) =>
        {
            _serchObjList = list;
        });
    }

    void SelectNCMB(NCMBObject selectObj)
    {
        _signalingNCMB = new NCMB_RTC();
        var json = JsonConverter.ToJson(new NCMBStateData(NCMBStateData.MyNCMBstate.SELECTEDROOM));
        selectObj = NCMB_RTC.SetJson_connectState(selectObj, json);
        _SignalingNCMB.SaveObject(selectObj);
    }
    #region onclick
    public void CreateNCMB_onclick()
    {
        CreateNCMB(_roomName);
    }
    public void SerchNCMB_onclick()
    {
        SerchNCMB(_roomName);
    }

    public void SelectNCMB_onclick()
    {
        if (_serchObjList.Count == 0)
        {
            Debug.LogWarning("_serchObject is not exist");
            return;
        }
        SelectNCMB(_serchObjList[0]);
    }

    public void StateUpdate_onClick()
    {
        _signalingNCMB.CheckStateUpdate();
    }

    public void DeleteNCMB_onclick()
    {
        _signalingNCMB.DeleteObject();
    }
    #endregion
}
