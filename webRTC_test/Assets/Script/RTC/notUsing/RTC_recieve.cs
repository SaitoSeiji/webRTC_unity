using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;

public class RTC_recieve : AbstractRTC_actioner
{


    protected override void RecieveSDPText(RTCPeerConnection peer)
    {
        StartConnect();
        var text = _recieveSDPText.text;
        var answer = new RTCSessionDescription();
        answer.type = RTCSdpType.Offer;
        answer.sdp = text;
        peer.SetRemoteDescription(ref answer);
    }

    protected override RTCPeerConnection CreatePeer()
    {
        RTCConfiguration pc_config = new RTCConfiguration();
        var server = new RTCIceServer();
        server.urls = new string[] { "stun:stun.webrtc.ecl.ntt.com:3478" };
        pc_config.iceServers = new RTCIceServer[] {
            server
        };
        var peer = new RTCPeerConnection(ref pc_config);
        peer.OnIceConnectionChange =
            new DelegateOnIceConnectionChange(state => { MyRTC.OnIceConnectionChange(peer, state); });

        peer.OnIceCandidate = new DelegateOnIceCandidate(state => {
            Debug.Log(state.candidate);
        });
        Debug.Log("crete peer");
        return peer;
    }
}
