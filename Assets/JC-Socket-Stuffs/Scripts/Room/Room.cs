using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class Room : MonoSingleton<Room>, IRoom
{
    /* -------------------------------------------------------------------------- */
    /*                                  Variables                                 */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// room data
    /// </summary>
    [SerializeField]
    RoomData roomData;

    /// <summary>
    /// (Get) Room data
    /// </summary>
    public RoomData RoomData => roomData;

    /// <summary>
    /// my user data
    /// </summary>
    public RoomUser MyUser;

    public bool IsHost = false;

    /// <summary>
    /// socket used to send / receive data
    /// </summary>
    private ISocketBase socket;

    private ChatPanel chatPanel;
    

    /* -------------------------------------------------------------------------- */
    /*                                   Events                                   */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// On Destroy
    /// </summary>
    public event Action OnDispose;

    /// <summary>
    /// On User Join
    /// </summary>
    public event Action<RoomUser> OnJoin;

    /// <summary>
    /// On User Leave
    /// (UserID)
    /// </summary>
    public event Action<string> OnLeave;

    /// <summary>
    /// On User Chat
    /// (Name, Content)
    /// </summary>
    public event Action<string, string> OnChat;

    // Make your own events here :)

    /* -------------------------------------------------------------------------- */
    /*                             Monobehaviour Func                             */
    /* -------------------------------------------------------------------------- */


    void Awake()
    {
        // MyName =  "Player" + Random.Range(0, short.MaxValue);
        DontDestroyOnLoad(gameObject);

        chatPanel = JC.Utility.ResourcesUtil.InstantiateFromResources("Prefabs/ChatPanel").GetComponent<ChatPanel>();
        chatPanel.transform.parent = transform;
        chatPanel.gameObject.SetActive(false);

        OnDispose += ()=>{
            SceneManager.LoadSceneAsync("Landing").completed += a => {
                PromptBox.CreateMessageBox("Disconnected from Room!");
            };
        };
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F7))
        {
            ToggleChatPanel();
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleChatPanel();
        }
    }

    /* -------------------------------------------------------------------------- */
    /*                                 Public Func                                */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Create room and socket connection.
    /// </summary>
    /// <param name="username">my user name</param>
    /// <param name="isHost">if true, create as server; otherwise, try connect to ip</param>
    /// <param name="ip">if isHost = true, this param is not required</param>
    /// <returns>ip</returns>
    public void CreateRoom(string username, bool isHost, string ip)
    {
        try
        {
            // create a fake new room data for init, until the server update this
            roomData = new RoomData();
            MyUser = new RoomUser(username);
            IsHost = isHost;

            if(isHost)
            {
                var ipEndpoint = SocketServer.Instance.StartServer();
                socket = SocketServer.Instance;
                socket.RegisterRoom(this);
                roomData.Ip =  ipEndpoint.Address.ToString();
            }
            else
            {
                SocketClient.Instance.TryConnect(ip, 42069);
                socket = SocketClient.Instance;
                socket.RegisterRoom(this);
                roomData.Ip = ip;
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError(e);
            Debug.LogError("Error while creating room, now dispose.");
            Dispose();
        }
    }

    /// <summary>
    /// Toggle the chat panel / console
    /// </summary>
    public void ToggleChatPanel()
    {
        chatPanel.gameObject.SetActive(!chatPanel.gameObject.activeInHierarchy);
    }

    /// <summary>
    /// Toggle the chat panel / console
    /// </summary>
    public void ToggleChatPanel(bool active)
    {
        chatPanel.gameObject.SetActive(active);
    }

    /* -------------------------------------------------------------------------- */
    
    /// <summary>
    /// Send Message to socket
    /// </summary>
    /// <param name="msgtype"></param>
    /// <param name="content"></param>
    public void SendMessage(string msgtype, string content)
    {
        if(socket == null)
        {
            throw new System.Exception("[Room] Socket not init!");
        }

        SocketMessage message = new SocketMessage();
        message.Author = MyUser.Name;
        message.Type = msgtype;
        message.Content = content;
        // message.Timestamp = System.DateTime.UtcNow;

        print($"[ROOM SENDMSG] {message.Author} : ({message.Type}) {message.Content}");
        socket.Send(message);
    }

    public void SendMessage<T>(string msgtype, T content)
    {
        SendMessage(msgtype, JsonUtility.ToJson(content));
    }

     /* -------------------------------------------------------------------------- */

    public void Dispose()
    {
        socket?.Dispose(); 

        if(gameObject)
        {
            OnDispose?.Invoke();  
            Destroy(gameObject);
            print("[ROOM] Destroyed");
        }
    }

    /* -------------------------------------------------------------------------- */
    /*                             Signal from socket                             */
    /* -------------------------------------------------------------------------- */

    public void OnRoomUpdate(RoomData roomData)
    {
        print("[ROOM] Update");
        this.roomData = roomData;
    }

    public void OnReceiveMessage(SocketMessage message)
    {
        message.Type = message.Type.ToUpper();
        print($"[ROOM GETMSG] {message.Author} : ({message.Type}) {message.Content}");

        // handle msg
        RoomUser user;
        switch(message.Type)
        {
            default:
                Debug.LogWarning("[ROOM GETMSG] Message type is undefined: " + message.Type);
                break;

            case "JOIN":
                user = JsonUtility.FromJson<RoomUser>(message.Content);
                roomData.Users.Add(user);
                OnJoin?.Invoke(user);
                break;
            
            case "LEAVE":
                roomData.Users.RemoveAll(u => u.Name == message.Content);
                OnLeave?.Invoke(message.Content);
                break;

            case "CHAT":
                OnChat?.Invoke(message.Author, message.Content);
                break;

            // Add your custom message handler here :)
        }
    }

    public void OnSocketConnected()
    {
        SendMessage("JOIN", MyUser);
    }

    public void OnSocketDispose()
    {
        print("Socket has disposed!");
        socket = null;
        Dispose();
    }

    

    /* -------------------------------------------------------------------------- */

    
}