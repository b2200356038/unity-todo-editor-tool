using System;
using System.Collections.Generic;
using UnityEngine;

public enum Priority
{
    Low,
    Medium,
    High
}

[Serializable]
public class TodoItem
{
    [TextArea(1, 5)] public string description;
    public bool isDone;
    public Priority priority;
}


[CreateAssetMenu(fileName = "newTodoData", menuName = "Emarex/TodoData")]
public class TodoData : ScriptableObject
{
    public List<TodoItem> items = new List<TodoItem>();
}