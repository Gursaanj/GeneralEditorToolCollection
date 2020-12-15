﻿using System;
using UnityEditor;
using UnityEngine;

public class RenameObjects_Editor : EditorWindow
{
    #region Variables

    private static RenameObjects_Editor _window = null;
    private static readonly Vector2 _minSize = new Vector2(300,140);
    private static readonly Vector2 _maxSize = new Vector2(300,180);

    private const string _renameSelectedObjects = "Rename Selected Objects";
    private const string _selectionCountString = "Selection Count: ";
    private const string _prefixLabel = "Prefix: ";
    private const string _nameLabel = "Name: ";
    private const string _suffixLabel = "Suffix: ";
    private const string _addNumberingLabel = "Add Numbering? ";
    private const string _undoRenameLabel = "Rename";

    private const string _errorTitle = "Error";
    private const string _nothingSelectedWarning = "No objects to rename!";
    private const string _confirmationMessage = "Sounds good";
    
    private const string _finalNameFormat = "{0}_{1}";
    
    private const float _horizontalPadding = 10.0f;
    private const float _verticalPadding = 2.5f;

    private GameObject[] _selectedGameObjects = null;
    private string _wantedPrefix = string.Empty;
    private string _wantedName = string.Empty;
    private string _wantedSuffix = string.Empty;
    private bool _shouldAddNumbering = false;

    #endregion

    #region Unity Methods

    public static void InitWindow()
    {
        _window = GetWindow<RenameObjects_Editor>();
        _window.titleContent = new GUIContent(_renameSelectedObjects);
        _window.minSize = _minSize;
        _window.maxSize = _maxSize;
        _window.autoRepaintOnSceneChange = true;
        _window.Show();
    }
    
    //Todo : Add GUI control to TextField
    private void OnGUI()
    {
        _selectedGameObjects = Selection.gameObjects;
        EditorGUILayout.LabelField(string.Format("{0}{1}", _selectionCountString, _selectedGameObjects.Length.ToString("000")));
        
        //Add UI
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(_horizontalPadding);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(_verticalPadding); 
                _wantedPrefix = EditorGUILayout.TextField(_prefixLabel, _wantedPrefix, EditorStyles.miniTextField, GUILayout.ExpandWidth(true));
                _wantedName = EditorGUILayout.TextField(_nameLabel, _wantedName);
                _wantedSuffix = EditorGUILayout.TextField(_suffixLabel, _wantedSuffix, EditorStyles.miniTextField, GUILayout.ExpandWidth(true));
                _shouldAddNumbering = EditorGUILayout.Toggle(_addNumberingLabel, _shouldAddNumbering);
                GUILayout.Space(_verticalPadding);
            }
            GUILayout.Space(_horizontalPadding);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(_renameSelectedObjects,GUILayout.ExpandWidth(true)) || IsReturnPressed())
            {
                RenameGameObjects();
            }
        }

        Repaint();
    }

    #endregion

    #region Custom Methods

    private void RenameGameObjects()
    {
        if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(_errorTitle, _nothingSelectedWarning, _confirmationMessage);
            return;
        }


        Array.Sort(_selectedGameObjects,
            delegate(GameObject aGameObject, GameObject bGameObject)
            {
                return aGameObject.name.CompareTo(bGameObject.name);
                
            });
        
        for (int i = 0; i < _selectedGameObjects.Length; i++)
        {
            string finalName = string.Empty;

            finalName = AddToFinalName(finalName, _wantedPrefix);
            finalName = AddToFinalName(finalName, _wantedName);
            finalName = AddToFinalName(finalName, _wantedSuffix);

            if (_shouldAddNumbering && i > 0)
            {
                finalName = string.Format(_finalNameFormat, finalName, i.ToString());
            }

            GameObject selectedGameObject = _selectedGameObjects[i];
            Undo.RecordObject(selectedGameObject, _undoRenameLabel);
            selectedGameObject.name = finalName;
        }
    }

    private bool IsReturnPressed()
    {
        Event currentEvent = Event.current;
        return currentEvent.isKey && currentEvent.keyCode == KeyCode.Return;
    }

    private string AddToFinalName(string finalName , string nameSegement)
    {
        if (!string.IsNullOrEmpty(nameSegement))
        {
            finalName = string.IsNullOrEmpty(finalName)
                ? nameSegement
                : string.Format(_finalNameFormat, finalName, nameSegement);
        }

        return finalName;
    }


    #endregion
}
