using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using System.Linq;
using NCMB;
using System;
using MyRTCEnum;

namespace MyRTCEnum
{
    public enum RTCTYPE
    {
        OFFER, ANSWER
    }
}

[DisallowMultipleComponent]
public class RTCObject_server : MonoBehaviour
{
    [SerializeField] RTCTYPE _rtcType;
    public RTCTYPE _RtcType { get { return _rtcType; } }
    bool _IsOffer { get { return _rtcType == RTCTYPE.OFFER; } }
    public bool _connectRTC { get; private set; } = false;//接続が成立したかどうかを知らせる

    [SerializeField] Text sendText;
    [SerializeField] Text recieveText;

    RTCPeerConnection localConnection;
    private RTCDataChannel localDataChannel;
    private RTCDataChannel remoteDataChannel;
    List<RTCIceCandidate> _localIceCandidate = new List<RTCIceCandidate>();

    MatchingNCMB _matchingNCMB;
    NCMB_RTC _signalingNCMB { get { return _matchingNCMB._SignalingNCMB; } }

    private RTCOfferOptions OfferOptions = new RTCOfferOptions
    {
        iceRestart = true,
        offerToReceiveAudio = true,
        offerToReceiveVideo = false
    };
    private RTCAnswerOptions AnswerOptions = new RTCAnswerOptions
    {
        iceRestart = true,
    };
    private DelegateOnMessage onDataChannelMessage;//データチャンネル受信時のコールバック


    private void Awake()
    {
        WebRTC.Initialize();
        //メッセージ受信時の処理
        onDataChannelMessage = new DelegateOnMessage(bytes => {
            recieveText.text = System.Text.Encoding.UTF8.GetString(bytes);
            if (!_connectRTC)
            {
                SendMsg_data("Connected");
            }
            _connectRTC = true;
        });
        _matchingNCMB = GetComponent<MatchingNCMB>();
    }


    private void OnDestroy()
    {
        WebRTC.Finalize();
    }
    #region peerAction_raw
    //ピアの作成をする
    void CreatePeer()
    {
        //ローカル
        RTCConfiguration pc_config = new RTCConfiguration();
        var server = new RTCIceServer();
        server.urls = new string[] { "stun:stun.webrtc.ecl.ntt.com:3478" };
        pc_config.iceServers = new RTCIceServer[] {
            server
        };
        localConnection = new RTCPeerConnection(ref pc_config);
        RTCDataChannelInit conf = new RTCDataChannelInit(true);
        localDataChannel = localConnection.CreateDataChannel("send", ref conf);
        localDataChannel.OnOpen = new DelegateOnOpen(() => { Debug.Log("データチャネル:localOpen"); });
        localDataChannel.OnClose = new DelegateOnClose(() => { Debug.Log("データチャネル:localClose"); });
        //localDataChannel.OnMessage = onDataChannelMessage;
        localConnection.OnDataChannel = new DelegateOnDataChannel(x => {
            Debug.Log("ondatachannel");
            remoteDataChannel = x;
            remoteDataChannel.OnMessage = onDataChannelMessage;
        });
        localConnection.OnIceConnectionChange =
            new DelegateOnIceConnectionChange(state => { OnIceConnectionChange(localConnection, state); });

        //ICEの登録
        localConnection.OnIceCandidate = new DelegateOnIceCandidate(candidate => {
            if (!string.IsNullOrEmpty(candidate.candidate))
            {
                _localIceCandidate.Add(candidate);
                Debug.Log("アイス：add my Ice" + candidate.candidate);
            }
            else
            {
                Debug.Log("end ice candidate");
            }
        });

        Debug.Log("crete peer");
    }

    void SendSDP(RTCSessionDescription session,NCMBStateData.MyNCMBstate state)
    {
        bool isoffer = (session.type == RTCSdpType.Offer);
        var type = (isoffer) ? RTCSendData.DATATYPE.OFFERE : RTCSendData.DATATYPE.ANSWER;
        var data = new RTCSendData(type,session.sdp);
        var json = JsonConverter.ToJson(data);
        var json_state = JsonConverter.ToJson(new NCMBStateData(state));
        _signalingNCMB.FetchObject((NCMBObject obj)=>{
            var saveobj= NCMB_RTC.SetJson_SDPData( obj, isoffer, json);
            saveobj = NCMB_RTC.SetJson_connectState(saveobj,json_state);
            _signalingNCMB.UpdateObject(saveobj);
        });
        
    }

     void  RecieveSDP(Action<RTCSessionDescription> act)
    {
        _signalingNCMB.FetchObject((obj) => {
            
            string json = NCMB_RTC.GetJson_SDPData(obj,_IsOffer);
            var data = JsonConverter.FromJson<RTCSendData>(json);
            var result = new RTCSessionDescription();
            result.type = (_IsOffer) ? RTCSdpType.Answer : RTCSdpType.Offer;
            result.sdp = data._sdp;
            act.Invoke( result);
        });
    }

