using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SNShien.Common.TesterTools;

public enum IOType
{
    Stream,
    File
}

public enum FileType
{
    CSV,
    TXT
}

public enum ContentLanguageType
{
    Digits,
    Alphabet,
    Chinese
}

public enum EncodingType
{
    UTF8,
    UTF32
}

public enum IOActionType
{
    None,
    Save,
    Load
}

public class FileIOManager : MonoBehaviour
{
    private const string FILE_PATH_FORMAT = @"Assets/TempFile/TestFile{0}.{1}";
    private const string FILE_PATH_RUN_RESULT = @"Assets/TempFile/RunResultRecord.csv";
    private const string UNICODE_PREFIX_ALPHABET = @"\u00";
    private const int UNICODE_START_VALUE_ALPHABET = 41;
    private const string UNICODE_PREFIX_CHINESE = @"\u4e";
    private const int UNICODE_START_VALUE_CHINESE = 1;

    [Header("Save")]
    [SerializeField] private int columnCount;
    [SerializeField] private int rowCount;
    [SerializeField] private IOType saveIOType;
    [SerializeField] private FileType saveFileType;
    [SerializeField] private ContentLanguageType contentLanguageType;
    [SerializeField] private EncodingType saveEncodingType;

    [Header("Load")]
    [SerializeField] private IOType loadIOType;
    [SerializeField] private EncodingType loadEncogingType;

    [Header("UI Ref")]
    [SerializeField] private Text txt_dataQuantity;
    [SerializeField] private Text txt_langType;
    [SerializeField] private Text txt_saveEncodeType;
    [SerializeField] private Text txt_saveFuncType;
    [SerializeField] private Text txt_fileType;
    [SerializeField] private Text txt_loadEncodeType;
    [SerializeField] private Text txt_loadFuncType;
    [SerializeField] private InputField input_gc;
    [SerializeField] private InputField input_fileSize;
    [SerializeField] private Button btn_recordResult;

    [Header("TempParam")]
    [SerializeField] private string preLoadFilePath;

    private StreamWriter recordWriter;
    private List<int> settingRecords;
    private List<string> tempRunResult;
    private IOActionType tempResultActionType;

    void Start()
    {
        recordWriter = new StreamWriter(FILE_PATH_RUN_RESULT, true, Encoding.GetEncoding("utf-8"));
    }

    private void OnDestroy()
    {
        if (recordWriter != null)
        {
            recordWriter.Close();
            recordWriter.Dispose();
        }
    }

    void Update()
    {
        if (CheckSettingIsSame() == false)
        {
            Debug.Log("Setting Is Changed");
            RecordCurrentSettingsValue();
            SetSettingText();
        }
    }

    public void BTN_SaveFile()
    {
        SaveFile(saveIOType, saveFileType, saveEncodingType);
    }

    public void BTN_LoadFile()
    {
        LoadFile(loadIOType, loadEncogingType);
    }

    public void BTN_RecordResult()
    {
        if (tempResultActionType != IOActionType.None && tempRunResult != null && tempRunResult.Count > 0)
        {
            tempRunResult.Add($"{input_gc.text} mb");
            tempRunResult.Add($"{input_fileSize.text} kb");

            string _finalConetnt = string.Join(",", tempRunResult);

            recordWriter.WriteLine(_finalConetnt);
            recordWriter.Flush();
            //recordWriter.Close();
            //recordWriter.Dispose();
        }

        tempResultActionType = IOActionType.None;
        tempRunResult = new List<string>();
        btn_recordResult.interactable = false;
    }

    private string CreateOneRowContent(int column, ContentLanguageType languageType)
    {
        List<string> _rowContents = new List<string>();
        for (int i = 0; i < columnCount; i++)
        {
            string _t = string.Empty;
            switch (languageType)
            {
                case ContentLanguageType.Digits:
                    _t = (i + 1).ToString();
                    break;

                case ContentLanguageType.Alphabet:
                    {
                        string _unicode = $"{UNICODE_PREFIX_ALPHABET}{UNICODE_START_VALUE_ALPHABET + i}";
                        _t = Regex.Unescape(_unicode);
                    }
                    break;

                case ContentLanguageType.Chinese:
                    {
                        string _unicode = $"{UNICODE_PREFIX_CHINESE}{(UNICODE_START_VALUE_CHINESE + i).ToString("00")}";
                        _t = Regex.Unescape(_unicode);
                    }
                    break;

            }

            _rowContents.Add(_t);
        }
        string _content = string.Join(",", _rowContents);

        return _content;
    }

    private List<string> CreateAllContents(int column, int row, ContentLanguageType languageType)
    {
        string _oneRowContent = CreateOneRowContent(column, languageType);

        List<string> _allContents = new List<string>();
        for (int i = 0; i < row; i++)
        {
            _allContents.Add(_oneRowContent);
        }

        return _allContents;
    }

