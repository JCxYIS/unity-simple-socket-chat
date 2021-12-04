using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SocketMessage
{
    public SocketMessage() { }
    public SocketMessage(string author, string type, string content)
    {
        Author = author;
        Type = type;
        Content = content;
    }

    /// <summary>
    /// By who?
    /// </summary>
    public string Author;

    /// <summary>
    /// The type of this message
    /// </summary>
    public string Type;

    /// <summary>
    /// Usually serialized Json strings
    /// </summary>
    public string Content;
}