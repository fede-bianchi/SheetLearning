# ChatHub — SignalR Client Contract

## Connection
URL: /hubs/chat
Auth: JWT passed as query parameter: ?access_token={token}

## Client → Server methods

| Method | Parameters | Description |
|---|---|---|
| JoinChat | chatId: int | Join typing-indicator group for a chat |
| LeaveChat | chatId: int | Leave the typing-indicator group |
| StartTyping | chatId: int | Broadcast that this user is typing |
| StopTyping | chatId: int | Broadcast that this user stopped typing |

## Server → Client events

| Event | Payload | Description |
|---|---|---|
| ReceiveMessage | ChatMessageDto | New message delivered to a participant |
| MessagesRead | chatId: int | The other participant read all messages |
| UserTyping | chatId: int, nickname: string | A user started typing |
| UserStoppedTyping | chatId: int, nickname: string | A user stopped typing |

## ChatMessageDto shape
{
  id: number,
  chatId: number,
  senderId: number,
  senderNickname: string,
  contenuto: string,
  letto: boolean,
  createdAt: string (ISO 8601)
}
