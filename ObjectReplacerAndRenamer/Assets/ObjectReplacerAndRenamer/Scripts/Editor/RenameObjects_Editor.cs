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
    private static readonly Vector2 MinSize = new Vector2(300,140);
    private static readonly Vector2 MaxSize = new Vector2(300,180);

    private const string RenameSelectedObjects = "Rename Selected Objects";
    private const string CastedCountFormat = "000";
    private const string SelectionCountString = "Selection Count: ";
    private const string PrefixLabel = "Prefix: ";
    private const string NameLabel = "Name: ";
    private const string SuffixLabel = "Suffix: ";
    private const string AddNumberingLabel = "Add Numbering? ";
    private const string UndoRenameLabel = "Rename";

    private const string ErrorTitle = "Error";
    private const string NothingSelectedWarning = "No objects to rename!";
    private const string NoNameToRenameWithWarning = "Are you sure you want to remove the names from the selected objects?";
    private const string ConfirmationMessage = "Sounds good";
    private const string CancellationMessage = "Actually, no!";
    
    // Handling GUI Control Manually
    private const string PrefixControlName = "preifxControl";
    private const string NameControlName = "nameControl";
    private const string SuffixControlName = "suffixControl";
    
    private const string FinalNameFormat = "{0}_{1}";
    
    private const float HorizontalPadding = 10.0f;
    private const float VerticalPadding = 2.5f;

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
        _window.titleContent = new GUIContent(RenameSelectedObjects);
        _window.minSize = MinSize;
        _window.maxSize = MaxSize;
        _window.autoRepaintOnSceneChange = true;
        _window.Focus();
        _window.Show();
    }
    
    private void OnGUI()
    {
        _selectedGameObjects = Selection.gameObjects;
        EditorGUILayout.LabelField($"{SelectionCountString}{_selectedGameObjects.Length.ToString(CastedCountFormat)}");
        
        //Add UI
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(HorizontalPadding);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(VerticalPadding);

                CreateControlledTextField(PrefixControlName, ref _wantedPrefix, PrefixLabel);
                CreateControlledTextField(NameControlName, ref _wantedName, NameLabel);
                CreateControlledTextField(SuffixControlName, ref _wantedSuffix, SuffixLabel);
                _shouldAddNumbering = EditorGUILayout.Toggle(AddNumberingLabel, _shouldAddNumbering);
                
                GUILayout.Space(VerticalPadding);
            }
            GUILayout.Space(HorizontalPadding);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(RenameSelectedObjects,GUILayout.ExpandWidth(true)) || IsReturnPressed())
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
            EditorUtility.DisplayDialog(ErrorTitle, NothingSelectedWarning, ConfirmationMessage);
            return;
        }

        if (string.IsNullOrEmpty(_wantedPrefix) && string.IsNullOrEmpty(_wantedName) &&
            string.IsNullOrEmpty(_wantedSuffix))
        {
            if (!EditorUtility.DisplayDialog(ErrorTitle, NoNameToRenameWithWarning, ConfirmationMessage,
                CancellationMessage))
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
                finalName = string.Format(FinalNameFormat, finalName, i.ToString());
            }

            GameObject selectedGameObject = _selectedGameObjects[i];
            Undo.RecordObject(selectedGameObject, UndoRenameLabel);
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
                : string.Format(FinalNameFormat, finalName, nameSegement);
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

