using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LandingSceneManager : MonoBehaviour
{
    public enum State 
    {
        Main = 0, 
        Multiplayer = 1,
        Room = 2,
    }

    [Header("Bindings")]
    [SerializeField] RectTransform[] Panels;
    [SerializeField] InputField Intro_NameInput;
    [SerializeField] Text Room_IpText;
    [SerializeField] Text Room_PlayersText;


    [Header("Variables")]
    [SerializeField] State state;


    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        Intro_NameInput.text = PlayerPrefs.GetString("JC_SOCKET_CHAT_USERNAME");
        ChangeState(State.Main);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // DEBUG
        // if(Input.GetKeyDown(KeyCode.F7))
        // {
        //     SceneManager.LoadScene("Test_Chat");
        // }

        // 
        if(state == State.Room)
        {
            Room_IpText.text = "IP: " + Room.Instance.RoomData.Ip;
            Room_PlayersText.text = "<b>---Players---</b>\n"; 
            Room.Instance.RoomData.Users.ForEach(u => Room_PlayersText.text += u.Name + "\n");
        }
    }


    /* -------------------------------------------------------------------------- */

    public void ChangeState(int stateId)
    {
        if(string.IsNullOrWhiteSpace(Intro_NameInput.text))
        {
            PromptBox.CreateMessageBox("Name cannot be empty!");
            return;
        }
        PlayerPrefs.SetString("JC_SOCKET_CHAT_USERNAME", Intro_NameInput.text);
        ChangeState((State)stateId);
    }

    public void ChangeState(State state)
    {
        this.state = state;

        for(int i = 0; i < 3; i++)
        {
            Panels[i].gameObject.SetActive(i == (int)state);
        }
    }

    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Open Chat button
    /// </summary>
    public void OpenChat()
    {
        Room.Instance.ToggleChatPanel(true);
    }

    /* -------------------------------------------------------------------------- */
    /*                              Socket Stuff                                  */
    /* -------------------------------------------------------------------------- */
    
    public void CreateRoom()
    {
        Room.Instance.CreateRoom(Intro_NameInput.text, true, "");
        ChangeState(State.Room);
    }

    public void JoinRoom(InputField ipInput)
    {
        if(!System.Net.IPAddress.TryParse(ipInput.text, out System.Net.IPAddress ip))
        {
            PromptBox.CreateMessageBox("Invalid IP detected");
            return;
        }
        Room.Instance.CreateRoom(Intro_NameInput.text, false, ipInput.text);
        ChangeState(State.Room);
    }

    public void ExitRoom()
    {
        Room.Instance.Dispose();
        ChangeState(State.Main);
    }

    /* -------------------------------------------------------------------------- */
}