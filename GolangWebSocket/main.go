package main

import (
	"fmt"
	"log"
	"net/http"
	"time"

	"websocketTEST/ProtoBuf/WebSocketPacket"

	"github.com/gorilla/websocket"
	"google.golang.org/protobuf/proto"
)

type Client struct {
	conn     *websocket.Conn
	userUUID string
}

var clients = make(map[*websocket.Conn]*Client)

var upgrader = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool {
		return true
	},
}

func handleConnection(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		fmt.Println(err)
		return
	}
	defer conn.Close()

	fmt.Println("클라이언트 연결됨")

	// 클라이언트 생성
	client := &Client{
		conn:     conn,
		userUUID: "",
	}

	clients[conn] = client

	for {
		_, p, err := conn.ReadMessage()
		if err != nil {
			fmt.Println("WebSocket Connection Closed:", err)
			delete(clients, conn)
			return
		}

		// 역직렬화
		payload := &WebSocketPacket.PayloadClass{} // 여기서 초기화가 필요합니다.
		err = proto.Unmarshal(p, payload)
		if err != nil {
			log.Fatal("unmarshaling error: ", err)
		}
		// 모든 데이터를 출력
		log.Printf("Received payloadData: %s", payload)

		//! TestEnum 받기
		if payload.RequestCode == WebSocketPacket.PayloadType_TestEnum {
			reciveData := &WebSocketPacket.PayloadClassTestEnum{} // 여기서 초기화가 필요합니다.
			err = proto.Unmarshal(payload.RequestData, reciveData)
			if err != nil {
				log.Fatal("unmarshaling error: ", err)
			}

			// 모든 데이터를 출력
			log.Printf("Received reciveData: %s", reciveData)

			// 작업하기
			//
			reciveData.UserId = "장풍을 했다!"
			payload.BroadCastGroup = "그런그룹 없어요!"

			// 모든 작업 종료 후 직렬화
			payload.RequestData, err = proto.Marshal(reciveData)
			if err != nil {
				log.Fatal("marshaling error: ", err)
			}

			sendWebSocket(conn, payload)
		}
	}
}

func sendWebSocket(conn *websocket.Conn, payload *WebSocketPacket.PayloadClass) {
	if payload == nil {
		log.Fatal("Error sendWebSocket")
	}

	data, err := proto.Marshal(payload)
	if err != nil {
		log.Fatal("marshaling error: ", err)
	}

	// 모든 데이터를 출력
	log.Printf("Send data: %+v", payload)

	// 클라이언트로 응답 데이터 보내기
	err = conn.WriteMessage(websocket.BinaryMessage, data)
	if err != nil {
		log.Fatal("Error sending MessagePack:", err)
	}
}

func Goroutine() {
	fmt.Println("Goroutine Start")
	payload := &WebSocketPacket.PayloadClass{
		Device:             "null",
		ApplicationName:    "null",
		ApplicationVersion: "null",
		UserUUID:           "null",
		RequestCode:        0,
		ResultMessage:      "Complete",
		BroadCastGroup:     "0",
	}
	number := 1
	for {
		time.Sleep(time.Millisecond * 1000) // main 함수 바로 종료 방지
		number++
		fmt.Println("!!!! Goroutine Update !!!! : ", number)

		for conn, client := range clients {
			fmt.Printf("Client UserUUID: %s, RemoteAddr: %s\n", client.userUUID, conn.RemoteAddr())

			// 클라이언트로 응답 데이터 보내기
			sendWebSocket(conn, payload)
		}
	}
}
func myWebSocket() {
	http.HandleFunc("/", handleConnection)

	port := 28088
	fmt.Printf("서버가 %d 포트에서 실행 중입니다...\n", port)
	err := http.ListenAndServe(fmt.Sprintf(":%d", port), nil)
	if err != nil {
		fmt.Println(err)
	}
}
func main() {
	go Goroutine()
	myWebSocket()
}
