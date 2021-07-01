using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ComponentFinder : EditorWindow
{
    public class SearchPrefabInfo
    {
        public List<Component> Components { private set; get; }
        public UnityEngine.Object ProjectObject { private set; get; }
        public string AssetPath { private set; get; }

        public SearchPrefabInfo(UnityEngine.Object _obj, string _path)
        {
            ProjectObject = _obj;
            AssetPath = _path;
            Components = new List<Component>();
        }
    }

    public class TypeNameSearchResult
    {
        public string SearchTypeName { private set; get; }
        public Type[] SearchResultTypes { private set; get; }

        public TypeNameSearchResult(string _typeName, Type[] _resultTypes)
        {
            SearchTypeName = _typeName;
            SearchResultTypes = _resultTypes;
        }
    }

    public class PrefabSearchResult
    {
        public Dictionary<string, SearchPrefabInfo> Dict_searchResult { private set; get; }
        public string ComponentName { private set; get; }
        public int TotalPrefabCount { private set; get; }
        public int TotalComponentCount { private set; get; }

        public PrefabSearchResult()
        {
            Dict_searchResult = new Dictionary<string, SearchPrefabInfo>();
            Clear();
        }

        public void Clear()
        {
            Dict_searchResult.Clear();
            ComponentName = string.Empty;
            TotalPrefabCount = 0;
            TotalComponentCount = 0;
        }

        public void SetSearchInfo(string _compName, int _prefabCount, int _compCount)
        {
            ComponentName = _compName;
            TotalPrefabCount = _prefabCount;
            TotalComponentCount = _compCount;
        }
    }

    public class CustomGUISetting
    {
        public int space_componentField;
        public int space_backgroundInside;
        public int space_backgroundInterval;
        public int limit_filterButtonCount;

        public string styleName_panelBackground;

        public GUILayoutOption[] searchField_option;
        public GUIStyle searchField_style;
        public GUILayoutOption[] searchButton_option;
        public GUILayoutOption[] searchLabel_option;
        public GUIStyle searchLabel_style;
        public GUILayoutOption[] typeSelectionButton_option;
        public GUIStyle typeSelectionButton_style;
        public GUILayoutOption[] prefabFocusButton_option;
        public GUIStyle prefabPath_style;
        public GUILayoutOption[] componentField_option;
        public GUILayoutOption[] filterButton_option;
        public GUIStyle resultLabelTitle_style;
    }
    private static CustomGUISetting _customGui;
    public static CustomGUISetting CustomGUI
    {
        get
        {
            if (_customGui == null)
                InitGUISetting();

            return _customGui;
        }
    }

    private static string _assetsfolder = @"Assets";
    private static int typeSearchResultMax = 10;
    private static ComponentFinder componentFinder_Window;
    private static ComponentFinder_ResultView subWindow;

    private static string currentSearchTypeName;
    private static TypeNameSearchResult typeNameSearchData;

    [MenuItem("SNTool/ComponentFinder")]
    private static void Init()
    {
        InitSearchData();
        InitGUISetting();
        InitWindow();
    }

    private static void InitSearchData()
    {
        currentSearchTypeName = string.Empty;
        typeNameSearchData = null;
    }

    private static void InitGUISetting()
    {
        _customGui = new CustomGUISetting();

        _customGui.space_componentField = 18;
        _customGui.space_backgroundInside = 15;
        _customGui.space_backgroundInterval = 5;
        _customGui.limit_filterButtonCount = 3;

        _customGui.styleName_panelBackground = "AnimLeftPaneSeparator";

        _customGui.searchField_option = new GUILayoutOption[] { GUILayout.Height(23), GUILayout.Width(110) };
        _customGui.searchField_style = new GUIStyle("TextField") { alignment = TextAnchor.MiddleLeft };

        _customGui.searchButton_option = new GUILayoutOption[] { GUILayout.Height(23) };
        _customGui.searchLabel_option = new GUILayoutOption[] { GUILayout.Height(23) };
        _customGui.searchLabel_style = new GUIStyle("Label") { alignment = TextAnchor.MiddleLeft };

        _customGui.typeSelectionButton_option = new GUILayoutOption[] { GUILayout.Height(30) };
        _customGui.typeSelectionButton_style = new GUIStyle("Button") { alignment = TextAnchor.MiddleCenter };

        _customGui.prefabFocusButton_option = new GUILayoutOption[] { GUILayout.Height(28) };

        _customGui.prefabPath_style = new GUIStyle("Label") { richText = true };

        _customGui.componentField_option = new GUILayoutOption[] { };

        _customGui.filterButton_option = new GUILayoutOption[] { GUILayout.Width(130) };

        _customGui.resultLabelTitle_style = new GUIStyle("Label") { richText = true };
    }

    private static void InitWindow()
    {
        componentFinder_Window = GetWindow<ComponentFinder>(true, "ComponentFinder");
        componentFinder_Window.minSize = new Vector2(340, 390);
        componentFinder_Window.Show();
    }

    private void OnGUI()
    {
        Layout_SearchTypeName();
    }

    private void OnDestroy()
    {
        if (subWindow != null)
            subWindow.Close();
    }

    private static void Layout_SearchTypeName()
    {
        EditorGUILayout.BeginHorizontal();

        currentSearchTypeName = EditorGUILayout.TextField(currentSearchTypeName, CustomGUI.searchField_style, CustomGUI.searchField_option);

        bool _onSearchTypeClicked = GUILayout.Button("搜尋類型", CustomGUI.searchButton_option);
        bool _isSearchKeyInvalid = string.IsNullOrEmpty(currentSearchTypeName);

        if (_onSearchTypeClicked && _isSearchKeyInvalid)
            typeNameSearchData = null;
        else if (_onSearchTypeClicked)
        {
            Assembly[] _targetAssemblies = GetAssembliesByType(
                typeof(Button),
                typeof(TMPro.TextMeshProUGUI),
                typeof(MonoBehaviour));

            Type[] _resultTypes = FindTypesByAssemblies(currentSearchTypeName, _targetAssemblies);
            typeNameSearchData = new TypeNameSearchResult(currentSearchTypeName, _resultTypes);

        }

        bool _isSearched = typeNameSearchData != null;
        bool _isSearchSuccess = _isSearched && typeNameSearchData.SearchResultTypes != null && typeNameSearchData.SearchResultTypes.Length > 0;
        string _searchStateLabel = !_isSearched ? "尚未開始搜尋" : _isSearchSuccess ? "搜尋成功, 以下是搜尋結果 : " : _isSearchKeyInvalid ? "關鍵字錯誤" : "沒有找到結果";

        EditorGUILayout.LabelField(_searchStateLabel, CustomGUI.searchLabel_style, CustomGUI.searchLabel_option);

        EditorGUILayout.EndHorizontal();

        if (_isSearchSuccess)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("選擇欲搜尋的Component");
            ShowTypeNameSelection(typeNameSearchData.SearchResultTypes);
            EditorGUILayout.EndVertical();
        }
    }

    private static Assembly[] GetAssembliesByType(params Type[] _types)
    {
        List<Assembly> _assemblyList = new List<Assembly>();

        if (_types == null || _types.Length <= 0)
            return _assemblyList.ToArray();

        for (int i = 0; i < _types.Length; i++)
        {
            try
            {
                Assembly _assembly = _types[i].Assembly;

                if (!_assemblyList.Contains(_assembly))
                    _assemblyList.Add(_assembly);
            }
            catch (Exception)
            {
                continue;
            }
        }

        return _assemblyList.ToArray();
    }

    private static Type[] FindTypesByAssemblies(string searchName, params Assembly[] _assemblies)
    {
        List<Type> _filterResult = new List<Type>();

        List<Type> _similarResult = new List<Type>();
        List<Type> _fitResult = new List<Type>();

        if (_assemblies == null || _assemblies.Length <= 0)
            return _filterResult.ToArray();

        for (int i = 0; i < _assemblies.Length; i++)
        {
            List<Type> _similarTypes = _assemblies[i].GetTypes()
                .Where(x => x.IsSubclassOf(typeof(MonoBehaviour)))
                .Where(x => x.Name.Contains(searchName))
                .ToList();

            if (_similarTypes == null || _similarTypes.Count <= 0)
                continue;

            FillUpTypeList(_similarResult, _similarTypes, typeSearchResultMax);

            List<Type> _fitTypes = _similarTypes
                .Where(x => x.Name == searchName)
                .ToList();

            if (_fitTypes != null && _fitTypes.Count > 0)
                FillUpTypeList(_fitResult, _fitTypes, typeSearchResultMax);

        }

        FillUpTypeList(_filterResult, _fitResult, typeSearchResultMax);
        FillUpTypeList(_filterResult, _similarResult, typeSearchResultMax);

        return _filterResult.ToArray();

    }

    private static void FillUpTypeList(List<Type> target, List<Type> source, int maxLength)
    {
        if (target == null)
            target = new List<Type>();

        if (source == null || source.Count <= 0)
            return;

        for (int i = 0; i < source.Count; i++)
        {
            Type _type = source[i];

            if (!target.Contains(_type))
                target.Add(_type);

            if (target.Count >= maxLength)
                break;
        }
    }

    private static void ShowTypeNameSelection(Type[] searchResultTypes)
    {
        if (searchResultTypes == null || searchResultTypes.Length <= 0)
            return;

        for (int i = 0; i < searchResultTypes.Length; i++)
        {
            Type _type = searchResultTypes[i];

            if (GUILayout.Button(_type.FullName, CustomGUI.typeSelectionButton_style, CustomGUI.typeSelectionButton_option))
            {
                FindCompoment(_type, ShowResultWindow);
            }
        }
    }

    private static void FindCompoment(Type searchType, Action<PrefabSearchResult> callBack)
    {
        PrefabSearchResult _resultData = new PrefabSearchResult();

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { _assetsfolder });
        int _totalPrefabCount = 0;
        int _totalComponentCount = 0;
        int _guidIndex = 0;

        foreach (string guid in guids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject obj_root = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            Component[] componentInPrefab = obj_root.GetComponentsInChildren(searchType, true)
                .Where(x => x.GetType() == searchType)
                .ToArray();
            _guidIndex++;

            EditorUtility.DisplayProgressBar("Find Compoment", string.Format("({0}/{1}) Search Prefab ... {2}", _guidIndex, guids.Length, obj_root.name), Mathf.InverseLerp(0, guids.Length, _guidIndex));

            if (componentInPrefab != null && componentInPrefab.Length > 0)
            {
                _totalPrefabCount += 1;
                _totalComponentCount += componentInPrefab.Length;

                for (int i = 0; i < componentInPrefab.Length; ++i)
                {
                    Component temp_comp = componentInPrefab[i];

                    if (!_resultData.Dict_searchResult.ContainsKey(obj_root.name))
                    {
                        SearchPrefabInfo _info = new SearchPrefabInfo(obj_root, GetParentFolderPath(prefabPath));
                        _resultData.Dict_searchResult[obj_root.name] = _info;
                    }

                    _resultData.Dict_searchResult[obj_root.name].Components.Add(temp_comp);
                }
            }
        }

        EditorUtility.ClearProgressBar();
        _resultData.SetSearchInfo(searchType.FullName, _totalPrefabCount, _totalComponentCount);

        if (callBack != null)
            callBack.Invoke(_resultData);
    }

    private static void ShowResultWindow(PrefabSearchResult resultData)
    {
        subWindow = GetWindow<ComponentFinder_ResultView>(true, "Search Result");
        subWindow.minSize = new Vector2(420, 700);
        ComponentFinder_ResultView.ResultData = resultData;
        subWindow.Show();
    }

    private static string GetHightlightPathString(string fullPath, string parentPath)
    {
        string _mainPath = fullPath.Replace(parentPath, string.Empty);
        int _rejectCharIndex = _mainPath.LastIndexOf("/");
        _mainPath = _mainPath.Remove(_rejectCharIndex);

        string _result = string.Format("<color=grey>{0}</color><color=yellow>{1}</color>", parentPath, _mainPath);

        return _result;
    }

    private static string GetParentFolderPath(string fullPath)
    {
        string result = fullPath;
        int slashIndex = result.LastIndexOf("/");

        if (slashIndex < 0)
            return result;

        result = result.Remove(slashIndex);
        return result;
    }

}
