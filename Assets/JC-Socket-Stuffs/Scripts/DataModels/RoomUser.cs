using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class RoomUser 
{
    public string Name;

    public RoomUser(string myname)
    {
        this.Name = myname;
    }
}