using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

public class SocketClient : MonoSingleton<SocketClient>, ISocketBase
{
    /// <summary>
    /// Should continue listening to socket?
    /// </summary>
    private volatile bool shouldStop = false;

    private Socket socket;

    private Thread thread;

    private IRoom room;

    private Queue<Action> threadTasks = new Queue<Action>();

    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        // Dispose();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        lock(threadTasks)
        {
            if(threadTasks.Count == 0) 
                return;
            threadTasks.Dequeue().Invoke();
        }
    }

    /* -------------------------------------------------------------------------- */


    public void TryConnect(string ip, int port)
    {
        // thread is running
        if(thread != null)
        {
            throw new Exception("[SOCKET] Socket is already created");
        }

        // parse param
        IPEndPoint host = new IPEndPoint(IPAddress.Parse(ip), port);

        // create thread
        thread = new Thread(()=>ClientThread(host));
        thread.IsBackground = true;
        thread.Start();
    }

    private void ClientThread(IPEndPoint host)
    {
        // SOCKET
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // CONNECT
        try
        {
            socket.Connect(host);
            room.OnSocketConnected();
            print("[SOCKETC Connected]");
        }
        catch(Exception e)
        {
            Debug.LogError("[SOCKETC Error] Error while Connecting: " + e);
            Dispose();
            return;
        }

        // read
        byte[] buffer = new byte[1024];
        string receive = "";
        while(true)
        {
            buffer = new byte[1024];

            // Read buffer
            int receiveCount = socket.Receive(buffer);  // frz here until buffer full lmao
            if(receiveCount == 0)
            {
                Debug.LogWarning("[SOCKETC EMPTY RECV] Socket Disconnected");
                lock(threadTasks)
                {
                    threadTasks.Enqueue(()=>Dispose());
                }
                return;
            }

            // Encode to string
            receive = Encoding.UTF8.GetString(buffer, 0, receiveCount);
            Debug.Log("[SOCKETC GET] "+receive);

            // Parse Message
            try
            {
                var msg = JsonUtility.FromJson<SocketMessage>(receive);
                lock(threadTasks)
                {
                    threadTasks.Enqueue(()=>room?.OnReceiveMessage(msg));
                }
            }
            catch(Exception e)
            {
                Debug.LogError("[SOCKETC] Failed to parse SocketMessage: "+ e +"\nMSG=" + receive);
            }

            System.Threading.Thread.Sleep(1);
        }
    }

    public void RegisterRoom(IRoom listener)
    {
        room = listener;
    }

    public void Send(SocketMessage message)
    {
        string str = JsonUtility.ToJson(message);
        byte[] sendData = new byte[1024];
        sendData = Encoding.UTF8.GetBytes(str);
        socket.Send(sendData, sendData.Length, SocketFlags.None);
        Debug.Log("[SOCKETC SEND] "+sendData);
    }

    public void Dispose()
    {        
        // socket
        socket?.Dispose();

        // lock
        lock(threadTasks)
        {
            threadTasks.Enqueue(()=>{
                room?.OnSocketDispose();
                Destroy(gameObject);
            });
        }

        // thread
        thread?.Abort();
        thread = null;
        
    }    
}