    IEnumerator CreateOffer()
    {
        if (!_IsOffer) yield break;
        //オファー
        var op1 = localConnection.CreateOffer(ref OfferOptions);
        yield return op1;
        var op2 = localConnection.SetLocalDescription(ref op1.desc);
        yield return op2;
        if (!op1.isError)
        {
            //TextSDP(op1.desc, _mySDPText);
            SendSDP(op1.desc, NCMBStateData.MyNCMBstate.SENDED_offer);
        }
        else
        {
            Debug.Log("error offer");
        }
    }
    void RecieveOffer()
    {
        if (_rtcType != RTCTYPE.ANSWER) return;
        //var offer = ConvertText2desc(_recieveSDPText, true);
        RecieveSDP((offer)=> {
            try
            {
                Debug.Log($"recieve offer");
                localConnection.SetRemoteDescription(ref offer);
                StartCoroutine(CreateAnswer());
            }
            catch
            {
                Debug.Log("cant offer recieve");
            }
        });
        
    }

    IEnumerator CreateAnswer()
    {
        if (_rtcType != RTCTYPE.ANSWER) yield break;
        //アンサー
        var op4 = localConnection.CreateAnswer(ref AnswerOptions);
        yield return op4;
        var op5 = localConnection.SetLocalDescription(ref op4.desc);
        yield return op5;
        //TextSDP(op4.desc, _mySDPText);
        SendSDP(op4.desc,NCMBStateData.MyNCMBstate.SENDED_answer);
        Debug.Log("create answer");
    }

    void RecieveAnswer()
    {
        if (_rtcType != RTCTYPE.OFFER) return;
        //var answer = ConvertText2desc(_recieveSDPText, false);
        RecieveSDP((answer)=> {
            try
            {
                localConnection.SetRemoteDescription(ref answer);
                Debug.Log("recieved answere");

            }
            catch
            {
                Debug.Log("cant answer recieve");
            }
        });
    }

    void SendIceCandidata(List<string> jsonlist)
    {
        _signalingNCMB.FetchObject((obj) =>
        {
            var json = NCMB_RTC.GetJson_SDPData(obj, _IsOffer);
            var data = JsonConverter.FromJson<RTCSendData>(json);
            data.candidateJson = jsonlist;
            json = JsonConverter.ToJson(data);
            var json_state = (_IsOffer) ? JsonConverter.ToJson(new NCMBStateData(NCMBStateData.MyNCMBstate.CONNECTED_sdp))
                                        : JsonConverter.ToJson(new NCMBStateData(NCMBStateData.MyNCMBstate.SENDED_ice));
            var saveobj=NCMB_RTC.SetJson_SDPData( obj, _IsOffer, json);
            saveobj = NCMB_RTC.SetJson_connectState(saveobj, json_state);
            
            _signalingNCMB.UpdateObject(saveobj);
            Debug.Log($"sendJson");
        });
    }

    void RecieveIceCandidate()
    {
        _signalingNCMB.FetchObject((obj) =>
        {
            var json = NCMB_RTC.GetJson_SDPData(obj,_IsOffer);
            var data = JsonConverter.FromJson<RTCSendData>(json);
            var remoteICE = new List<RTCIceCandidate>();
            foreach(var target in data.candidateJson)
            {
                remoteICE.Add(JsonConverter.FromJson<RTCIceCandidate>(target));
            }
            remoteICE.ForEach(x => localConnection.AddIceCandidate(ref x));
            Debug.Log($"recieveJson {gameObject.name}");
        });
    }

    //接続が成立したことを知らせる
    void InformConnect()
    {
        _signalingNCMB.FetchObject((obj) =>
        {
            var json_state = JsonConverter.ToJson(new NCMBStateData(NCMBStateData.MyNCMBstate.CONNECTED));
            var savedObj = NCMB_RTC.SetJson_connectState(obj, json_state);
            _signalingNCMB.UpdateObject(savedObj);
        });
    }
    #endregion

    #region onclick
    public void OnclickCreatePeer()
    {
        CreatePeer();
    }

    public void OnclickCreateOffer()
    {
        StartCoroutine(CreateOffer());
    }

    public void OnclickRecieveOffer()
    {
        RecieveOffer();
    }
    public void OnclickRecieveAnswer()
    {
        RecieveAnswer();
    }

    public void SendIce_onclick()
    {
        var jsonlist = _localIceCandidate.Select(x => JsonConverter.ToJson(x)).ToList();
        SendIceCandidata(jsonlist);
    }

    public void RecieveIce_onclick()
    {
        RecieveIceCandidate();
    }

    public void Onclicl_setState_Offer()
    {
        SetState(RTCTYPE.OFFER);
    }
    public void Onclicl_setState_Answer()
    {
        SetState(RTCTYPE.ANSWER);
    }
    #endregion
    #region public
    public void SendMsg_text()
    {
        localDataChannel.Send(sendText.text);
    }
    public void SendMsg_data(string data)
    {
        localDataChannel.Send(data);
    }

    public void SetState(RTCTYPE type)
    {
        if (_signalingNCMB._MyNCMBState != NCMBStateData.MyNCMBstate.NONE) return;
        _rtcType = type;
    }

    #endregion
    //裏方======================================
    void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
    {
        switch (state)
        {
            case RTCIceConnectionState.New:
                Debug.Log($"{GetName(pc)} IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"{GetName(pc)} IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Closed");
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Completed");
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Connected");
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Disconnected");
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Failed");
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"{GetName(pc)} IceConnectionState: Max");
                break;
            default:
                break;
        }
    }
    //=======================================


    string GetName(RTCPeerConnection pc)
    {
        return (pc == localConnection) ? "localConnection" : "remoteConnection";
    }

    

}
