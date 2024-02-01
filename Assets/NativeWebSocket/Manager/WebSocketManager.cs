using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using NativeWebSocket;
using Google.Protobuf;
using ProtoBuf.WebSocketPacket;

public class WebSocketManager : MonoBehaviour
{
    
    private static WebSocketManager instance;
 
    public static WebSocketManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WebSocketManager>();
            }
            return instance;
        }
    }

    public string WebSocketUrl = "ws://192.168.45.20:28088";

    public string ApplicationName = "UnityNativeWebSocketProtoBuf";
    public string ApplicationVersion = "0.0.1";
    public string UserUUID = "0";


    public WebSocket m_websocket;
    // 보낸 데이터를 받았을 때 잘 받기 위함
    public Action m_onceAction;
    // 보낸 데이터를 받았을 때 잘 받기 위함
    public Action<PayloadClass> m_messageAction;
    public bool m_isAppRunning = false;

    // 시작 시 웹소켓 연결 및 초기화
    void Awake()
    {
        m_isAppRunning = true;
        WebSocketConnect(WebSocketUrl);
    }

    async void WebSocketConnect(string _url){
        m_websocket = new WebSocket(_url);

        m_websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!"+WebSocketUrl);

            // 실행 후 다 제거
            if(m_onceAction != null){
                m_onceAction();
                m_onceAction = null;
            }
        };

        m_websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        m_websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!"+WebSocketUrl);
        };

        // 메세지 받았을 때
        m_websocket.OnMessage += (bytes) =>
        {
            PayloadClass payload = PayloadClass.Parser.ParseFrom(bytes);
            
            // 받은 payload 로그
            Debug.Log("ReceiveMessage : " + payload.ToString());

            // Action에 저장된 함수를 통해 메세지 전달
            if(m_messageAction != null){
                m_messageAction(payload);
            } else {
                Debug.LogError("AddWebSocketReciveMessage로 Action을 추가해주세요");
            }
        };

        // waiting for messages
        await m_websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        m_websocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        m_isAppRunning = false;
        await m_websocket.Close();
    }


    // 메세지를 받았을 때 Action 추가
    public void AddWebSocketReciveMessage(Action<PayloadClass> action){
        m_messageAction += action;
    }
    
    // 메세지를 받았을 때 Action 삭제
    public void DeleteWebSocketReciveMessage(Action<PayloadClass> action){
        m_messageAction -= action;
    }

    
    // 연결되기전에 함수가 들어오면 대기 상태로
    public void OnceActionWebSocketMessage(Action action){
        if(m_websocket.State == WebSocketState.Open){
            action();
        } else {
            m_onceAction += action;
        }
    }
    
    // 메세지 보내기
    public void SendWebSocket<T>(PayloadType _requestCode, T _requestData, string _broadCastGroup = "") where T : IMessage<T> {
        OnceActionWebSocketMessage(()=>SendWebSocketAction(_requestCode, _requestData, _broadCastGroup));
    }

    public async void SendWebSocketAction<T>(PayloadType _requestCode, T _requestData, string _broadCastGroup = "") where T : IMessage<T> {
        if (m_websocket.State == WebSocketState.Open)
        {
            // RequestData를 위해 직렬화
            byte[] requestDataBytes = _requestData.ToByteArray();

            PayloadClass payload = new PayloadClass
            {
                Device = SystemInfo.deviceModel,
                ApplicationName = ApplicationName,
                ApplicationVersion = ApplicationVersion,
                UserUUID = UserUUID,
                BroadCastGroup = _broadCastGroup,
                RequestCode = _requestCode,
                RequestData = ByteString.CopyFrom(requestDataBytes), // PayloadType에 맞는 Class를 직렬화하여 데이터 넣음
                ResultMessage = ""
            };

            // 패킷 직렬화
            byte[] packet = payload.ToByteArray();

            // 보낼 payload 로그
            Debug.Log("SendMessage : " + payload.ToString());

            await m_websocket.Send(packet);
        }
    }
}
