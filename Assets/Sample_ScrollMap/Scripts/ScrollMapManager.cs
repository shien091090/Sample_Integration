using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMapManager : MonoBehaviour
{
    public void PrintMapPos(Vector2 pos)
    {
        Debug.Log(string.Format("Pos({0}, {1})", pos.x, pos.y));
    }
}
