using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotionDataGroup
{
    public static NotionRichTextObject[] GetRichTextObjectArray(params RichTextSetting[] settings)
    {
        List<NotionRichTextObject> _tempRichTxtList = new List<NotionRichTextObject>();

        if (settings == null || settings.Length <= 0)
            return _tempRichTxtList.ToArray();

        for (int i = 0; i < settings.Length; i++)
        {
            NotionRichTextObject _richTxtObj = ParseRichTextSetting(settings[i]);
            _tempRichTxtList.Add(_richTxtObj);
        }

        return _tempRichTxtList.ToArray();
    }

    private static NotionRichTextObject ParseRichTextSetting(RichTextSetting setting)
    {
        NotionRichTextObject _richTxtObj = new NotionRichTextObject();

        NotionTextObject _contentTxt = new NotionTextObject();
        _contentTxt.content = setting.content;

        _richTxtObj.Text = _contentTxt;
        _richTxtObj.annotations = ParseAnnotationSetting(setting.annotationTypes, setting.colorType);

        return _richTxtObj;
    }

    private static NotionRichTextObject.AnnotationsInfo ParseAnnotationSetting(AnnotationType[] annotations, ColorType color)
    {
        NotionRichTextObject.AnnotationsInfo _annotationInfo = new NotionRichTextObject.AnnotationsInfo();

        if(annotations != null && annotations.Length > 0)
        {
            foreach (AnnotationType _type in annotations)
            {
                switch (_type)
                {
                    case AnnotationType.bold:
                        _annotationInfo.bold = true;
                        break;

                    case AnnotationType.italic:
                        _annotationInfo.italic = true;
                        break;

                    case AnnotationType.strikethrough:
                        _annotationInfo.strikethrough = true;
                        break;

                    case AnnotationType.underline:
                        _annotationInfo.underline = true;
                        break;

                    case AnnotationType.code:
                        _annotationInfo.code = true;
                        break;
                }
            }
        }

        string _colorName = color.ToString().ToLower();
        _annotationInfo.color = _colorName;

        return _annotationInfo;
    }
}

public enum AnnotationType
{
    bold,
    italic,
    strikethrough,
    underline,
    code
}

public enum ColorType
{
    Default,
    gray,
    brown,
    orange,
    yellow,
    green,
    blue,
    purple,
    pink,
    red,
    gray_background,
    brown_background,
    orange_background,
    yellow_background,
    green_background,
    blue_background,
    purple_background,
    pink_background,
    red_background
}

public class RichTextSetting
{
    public string content;
    public ColorType colorType;
    public AnnotationType[] annotationTypes;

    public RichTextSetting(string _content, ColorType _color, params AnnotationType[] _annotations)
    {
        content = _content;
        colorType = _color;
        annotationTypes = _annotations;
    }

    public RichTextSetting(string _content, params AnnotationType[] _annotations)
    {
        content = _content;
        colorType = ColorType.Default;
        annotationTypes = _annotations;
    }
}

public class NotionTextObject
{
    public string content;
}

public class NotionRichTextObject
{
    public class AnnotationsInfo
    {
        public bool bold;
        public bool italic;
        public bool strikethrough;
        public bool underline;
        public bool code;
        public string color;
    }

    public AnnotationsInfo annotations;
    public NotionTextObject Text;
}