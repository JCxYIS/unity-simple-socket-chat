using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChatPanel : MonoBehaviour
{
    [SerializeField] 
    Text _chatText;

    [SerializeField]
    InputField _inputField;


    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {        
        Room.Instance.OnChat += OnChat;
        _chatText.transform.Translate(0, 1000, 0);
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        Room.Instance.OnChat -= OnChat;
    }


    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _chatText.text = "";
    }    

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendChat();
        }
    }


    public void SendChat()
    {
        if(string.IsNullOrEmpty(_inputField.text))
            return;

        Room.Instance.SendMessage("Chat", _inputField.text);
        _inputField.text = "";
    }

    void OnChat(string author, string msg)
    {
        _chatText.text += $"{author}: {msg}\n";
    }
}