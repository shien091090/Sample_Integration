using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareContoller : MonoBehaviour
{
    [SerializeField] private RectTransform square;

    public void SetPosX(float posX)
    {
        square.anchoredPosition = new Vector2(posX, square.anchoredPosition.y);
    }

    public void SetPosY(float posY)
    {
        square.anchoredPosition = new Vector2(square.anchoredPosition.x, posY);
    }

    public void SetWidth(float width)
    {
        square.sizeDelta = new Vector2(width, square.sizeDelta.y);
    }

    public void SetHeight(float height)
    {
        square.sizeDelta = new Vector2(square.sizeDelta.x, height);
    }

    public void SetPivotX(float pivotX)
    {
        square.pivot = new Vector2(pivotX, square.pivot.y);
    }

    public void SetPivotY(float pivotY)
    {
        square.pivot = new Vector2(square.pivot.x, pivotY);
    }

}
