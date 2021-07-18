using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NotionAPITest : MonoBehaviour
{
    public class NotionPageCreator
    {
        public Dictionary<string, string> parent;
        public Dictionary<string, NotionProperty> properties;

        public NotionPageCreator()
        {
            parent = new Dictionary<string, string>();
            properties = new Dictionary<string, NotionProperty>();
        }
    }

    public class NotionProperty { }

    public class NotionProp_PageTitle : NotionProperty
    {
        public List<NotionUnit> title;

        public NotionProp_PageTitle()
        {
            title = new List<NotionUnit>();
        }
    }

    public class NotionProp_RichText : NotionProperty
    {
        public List<NotionUnit> Details;

        public NotionProp_RichText()
        {
            Details = new List<NotionUnit>();
        }
    }

    public class NotionUnit
    {
        //public string type;
        public Dictionary<string, string> text;

        public NotionUnit()
        {
            text = new Dictionary<string, string>();
        }
    }

    public void BTN_Test()
    {
        NotionPageCreator _notionNewPage = new NotionPageCreator();

        _notionNewPage.parent.Add("page_id", "e5de0bc52e834047aa29742235991eb7");

        NotionUnit _titleText = new NotionUnit();
        _titleText.text.Add("content", "TestABC");

        NotionProp_PageTitle _newTitle = new NotionProp_PageTitle();
        _newTitle.title = new List<NotionUnit>() { _titleText };

        _notionNewPage.properties.Add("title", _newTitle);

        NotionUnit _richText = new NotionUnit();
        _richText.text.Add("content", "123456789");

        NotionProp_RichText _newRichTxt = new NotionProp_RichText();
        _newRichTxt.Details = new List<NotionUnit>() { _richText };

        _notionNewPage.properties.Add("rich_text", _newRichTxt);

        string _result = JsonConvert.SerializeObject(_notionNewPage);
        Debug.Log(_result);
    }
}
