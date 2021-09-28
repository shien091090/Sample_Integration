using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ComponentFinder_ResultView : EditorWindow
{
    public enum FilterType
    {
        符合一個即可,
        需全部符合
    }

    public static ComponentFinder.PrefabSearchResult ResultData { set; private get; }

    private const string FOCUS_FILTER_BUTTON = "FOCUS_FILTER_BUTTON";
    private static Vector2 _scrollPos = Vector2.zero;
    private static List<string> filterStringList;
    private static string[] filterTypeNames;

    private static string filterString;
    private static int filterTypeIndex;

    private void OnGUI()
    {
        if (ResultData != null)
            ShowSearchResultLayout(ResultData);
        else
            Close();
    }

    private void OnDestroy()
    {
        if (ResultData != null)
            ResultData = null;
    }

    private static void ShowSearchResultLayout(ComponentFinder.PrefabSearchResult data)
    {
        if (data.Dict_searchResult == null || data.Dict_searchResult.Count <= 0)
            return;

        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        ShowFilterPanel();
        GUILayout.Space(customGui.space_backgroundInterval);
        ShowResultLabel(data.ComponentName, data.TotalPrefabCount, data.TotalComponentCount);
        GUILayout.Space(customGui.space_backgroundInterval);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        foreach (KeyValuePair<string, ComponentFinder.SearchPrefabInfo> obj in data.Dict_searchResult)
        {
            EditorGUILayout.BeginVertical();
            {
                if (CheckFilter(obj.Key, obj.Value.AssetPath))
                {
                    ShowPrefabFocusButton(obj.Key, obj.Value.ProjectObject);
                    ShowPathLabel(obj.Value.AssetPath);
                    ShowComponentsField(obj.Value.Components);

                    GUILayout.Space(customGui.space_componentField);
                }
            }
            EditorGUILayout.EndVertical();

        }

        EditorGUILayout.EndScrollView();

    }

    private static void ShowFilterPanel()
    {
        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        EditorGUILayout.BeginVertical(customGui.styleName_panelBackground);
        {
            GUILayout.Space(customGui.space_backgroundInside);

            EditorGUILayout.LabelField("<color=yellow>【篩選面板】: 從Prefab名稱和路徑中篩選指定字段</color>", customGui.resultLabelTitle_style);

            EditorGUILayout.BeginHorizontal();
            {
                filterString = EditorGUILayout.TextField("請輸入篩選字段 : ", filterString);

                GUI.SetNextControlName(FOCUS_FILTER_BUTTON);
                if (GUILayout.Button("加入字段"))
                    AddFilterStringButton(filterString, FilterStringErrorPopUp);
            }
            EditorGUILayout.EndHorizontal();

            ShowFilterTypeDropdown();

            EditorGUILayout.LabelField("只顯示包含以下字段的結果 : ");

            ShowFilterStringButton();

            GUILayout.Space(customGui.space_backgroundInside);
        }
        EditorGUILayout.EndVertical();
    }

    private static void ShowFilterTypeDropdown()
    {
        if (filterTypeNames == null || filterTypeNames.Length <= 0)
            filterTypeNames = Enum.GetNames(typeof(FilterType));

        filterTypeIndex = EditorGUILayout.Popup("篩選模式", filterTypeIndex, filterTypeNames);

    }

    private static void AddFilterStringButton(string filterStr, Action<string> failedCallback)
    {
        if (string.IsNullOrEmpty(filterStr))
        {
            failedCallback.Invoke("輸入字段有誤");
            return;
        }

        if (filterStringList == null)
            filterStringList = new List<string>();

        if (filterStringList.Contains(filterStr))
        {
            failedCallback.Invoke("已存在相同字段");
            return;
        }

        filterStringList.Add(filterStr);
        ClearFilterStringTextField();
    }

    private static void FilterStringErrorPopUp(string message)
    {
        EditorUtility.DisplayDialog("輸入錯誤", message, "確定");
        ClearFilterStringTextField();
    }

    private static void ClearFilterStringTextField()
    {
        filterString = string.Empty;
        EditorGUI.FocusTextInControl(FOCUS_FILTER_BUTTON);
    }

    private static void ShowResultLabel(string componentName, int prefabCount, int componentCount)
    {
        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        EditorGUILayout.BeginVertical(customGui.styleName_panelBackground);
        {
            GUILayout.Space(customGui.space_backgroundInside);

            EditorGUILayout.LabelField("<color=yellow>【搜尋結果】</color>", customGui.resultLabelTitle_style);

            EditorGUILayout.LabelField(string.Format("指定Component : {0}, 搜尋結果如下 : ", componentName));
            EditorGUILayout.LabelField(string.Format("Prefab共 {0} 個, 掛載Component物件共 {1} 個", prefabCount, componentCount));

            GUILayout.Space(customGui.space_backgroundInside);
        }
        EditorGUILayout.EndVertical();
    }

    private static void ShowFilterStringButton()
    {
        if (filterStringList == null || filterStringList.Count <= 0)
            return;

        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < filterStringList.Count; i++)
        {
            if (i > 0 && i % customGui.limit_filterButtonCount == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }

            if (GUILayout.Button(filterStringList[i], customGui.filterButton_option))
            {
                filterStringList.Remove(filterStringList[i]);
                break;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private static bool CheckFilter(string prefabName, string path)
    {
        if (filterStringList == null || filterStringList.Count <= 0)
            return true;

        FilterType _filterType = (FilterType)filterTypeIndex;

        for (int i = 0; i < filterStringList.Count; i++)
        {
            string _str = filterStringList[i];

            switch (_filterType)
            {
                case FilterType.符合一個即可:
                    if (prefabName.Contains(_str) || path.Contains(_str))
                        return true;
                    break;

                case FilterType.需全部符合:
                    if (!prefabName.Contains(_str) && !path.Contains(_str))
                        return false;
                    break;
            }
        }

        switch (_filterType)
        {
            case FilterType.符合一個即可:
                return false;
            case FilterType.需全部符合:
                return true;
        }

        return false;
    }

    private static void ShowPrefabFocusButton(string prefabName, UnityEngine.Object projectObject)
    {
        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        if (GUILayout.Button(prefabName, customGui.prefabFocusButton_option))
        {
            EditorGUIUtility.PingObject(projectObject);
        }
    }

    private static void ShowPathLabel(string path)
    {
        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        EditorGUILayout.LabelField("Prefab路徑", path, customGui.prefabPath_style);
    }

    private static void ShowComponentsField(List<Component> comps)
    {
        ComponentFinder.CustomGUISetting customGui = ComponentFinder.CustomGUI;

        for (int i = 0; i < comps.Count; ++i)
        {
            Component tmp_comp = comps[i];
            tmp_comp = EditorGUILayout.ObjectField("物件", tmp_comp, typeof(Component), true, customGui.componentField_option) as Component;
        }
    }

}
