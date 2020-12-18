using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class RenameObjects_Editor : EditorWindow
{
    #region Variables

    private static RenameObjects_Editor _window = null;
    private static readonly Vector2 _minSize = new Vector2(300,140);
    private static readonly Vector2 _maxSize = new Vector2(300,180);

    private const string _renameSelectedObjects = "Rename Selected Objects";
    private const string _castedCountFormat = "000";
    private const string _selectionCountString = "Selection Count: ";
    private const string _prefixLabel = "Prefix: ";
    private const string _nameLabel = "Name: ";
    private const string _suffixLabel = "Suffix: ";
    private const string _addNumberingLabel = "Add Numbering? ";
    private const string _undoRenameLabel = "Rename";

    private const string _errorTitle = "Error";
    private const string _nothingSelectedWarning = "No objects to rename!";
    private const string _noNameToRenameWithWarning = "Are you sure you want to remove the names from the selected objects?";
    private const string _confirmationMessage = "Sounds good";
    private const string _cancellationMessage = "Actually, no!";
    
    // Handling GUI Control Manually
    private const string _prefixControlName = "preifxControl";
    private const string _nameControlName = "nameControl";
    private const string _suffixControlName = "suffixControl";
    
    private const string _finalNameFormat = "{0}_{1}";
    
    private const float _horizontalPadding = 10.0f;
    private const float _verticalPadding = 2.5f;

    private GameObject[] _selectedGameObjects = null;
    private string _wantedPrefix = string.Empty;
    private string _wantedName = string.Empty;
    private string _wantedSuffix = string.Empty;
    private bool _shouldAddNumbering = false;
    private bool _shouldFocusOnTextField = true;
    
    private int _currentControlIndex = 0;
    private List<string> _listOfControls = new List<string>();

    #endregion

    #region BuiltIn Methods

    public static void InitWindow()
    {
        _window = GetWindow<RenameObjects_Editor>();
        _window.titleContent = new GUIContent(_renameSelectedObjects);
        _window.minSize = _minSize;
        _window.maxSize = _maxSize;
        _window.autoRepaintOnSceneChange = true;
        _window.Focus();
        _window.Show();
    }
    
    private void OnGUI()
    {
        _selectedGameObjects = Selection.gameObjects;
        EditorGUILayout.LabelField($"{_selectionCountString}{_selectedGameObjects.Length.ToString(_castedCountFormat)}");
        
        //Add UI
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(_horizontalPadding);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(_verticalPadding);

                CreateControlledTextField(_prefixControlName, ref _wantedPrefix, _prefixLabel);
                CreateControlledTextField(_nameControlName, ref _wantedName, _nameLabel);
                CreateControlledTextField(_suffixControlName, ref _wantedSuffix, _suffixLabel);
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

        ChangeCurrentControl();
        FocusOnTextField();
        
        if (_window != null)
        {
            Repaint();
        }
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

        if (string.IsNullOrEmpty(_wantedPrefix) && string.IsNullOrEmpty(_wantedName) &&
            string.IsNullOrEmpty(_wantedSuffix))
        {
            if (!EditorUtility.DisplayDialog(_errorTitle, _noNameToRenameWithWarning, _confirmationMessage,
                _cancellationMessage))
            {
                return;
            }
        }
        
        _selectedGameObjects.Reverse();
        
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

    private void CreateControlledTextField(string controlName, ref string textField, string label)
    {
        GUI.SetNextControlName(controlName);
        textField = EditorGUILayout.TextField(label, textField, EditorStyles.miniTextField,
            GUILayout.ExpandWidth(true));

        if (!_listOfControls.Contains(controlName))
        {
            _listOfControls.Add(controlName);
        }
    }

    private void ChangeCurrentControl()
    {
        Event currentEvent = Event.current;

        if (currentEvent.isKey)
        {
            if (currentEvent.keyCode == KeyCode.DownArrow)
            {
                if (_currentControlIndex < (_listOfControls.Count - 1))
                {
                    _currentControlIndex++;
                    _shouldFocusOnTextField = true;
                }
            }
            else if (currentEvent.keyCode == KeyCode.UpArrow)
            {
                if (_currentControlIndex > 0)
                {
                    _currentControlIndex--;
                    _shouldFocusOnTextField = true;
                }
            }
        }
    }

    private void FocusOnTextField()
    {
        if(_shouldFocusOnTextField && _window != null && _listOfControls != null && _listOfControls.Count != 0)
        {
            _window.Focus();
            EditorGUI.FocusTextInControl(_listOfControls[_currentControlIndex]);
            _shouldFocusOnTextField = false;
        }
    }


    #endregion
}
}

