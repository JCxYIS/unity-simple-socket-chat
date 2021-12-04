using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class RoomData
{
    public string Ip = "undefined";
    public List<RoomUser> Users = new List<RoomUser>();
}