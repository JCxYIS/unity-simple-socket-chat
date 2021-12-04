using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRoom
{
    // -- Data Update --
    void OnRoomUpdate(RoomData roomData);

    // -- Signal ---
    void OnReceiveMessage(SocketMessage message);

    void OnSocketDispose();
}