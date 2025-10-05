#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TodoWindow : EditorWindow
{
    private TodoData _todoData;
    private Vector2 _scrollPosition;
    private string _newTodoText = "";
    private Priority _newTodoPriority = Priority.Medium;
    private bool _showCompleted = true;

    [MenuItem("Tools/Emarex/To-Do List")]
    public static void ShowWindow()
    {
        GetWindow<TodoWindow>("To-Do List").minSize = new Vector2(450, 300);
    }

    private void OnEnable()
    {
        LoadDataAsset();
    }

    private void OnGUI()
    {
        if (_todoData == null)
        {
            EditorGUILayout.HelpBox(
                "Please create a 'To-Do Data Asset' first: Right Click > Create > Emarex > TodoData",
                MessageType.Warning);
            return;
        }

        bool guiChanged = false;

        DrawNewTaskArea(ref guiChanged);

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Task List", EditorStyles.boldLabel);

        _showCompleted = EditorGUILayout.ToggleLeft("Show Completed", _showCompleted, GUILayout.Width(180));
        EditorGUILayout.EndHorizontal();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawTaskList(ref guiChanged);
        EditorGUILayout.EndScrollView();

        if (guiChanged)
        {
            EditorUtility.SetDirty(_todoData);
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawNewTaskArea(ref bool changed)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Add New Task", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        _newTodoPriority = (Priority)EditorGUILayout.EnumPopup(_newTodoPriority, GUILayout.Width(100));
        _newTodoText = EditorGUILayout.TextArea(_newTodoText, GUILayout.MinHeight(15));

        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newTodoText));
        if (GUILayout.Button("Add Task", GUILayout.Height(15)))
        {
            AddNewTask(_newTodoText.Trim(), _newTodoPriority);
            _newTodoText = "";
            changed = true;
            GUI.FocusControl(null);
            Repaint();
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
    }

    private void DrawTaskList(ref bool changed)
    {
        List<TodoItem> itemsToDisplay = _showCompleted
            ? _todoData.items
            : _todoData.items.Where(item => !item.isDone).ToList();

        TodoItem itemToDelete = null;

        for (int i = itemsToDisplay.Count - 1; i >= 0; i--)
        {
            TodoItem item = itemsToDisplay[i];

            GUI.backgroundColor = item.isDone ? new Color(0.4f, 1f, 0.4f, 0.9f) : GetPriorityColor(item.priority);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            bool newIsDone = EditorGUILayout.Toggle(item.isDone, GUILayout.Width(20));
            if (newIsDone != item.isDone)
            {
                item.isDone = newIsDone;
                changed = true;
            }

            Priority newPriority = (Priority)EditorGUILayout.EnumPopup(item.priority, GUILayout.Width(80));
            if (newPriority != item.priority)
            {
                item.priority = newPriority;
                changed = true;
            }

            GUIStyle labelStyle = GetTaskTextStyle(item.isDone);
            labelStyle.alignment = TextAnchor.LowerLeft;

            string displayText = item.isDone ? $"<s>{item.description}</s>" : item.description;
            EditorGUILayout.LabelField(displayText, labelStyle);
            if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(20)))
            {
                itemToDelete = item;
                changed = true;
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            if (itemToDelete != null)
            {
                break;
            }
        }

        if (itemToDelete != null)
        {
            _todoData.items.Remove(itemToDelete);
        }
    }

    private void AddNewTask(string description, Priority priority)
    {
        Undo.RecordObject(_todoData, "Add New To-Do");

        _todoData.items.Insert(0, new TodoItem { description = description, priority = priority, isDone = false });
    }

    private void LoadDataAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:TodoData");

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _todoData = AssetDatabase.LoadAssetAtPath<TodoData>(path);
        }
    }

    private Color GetPriorityColor(Priority p)
    {
        return p switch
        {
            Priority.High => new Color(0.3f, 0.3f, 0.3f, 0.9f),     
            Priority.Medium => new Color(0.5f, 0.5f, 0.5f, 0.9f),    
            Priority.Low => new Color(0.7f, 0.7f, 0.7f, 0.9f),     
            _ => Color.white
        };
    }

    private GUIStyle GetTaskTextStyle(bool isDone)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        if (isDone)
        {
            style.normal.textColor = Color.gray;
            style.fontStyle = FontStyle.Italic;
        }

        style.wordWrap = true;
        style.richText = true;
        return style;
    }
}
#endif