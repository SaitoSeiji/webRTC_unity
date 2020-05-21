using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;

public class RTC_send : AbstractRTC_actioner
{


    int negoCount=0;
    
    protected override void RecieveSDPText(RTCPeerConnection peer)
    {
        var text = _recieveSDPText.text;
        var answer = new RTCSessionDescription();
        answer.type = RTCSdpType.Answer;
        answer.sdp = text;
        peer.SetRemoteDescription(ref answer);
    }

    protected override RTCPeerConnection CreatePeer()
    {//ローカル
        RTCConfiguration pc_config = new RTCConfiguration();
        var server = new RTCIceServer();
        server.urls = new string[] { "stun:stun.webrtc.ecl.ntt.com:3478" };
        pc_config.iceServers = new RTCIceServer[] {
            server
        };
        var peer = new RTCPeerConnection(ref pc_config);
        RTCDataChannelInit conf = new RTCDataChannelInit(true);
        localDataChannel = peer.CreateDataChannel("send", ref conf);
        localDataChannel.OnOpen = new DelegateOnOpen(() => { Debug.Log("localOpen"); });
        localDataChannel.OnClose = new DelegateOnClose(() => { Debug.Log("localClose"); });
        peer.OnIceConnectionChange =
            new DelegateOnIceConnectionChange(state => { MyRTC.OnIceConnectionChange(peer, state); });

        peer.OnIceCandidate = new DelegateOnIceCandidate(evt => {

            if (!string.IsNullOrEmpty( evt.candidate))
            {
                Debug.Log(evt.candidate);
            }
            //else
            //{
            //    SendSDP_onclick();
            //}
        });

        //peer.OnNegotiationNeeded = () =>
        //{
        //    try
        //    {
        //        if (negoCount == 0)
        //        {
        //            StartCoroutine(CreateOffer(peer));
        //            negoCount++;
        //        }
        //    }
        //    catch
        //    {
        //        Debug.Log("eroor");
        //    }
        //};

        Debug.Log("crete peer");
        return peer;
    }
    IEnumerator CreateOffer()
    {
        //オファー
        var op1 = localConnection.CreateOffer(ref OfferOptions);
        yield return op1;
        var op2 = localConnection.SetLocalDescription(ref op1.desc);
        yield return op2;
        if (!op1.isError)
        {
            SDP2Text(localConnection, _mySDPText);
        }
        else
        {
            Debug.Log("error offer");
        }
    }

    
    public void CreateOffer_onclick()
    {
        StartCoroutine( CreateOffer());
        Debug.Log("createOffer");
    }
}
