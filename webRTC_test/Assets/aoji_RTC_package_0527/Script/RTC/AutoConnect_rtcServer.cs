using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyRTCEnum;

[RequireComponent(typeof(RTCObject_server),typeof(MatchingNCMB))]
[DisallowMultipleComponent]
public class AutoConnect_rtcServer : MonoBehaviour
{
    public enum ConnectState
    {
        NONE,
        MATCHING,
        SDP,
        CONNECTED,
        DELETED
    }
    [SerializeField, NonEditable] ConnectState _connectState = ConnectState.NONE;

    RTCObject_server _myRTCObject;
    MatchingNCMB _myMatching;
    RTCTYPE _myRTCType { get { return _myRTCObject._RtcType; } }
    NCMB_RTC _signalingNCMB { get { return _myMatching._SignalingNCMB; } }

    NCMBStateData.MyNCMBstate _lastActState;//一度処理を行ったstateには処理をしないこと　のためにだけ使用

    WaitFlag _chengeSDPInterbal = new WaitFlag();
    WaitFlag _checkConnectInterbal = new WaitFlag();

    private void Start()
    {
        _chengeSDPInterbal.SetWaitLength(1.0f);
        _checkConnectInterbal.SetWaitLength(1.0f);
        _myRTCObject = GetComponent<RTCObject_server>();
        _myMatching = GetComponent<MatchingNCMB>();
    }

    private void Update()
    {
        WaitFlagUpdate();
        CheckRTCConnectUpdate();
        //stateが変更されるまでにラグがあるので変更中に処理をしないようにしたい
        //一度処理を行ったstateには処理をしないことで対応　場当たり的な感じあるので微妙
        if (_signalingNCMB._ServerNCMBState==_lastActState) return;
        _lastActState = _signalingNCMB._ServerNCMBState;
        if(_myRTCType== RTCTYPE.OFFER)
        {
            ConnectAction_offer();
        }else if(_myRTCType== RTCTYPE.ANSWER)
        {
            ConnectAction_answer();
        }
    }

    void ConnectAction_offer()
    {
        switch (_signalingNCMB._ServerNCMBState)
        {
            case NCMBStateData.MyNCMBstate.SELECTEDROOM:
                _myRTCObject.OnclickCreatePeer();
                _myRTCObject.OnclickCreateOffer();
                _connectState = ConnectState.SDP;
                _chengeSDPInterbal.WaitStart();
                break;
            case NCMBStateData.MyNCMBstate.SENDED_answer:
                _myRTCObject.OnclickRecieveAnswer();
                _myRTCObject.SendIce_onclick();
                break;
            case NCMBStateData.MyNCMBstate.SENDED_ice:
                _myRTCObject.RecieveIce_onclick();
                _connectState = ConnectState.CONNECTED;
                break;
        }
    }
    void ConnectAction_answer()
    {
        switch (_signalingNCMB._ServerNCMBState)
        {
            case NCMBStateData.MyNCMBstate.SELECTEDROOM:
                _connectState = ConnectState.SDP;
                _chengeSDPInterbal.WaitStart();
                break;
            case NCMBStateData.MyNCMBstate.SENDED_offer:
                _myRTCObject.OnclickCreatePeer();
                _myRTCObject.OnclickRecieveOffer();//createAnswer含む
                break;
            case NCMBStateData.MyNCMBstate.CONNECTED_sdp:
                _myRTCObject.SendIce_onclick();
                _myRTCObject.RecieveIce_onclick();
                _connectState = ConnectState.CONNECTED;
                break;
        }
    }

    void CheckRTCConnectUpdate()
    {
        if (_connectState != ConnectState.CONNECTED) return;
        if (_myRTCObject._connectRTC)
        {
            _signalingNCMB.DeleteObject();
            _connectState = ConnectState.DELETED;
        }
    }

    void WaitFlagUpdate()
    {
        if(_connectState== ConnectState.SDP)
        {
            if (!_chengeSDPInterbal._waitNow)
            {
                _myMatching.StateUpdate_onClick();
                _chengeSDPInterbal.WaitStart();
                Debug.Log($"{gameObject.name}:auto state update");
            }
        }else if(_connectState== ConnectState.CONNECTED)
        {
            //RTCの接続が完了したかどうかを確認
            //接続していたらSendMsg_dataが成功し_myRTCObject._connectRTC=trueになるのでCheckRTCConnectUpdateでオブジェクトがデリートされる
            if (_myRTCType == RTCTYPE.ANSWER) return;
            if (!_checkConnectInterbal._waitNow)
            {
                _checkConnectInterbal.WaitStart();
                _myRTCObject.SendMsg_data("check is connected");
            }
        }
    }
}
