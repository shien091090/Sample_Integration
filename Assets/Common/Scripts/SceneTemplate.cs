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

    public enum ComponentType
    {
        AddComponent,
        DefaultComponent
    }

    public class ComponentSetting
    {
        public Type comp;
        public ComponentType type;
        public Action<Component> metaSetting;
    }

    [MenuItem("CreateTemplateScene/TesterButton")]
    public static void CreateTemplateScene_ButtonTester()
    {
        GameObject _canvasGo = PushComponent("Canvas_Main", componentSetting:
            new ComponentSetting()
            {
                comp = typeof(Canvas),
                type = ComponentType.AddComponent,
                metaSetting = (canvas) =>
                {
                    Canvas _canvas = (Canvas)canvas;
                    _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    _canvas.worldCamera = Camera.main;
                }
            });

        PushComponent("Background", _canvasGo.transform,
            new ComponentSetting()
            {
                comp = typeof(Image),
                type = ComponentType.AddComponent,
                metaSetting = (image) =>
                {
                    Image _img = (Image)image;
                    _img.color = Color.gray;
                }
            },
            new ComponentSetting()
            {
                comp = typeof(Transform),
                type = ComponentType.DefaultComponent,
                metaSetting = (transform) =>
                {
                    Transform _trans = (Transform)transform;
                    SetTransformState(_trans, AnchorType.FitMagnify);
                }
            });

        Image _buttonImg = null;
        GameObject _testerButtonGo = PushComponent("TestButton", _canvasGo.transform,
            new ComponentSetting()
            {
                comp = typeof(Image),
                type = ComponentType.AddComponent,
                metaSetting = (image) =>
                 {
                     Image _img = (Image)image;
                     _buttonImg = _img;
                 }
            },
            new ComponentSetting()
            {
                comp = typeof(Button),
                type = ComponentType.AddComponent,
                metaSetting = (button) =>
                {
                    Button _btn = (Button)button;
                    _btn.targetGraphic = _buttonImg;
                }
            },
            new ComponentSetting()
            {
                comp = typeof(Transform),
                type = ComponentType.DefaultComponent,
                metaSetting = (transform) =>
                {
                    Transform _trans = (Transform)transform;
                    SetTransformState(_trans, AnchorType.SimpleCenter);

                    RectTransform _rect = _trans.GetComponent<RectTransform>();
                    _rect.sizeDelta = new Vector2(130, 40);
                }
            });

        PushComponent("Text", _testerButtonGo.transform,
            new ComponentSetting()
            {
                comp = typeof(Text),
                type = ComponentType.AddComponent,
                metaSetting = (text) =>
                  {
                      Text _txt = (Text)text;
                      _txt.alignment = TextAnchor.MiddleCenter;
                      _txt.text = "測試";
                      _txt.color = Color.black;
                  }
            },
            new ComponentSetting()
            {
                comp = typeof(Transform),
                type = ComponentType.DefaultComponent,
                metaSetting = (transform) =>
                {
                    Transform _trans = (Transform)transform;
                    SetTransformState(_trans, AnchorType.FitMagnify);
                }
            });

        PushComponent("---------------------");

        PushComponent("ScriptHolder");
    }

    private static GameObject PushComponent(string objectName, Transform parent = null, params ComponentSetting[] componentSetting)
    {
        GameObject _go = new GameObject(objectName);

        if (parent != null)
            _go.transform.SetParent(parent);

        for (int i = 0; i < componentSetting.Length; i++)
        {
            Type _compType = componentSetting[i].comp;
            Component _comp = null;

            switch (componentSetting[i].type)
            {
                case ComponentType.AddComponent:
                    _comp = _go.AddComponent(_compType);
                    break;

                case ComponentType.DefaultComponent:
                    _comp = _go.GetComponent(_compType);
                    break;
            }

            if (_comp != null)
                componentSetting[i].metaSetting.Invoke(_comp);
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
