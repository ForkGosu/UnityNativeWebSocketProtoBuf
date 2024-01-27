using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ProtoBuf.WebSocketPacket;

public class WebSocketSample : MonoBehaviour
{
    void Awake()
    {
        // 웹소켓 받기
        WebSocketManager.Instance?.AddWebSocketReciveMessage(WebSocketReceiveMessage);
    }
    private void OnDestroy()
    {
        // 오브젝트 파괴되면 더이상 받지 않기
        WebSocketManager.Instance?.DeleteWebSocketReciveMessage(WebSocketReceiveMessage);
    }
    
    public void WebSocketReceiveMessage(PayloadClass _payload)
    {
        //! (유저 입장 시) 플레이어 정보 불러오기
        if(_payload.RequestCode == PayloadType.TestEnum){
            if(_payload.ResultMessage == ""){
                PayloadClassTestEnum testEnum = PayloadClassTestEnum.Parser.ParseFrom(_payload.RequestData);
                ReceiveTest(testEnum);
            }
            // 에러시
            else if(_payload.ResultMessage == "Error"){

            }
        }
    }

    public void ReceiveTest(PayloadClassTestEnum _testEnum){
        Debug.Log(_testEnum.ToString());
    }
}
