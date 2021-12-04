using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager>
{
    public string Version => "v.2.0";

    public MonoBehaviour SocketServer;
}