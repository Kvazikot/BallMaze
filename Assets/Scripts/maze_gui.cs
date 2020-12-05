using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenParallel))]
public class maze_gui : Editor
{
    int _selected = 0;
    string[] _options = new string[3] { "Item1", "Item2", "Item3" };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Опции");
        MazeGenParallel maze_gen = (MazeGenParallel)target;
      

        if (GUILayout.Button("ReGenerate Maze Parallel"))
        {
            int n = maze_gen.transform.childCount;
            for (int i = 0; i < n; i++)
                SafeDestroy.SafeDestroyGameObject(maze_gen.transform.GetChild(0));
            maze_gen.RegenerateMaze();
        }

        if (GUILayout.Button("ReGenerate Maze"))
        {
            MazeGen maze_gen1 = GameObject.Find("Plane").GetComponent<MazeGen>();
            int n = maze_gen1.transform.childCount;
            for (int i = 0; i < n; i++)
                SafeDestroy.SafeDestroyGameObject(maze_gen1.transform.GetChild(0));
            maze_gen1.RegenerateMaze();
        }

        EditorGUI.BeginChangeCheck();
        this._selected = EditorGUILayout.Popup("My Simple Dropdown", _selected, _options);
                if (EditorGUI.EndChangeCheck())
        {
            Debug.Log(_options[_selected]);
        }
    }
}

