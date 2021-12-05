using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ChatPanel : MonoBehaviour
{
    [SerializeField] 
    Text _chatText;

    [SerializeField]
    InputField _inputField;


    

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _chatText.text = "";
        Room.Instance.OnChat += OnChat;
        Room.Instance.OnJoin += OnJoin;
        Room.Instance.OnLeave += OnLeave;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {        
        _chatText.transform.Translate(0, 1000, 0);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(string.IsNullOrWhiteSpace(_inputField.text))
            {
                _inputField.Select();
            }
            else
            {
                SendChat();
            }
        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        if(!Room.Instance)
            return;
        Room.Instance.OnChat -= OnChat;
        Room.Instance.OnJoin -= OnJoin;
        Room.Instance.OnLeave -= OnLeave;
    }


    /* -------------------------------------------------------------------------- */

    public void SendChat()
    {
        if(string.IsNullOrWhiteSpace(_inputField.text))
            return;

        Room.Instance.SendMessage("CHAT", _inputField.text);
        _inputField.text = "";
    }

    void OnChat(string author, string msg)
    {
        _chatText.text += $"{author}: {msg}\n";
    }
    
    void OnJoin(RoomUser user)
    {
        _chatText.text += $"<color=#878787><i>{user.Name} has joined the room!</i></color>\n";
    }

    void OnLeave(string userId)
    {
        _chatText.text += $"<color=#878787><i>{userId} has left the room...</i></color>\n";
    }
}