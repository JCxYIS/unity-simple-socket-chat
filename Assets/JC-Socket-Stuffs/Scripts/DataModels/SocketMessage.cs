using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SocketMessage
{
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