    private void SaveFile(IOType ioType, FileType fileType, EncodingType encodingType)
    {
        string _timeStamp = DateTime.Now.ToString("MMddHHmmss");
        string _fileNameExt = fileType == FileType.CSV ? "csv" : "txt";
        string _filePath = string.Format(FILE_PATH_FORMAT, _timeStamp, _fileNameExt);
        string _encodingType = encodingType == EncodingType.UTF32 ? "utf-32" : "utf-8";

        preLoadFilePath = _filePath;

        Action TestTimerAction = null;
        switch (ioType)
        {
            case IOType.Stream:
                {
                    string _oneRowContent = CreateOneRowContent(columnCount, contentLanguageType);

                    TestTimerAction = () =>
                    {
                        StringWriter w = new StringWriter();
                        StreamWriter writer = new StreamWriter(_filePath, false, Encoding.GetEncoding(_encodingType));
                        for (int i = 0; i < rowCount; i++)
                        {
                            writer.WriteLine(_oneRowContent);
                        }

                        writer.Close();
                    };

                }
                break;

            case IOType.File:
                {
                    List<string> _writeContents = CreateAllContents(columnCount, rowCount, contentLanguageType);

                    TestTimerAction = () =>
                    {
                        File.WriteAllLines(_filePath, _writeContents, Encoding.GetEncoding(_encodingType));
                    };

                }
                break;
        }

        if (TestTimerAction != null)
        {
            long _costTimeMs = MyStopwatch.TimerTest(TestTimerAction, MyStopwatch.TimeUnit.Milliseconds);
            SaveTempRunResult(_costTimeMs, IOActionType.Save);
        }

    }

    private void LoadFile(IOType ioType, EncodingType encodingType)
    {
        string _encodingType = encodingType == EncodingType.UTF32 ? "utf-32" : "utf-8";
        ICollection<string> _loadContents = null;
        Action TestTimerAction = null;
        switch (ioType)
        {
            case IOType.Stream:
                {
                    TestTimerAction = () =>
                    {
                        _loadContents = new List<string>();
                        StreamReader reader = new StreamReader(preLoadFilePath, Encoding.GetEncoding(_encodingType));

                        string _line = string.Empty;
                        while ((_line = reader.ReadLine()) != null)
                        {
                            _loadContents.Add(_line);
                        }

                    };

                }
                break;

            case IOType.File:
                {
                    TestTimerAction = () =>
                    {
                        _loadContents = File.ReadAllLines(preLoadFilePath, Encoding.GetEncoding(_encodingType));
                    };

                }
                break;
        }

        if (TestTimerAction == null)
            return;

        long _costTimeMs = MyStopwatch.TimerTest(TestTimerAction, MyStopwatch.TimeUnit.Milliseconds);

        string _oneRowContent = string.Empty;
        int _columnCount = 0;
        if (_loadContents.Count > 0)
        {
            _oneRowContent = _loadContents.ToList()[0];
            _columnCount = _oneRowContent.Split(',').Length;
        }

        string _log = $"[LoadCSVFile] OneRowContent = {_oneRowContent}\nColumnCount = {_columnCount}, RowCount = {_loadContents.Count} ";
        Debug.Log(_log);

        SaveTempRunResult(_costTimeMs, IOActionType.Load);
    }

    private bool CheckSettingIsSame()
    {
        if (settingRecords == null || settingRecords.Count <= 0)
            return false;

        List<int> _currentSettings = new List<int>
        {
            columnCount,
            rowCount,
            (int)saveIOType,
            (int)saveFileType,
            (int)contentLanguageType,
            (int)saveEncodingType,
            (int)loadIOType,
            (int)loadEncogingType
        };

        if (settingRecords.Count != _currentSettings.Count)
            return false;

        for (int i = 0; i < _currentSettings.Count; i++)
        {
            int _currentValue = _currentSettings[i];
            int _recordValue = settingRecords[i];

            if (_currentValue != _recordValue)
                return false;
        }

        return true;

    }

    private void RecordCurrentSettingsValue()
    {
        settingRecords = new List<int>
        {
            columnCount,
            rowCount,
            (int)saveIOType,
            (int)saveFileType,
            (int)contentLanguageType,
            (int)saveEncodingType,
            (int)loadIOType,
            (int)loadEncogingType
        };
    }

    private void SetSettingText()
    {
        txt_dataQuantity.text = $"{columnCount}欄{rowCount}列";
        txt_langType.text = contentLanguageType.ToString();
        txt_saveEncodeType.text = saveEncodingType.ToString();
        txt_saveFuncType.text = saveIOType.ToString();
        txt_fileType.text = saveFileType.ToString();
        txt_loadFuncType.text = loadIOType.ToString();
        txt_loadEncodeType.text = loadEncogingType.ToString();
    }

    private void SaveTempRunResult(long costTime, IOActionType actionType)
    {
        List<string> contents = null;
        switch (actionType)
        {
            case IOActionType.Save:
                contents = new List<string>
                {
                    txt_dataQuantity.text,
                    txt_langType.text,
                    txt_saveEncodeType.text,
                    txt_saveFuncType.text,
                    txt_fileType.text,
                    $"{costTime} ms"
                };
                break;

            case IOActionType.Load:
                contents = new List<string>
                {
                    txt_dataQuantity.text,
                    txt_langType.text,
                    txt_loadEncodeType.text,
                    txt_loadFuncType.text,
                    txt_fileType.text,
                    $"{costTime} ms"
                };
                break;
        }

        tempRunResult = contents;
        tempResultActionType = actionType;

        btn_recordResult.interactable = true;
    }

}
