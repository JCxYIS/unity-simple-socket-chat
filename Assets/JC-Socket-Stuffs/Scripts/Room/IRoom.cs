using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRoom
{
    // -- Data Update --
    void OnRoomUpdate(RoomData roomData);

    // -- Signal ---
    void OnReceiveMessage(SocketMessage message);

    /// <summary>
    /// Server: Inited (Listening)
    /// Client: Connected
    /// </summary>
    void OnSocketConnected();

    /// <summary>
    /// On Socket Dispose
    /// </summary>
    void OnSocketDispose();
}