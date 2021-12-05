using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

/// <summary>
/// Socket Server.
/// Will run in another thread
/// </summary>
public class SocketServer : MonoSingleton<SocketServer>, ISocketBase
{
    /// <summary>
    /// Should continue listening to socket?
    /// </summary>
    private volatile bool shouldStop = false;

    /// <summary>
    /// Server socket endpoint
    /// </summary>
    private IPEndPoint ipEndPoint;

    /// <summary>
    /// Server socket for accepting connection
    /// </summary>
    private Socket serverSocket;

    /// <summary>
    /// Socket with client we established
    /// </summary>
    private List<Socket> clientSockets = new List<Socket>();

    /// <summary>
    /// Server socket thread
    /// </summary>
    private Thread serverSocketThread;

    /// <summary>
    /// Room
    /// </summary>
    private IRoom room;

    /// <summary>
    /// Client Socket threads
    /// </summary>
    /// <typeparam name="Socket"></typeparam>
    /// <typeparam name="Thread"></typeparam>
    /// <returns></returns>
    private Dictionary<Socket, Thread> clientSocketThread = new Dictionary<Socket, Thread>();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Action"></typeparam>
    /// <returns></returns>
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

    public SocketServer()
    {
        
    }

    /// <summary>
    /// Start Server Socket 
    /// </summary>
    /// <returns>local ip / port</returns>
    public IPEndPoint StartServer(int port)
    {
        // thread is running
        if(serverSocketThread != null)
        {
            throw new Exception("[Socket] Server Socket is already created");
        }

        // Get host
        IPAddress localIp = GetLocalIp();
        Debug.Log("Ip=" + localIp.ToString());
        ipEndPoint = new IPEndPoint(localIp, port);        

        // Create Update thread
        serverSocketThread = new Thread(ServerSocketThread);
        serverSocketThread.IsBackground = true;
        serverSocketThread.Start();
        
        return ipEndPoint;
    }

    /// <summary>
    /// The major code that server socket runs (on another thread)
    /// </summary>
    private void ServerSocketThread()
    {
        // SOCKET
        serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // BIND
        try
        {
            serverSocket.Bind(ipEndPoint);
        }
        catch(Exception e)
        {
            Debug.LogError("[SOCKETS Error] Error while Binding: " + e);
            Dispose();
            return;
        }

        // LISTEN
        try
        {
            serverSocket.Listen(16);
            room.OnSocketConnected();
            Debug.Log("[SOCKETS LISTENING] "+ipEndPoint.Address);
        }
        catch(Exception e)
        {
            Debug.LogError("[SOCKETS Error] Error while Listening: " + e);
            Dispose();
            return;
        }


        while(!shouldStop)
        {
            // ACCEPT
            Socket newClient = serverSocket.Accept(); // thread will stuck here until connect
            
            Debug.Log("[SOCKETS ACCEPTED] "+newClient.AddressFamily);
            clientSockets.Add(newClient);
            Thread newClientThread = new Thread(()=>ClientSocketThread(newClient));
            clientSocketThread.Add(newClient, newClientThread);
            newClientThread.Start();

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Thread of socket communicate with client (one thread per client)
    /// </summary>
    private void ClientSocketThread(Socket clientSocket)
    {
        byte[] buffer;
        string receive;
        string clientName = "";
        while(!shouldStop)
        {
            buffer = new byte[1024];

            // Read buffer
            int receiveCount = clientSocket.Receive(buffer);
            if(receiveCount == 0)
            {
                Debug.LogWarning("[SOCKETS EMPTY RECV] A Client Socket Disconnected");
                clientSocket.Dispose();
                clientSockets.Remove(clientSocket);
                clientSocketThread.Remove(clientSocket);                
                Send(new SocketMessage("", "LEAVE", clientName));
                return;
            }

            // Encode to string
            receive = Encoding.UTF8.GetString(buffer, 0, receiveCount);
            Debug.Log("[SOCKETS GET] "+receive);

            // Parse Message
            try
            {
                var msg = JsonUtility.FromJson<SocketMessage>(receive);

                // memorize client name, in case we forget / sanity check name is modified
                if(string.IsNullOrEmpty(clientName))
                {
                    // memorize that
                    clientName = msg.Author;
                }
                else
                {
                    // Debug.LogWarning($"[SERVERS] A client failed to pass sanity check: NAME {clientName} -> {msg.Author}");
                    msg.Author = clientName;
                }

                // broadcast to every clients including myself
                Send(msg);
            }
            catch(Exception e)
            {
                Debug.LogError("[SOCKETS] Failed to parse SocketMessage: "+ e +"\nMSG=" + receive);
            }

            System.Threading.Thread.Sleep(1);
        }        
    }

    /// <summary>
    /// Try closing sockets
    /// </summary>
    public void Dispose()
    {
        shouldStop = true;

        // close client
        clientSockets.ForEach(s => s?.Dispose());
        foreach(var key in clientSocketThread)
        {
            key.Value?.Abort();
        }
        clientSockets = new List<Socket>();
        clientSocketThread = new Dictionary<Socket, Thread>();
        
        // thread lock stuffs
        lock(threadTasks)
        {
            threadTasks.Enqueue(()=>{
                room?.OnSocketDispose();
                Destroy(gameObject);
            });
        }

        // close server
        serverSocketThread?.Abort(); // 
        serverSocket?.Dispose();
        serverSocketThread = null;

    }

    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Send / Broadcast message to ALL clients
    /// </summary>
    public void Send(SocketMessage message)
    {
        string str = JsonUtility.ToJson(message);
        byte[] sendData = new byte[1024];
        sendData = Encoding.UTF8.GetBytes(str);
        foreach(var client in clientSockets)
        {
            client.Send(sendData,sendData.Length, SocketFlags.None);
        }

        // send a message upwards to myself
        lock(threadTasks)
        {
            threadTasks.Enqueue(()=>room?.OnReceiveMessage(message));
        }
        
        Debug.Log("[SOCKETS SEND] (To ALL) "+sendData);
    }

    public void RegisterRoom(IRoom listener)
    {
        room = listener;
    }

    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Get local ip address
    /// </summary>
    /// <returns></returns>
    private IPAddress GetLocalIp()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }

        }
        return null;
    }
}