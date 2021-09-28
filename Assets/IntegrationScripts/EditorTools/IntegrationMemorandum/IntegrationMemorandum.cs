using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class IntegrationMemorandum : EditorWindow
{
    private const string PATH_SCRIPT = @"Assets\IntegrationScripts";
    private const string PATH_ASSET = @"Assets";
    private static IntegrationMemorandum _mainWindow;

    [MenuItem("SNTool/IntegrationMemorandum")]
    public static void Init()
    {
        _mainWindow = GetWindow<IntegrationMemorandum>(true, "整合備忘錄(Integration Memorandum)");
        _mainWindow.minSize = new Vector2(340, 390);
        _mainWindow.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Test"))
        {
            string[] _files = GetAssetFiles(PATH_SCRIPT);
            //Debug.Log(GetAssetFileContent("SceneTemplate.cs", _files));

            foreach (string f in _files)
            {
                Debug.Log(GetParentFolderPath(f));
            }
        }
    }

    private static string[] GetAssetFiles(string topFolderPath)
    {
        string[] _files = Directory.GetFiles(topFolderPath, "*.cs", SearchOption.AllDirectories);
        return _files;
    }

    private static string GetAssetFileContent(string fileName, string[] allFiles)
    {
        string[] _filterFiles = allFiles
            .Where(x => GetFileName(x) == fileName)
            .ToArray();

        if (_filterFiles.Length != 1)
            return string.Empty;

        string _fileContent = File.ReadAllText(_filterFiles[0]);
        return _fileContent;
    }

    private static string GetFullPath(string internalPath)
    {
        string _unityProjectPath = Path.GetFullPath(Application.dataPath);

        string _keywords = "Assets";
        int _keywordIndex = internalPath.IndexOf(_keywords);
        if (_keywordIndex >= 0)
            internalPath = internalPath.Remove(_keywordIndex, _keywords.Length);

        return _unityProjectPath + internalPath;
    }

    private static string GetParentFolderPath(string filePath)
    {
        string _resultPath = filePath;
        int _slashIndex = _resultPath.LastIndexOf("/");
        int _backSlashIndex = _resultPath.LastIndexOf(@"\");

        if (_slashIndex == -1 && _backSlashIndex == -1)
            return string.Empty;

        int _rejectIndex = Mathf.Max(_slashIndex, _backSlashIndex);
        _resultPath = _resultPath.Remove(_rejectIndex);

        return _resultPath;
    }

    private static string GetFileName(string filePath)
    {
        string _name = filePath;
        _name = ReplaceSlash(true, _name);
        int _charIndex = _name.LastIndexOf("/");
        _name = _name.Substring(_charIndex + 1);

        return _name;
    }

    private static string ReplaceSlash(bool slashType, string content)
    {
        string _result = content;

        if (slashType) //轉成正斜線"/"
            _result = _result.Replace("\\", "/");
        else //轉成反斜線"\\"
            _result = _result.Replace("/", "\\");

        return _result;
    }
}
