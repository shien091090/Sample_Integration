using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

public class SceneTemplate
{
    public enum AnchorType
    {
        None,
        FitMagnify, //放大&緊貼邊界
        SimpleCenter //僅定位至中心點
    }

    [MenuItem("CreateTemplateScene/TesterButton")]
    public static void CreateTemplateScene_ButtonTester()
    {
        Debug.Log("Test");

        //GameObject _canvasGo = new GameObject("Canvas_Main", typeof(Canvas));
        //Canvas _canvasComp = _canvasGo.GetComponent<Canvas>();
        //_canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
        //_canvasComp.worldCamera = Camera.main;

        GameObject _canvasGo = PushComponent<Canvas>("Canvas_Main",
            metaSetting: (canvas) =>
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
            });

        GameObject _backgroundGo = PushComponent<Image>("Background", _canvasGo.transform,
             (transfom) =>
             {
                 SetTransformState(transfom, AnchorType.FitMagnify);
             },
             (image) =>
             {
                 image.color = Color.gray;
             });

        Image _buttonImg = null;
        GameObject _testerButtonGo = PushComponent<Image>("TestButton", _canvasGo.transform,
            (transfom) =>
            {
                //_buttonImg = transfom.gameObject.AddComponent<Image>();

                SetTransformState(transfom, AnchorType.SimpleCenter);

                RectTransform _rect = transfom.GetComponent<RectTransform>();
                _rect.sizeDelta = new Vector2(130, 40);

                
            },
            (button) =>
            {
                //button.targetGraphic = _buttonImg;
            });

        GameObject _buttonTextGo = PushComponent<Text>("Text", _testerButtonGo.transform,
            (transfom) =>
            {
                SetTransformState(transfom, AnchorType.FitMagnify);
            },
            (text)=>
            {
                text.alignment = TextAnchor.MiddleCenter;
                text.text = "測試";
                text.color = Color.black;
            });
    }

    private static GameObject PushComponent<T>(string objectName, Transform parent = null, Action<Transform> transformSetting = null, Action<T> metaSetting = null, params Action<Component>[] addComponentSetting)
    {
        GameObject _go = new GameObject(objectName, typeof(T));

        if (parent != null)
            _go.transform.SetParent(parent);

        if (transformSetting != null)
            transformSetting.Invoke(_go.transform);

        if (metaSetting != null)
        {
            T _comp = _go.GetComponent<T>();
            metaSetting.Invoke(_comp);
        }

        if(addComponentSetting != null && addComponentSetting.Length > 0)
        {

        }

        return _go;
    }

    private static void SetTransformState(Transform target, AnchorType type)
    {
        RectTransform _rect = target.GetComponent<RectTransform>();

        switch (type)
        {
            case AnchorType.FitMagnify:
                target.localScale = Vector3.one;
                target.localPosition = Vector3.zero;

                if (_rect != null)
                {
                    _rect.sizeDelta = Vector2.zero;
                    _rect.anchorMin = Vector2.zero;
                    _rect.anchorMax = Vector2.one;
                }

                break;

            case AnchorType.SimpleCenter:
                target.localScale = Vector3.one;
                target.localPosition = Vector3.zero;
                break;
        }
    }
}
