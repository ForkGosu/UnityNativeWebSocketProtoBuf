package main

import (
	"fmt"
	"log"
	"net/http"

	"websocketTEST/ProtoBuf/WebSocketPacket"

	"github.com/gorilla/websocket"
	"google.golang.org/protobuf/proto"
)

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

	for {
		_, p, err := conn.ReadMessage()
		if err != nil {
			fmt.Println(err)
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

func main() {
	http.HandleFunc("/", handleConnection)

	port := 8080
	fmt.Printf("서버가 %d 포트에서 실행 중입니다...\n", port)
	err := http.ListenAndServe(fmt.Sprintf(":%d", port), nil)
	if err != nil {
		fmt.Println(err)
	}
}
