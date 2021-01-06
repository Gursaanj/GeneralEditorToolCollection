using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class RenameObjects_Editor : GuiControlEditorWindow
{
    #region Variables
    
    //GUI Labels
    private const string PrefixLabel = "Prefix: ";
    private const string NameLabel = "Name: ";
    private const string SuffixLabel = "Suffix: ";
    private const string AddNumberingLabel = "Add Numbering? ";
    private const string FinalNameLabel = "Rename Objects to:  ";
    private const string FinalNameFormat = "{0}_{1}";
    
    private const float HorizontalPadding = 10.0f;
    private const float VerticalPadding = 2.5f;
    
    //Warning Labels
    private const string NothingSelectedWarning = "No objects to rename!";
    private const string NoNameToRenameWithWarning = "Are you sure you want to remove the names from the selected objects?";
    
    //Undo Labels
    private const string UndoRenameLabel = "Rename";
    
    private GUIStyle _editorLabelWithEllipsis;
    
    private string _wantedPrefix = string.Empty;
    private string _wantedName = string.Empty;
    private string _wantedSuffix = string.Empty;
    private bool _shouldAddNumbering = false;
    #endregion
    
    #region BuiltIn Methods
    private void OnEnable()
    {
        _editorLabelWithEllipsis = new GUIStyle
        {
            clipping = TextClipping.Clip,
            wordWrap = false
        };
    }

    #endregion
    
    #region Abstract Methods
    protected override void CreateGUI(string controlName)
    {
        _selectedGameObjects = Selection.gameObjects;
        EditorGUILayout.LabelField($"{SelectionCountString}{_selectedGameObjects.Length.ToString(CastedCountFormat)}", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(HorizontalPadding);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(VerticalPadding);

                GUI.SetNextControlName(controlName);
                _wantedPrefix = EditorGUILayout.TextField(PrefixLabel, _wantedPrefix, EditorStyles.miniTextField,
                    GUILayout.ExpandWidth(true));
                _wantedName = EditorGUILayout.TextField(NameLabel, _wantedName, EditorStyles.miniTextField,
                    GUILayout.ExpandWidth(true));
                _wantedSuffix = EditorGUILayout.TextField(SuffixLabel, _wantedSuffix, EditorStyles.miniTextField,
                    GUILayout.ExpandWidth(true));
                
                _shouldAddNumbering = EditorGUILayout.Toggle(AddNumberingLabel, _shouldAddNumbering);
                
                GUILayout.Space(VerticalPadding);
            }
            GUILayout.Space(HorizontalPadding);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(HorizontalPadding);
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(string.Format($"{FinalNameLabel} {GetFinalName()}"), _editorLabelWithEllipsis);

                GUILayout.Space(VerticalPadding);
                
                if (GUILayout.Button(TitleContent.text,GUILayout.ExpandWidth(true)) || IsReturnPressed())
                {
                    RenameGameObjects();
                }
            }
            GUILayout.Space(HorizontalPadding);
        }
    }

    #endregion

    #region Custom Methods

    private void RenameGameObjects()
    {
        if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
        {
            DisplayDialogue(ErrorTitle, NothingSelectedWarning, false);
            return;
        }

        if (string.IsNullOrEmpty(_wantedPrefix) && string.IsNullOrEmpty(_wantedName) &&
            string.IsNullOrEmpty(_wantedSuffix))
        {
            if (!DisplayDialogue(ErrorTitle, NoNameToRenameWithWarning, true))
            {
                return;
            }
        }
        
        _selectedGameObjects.Reverse();
        
        Array.Sort(_selectedGameObjects,
            (aGameObject, bGameObject) => String.Compare(aGameObject.name, bGameObject.name, StringComparison.Ordinal));
        
        for (int i = 0; i < _selectedGameObjects.Length; i++)
        {
            string finalName = GetFinalName();
            
            //TODO: Look into EnsureUniqueNameForSibiling
            if (_shouldAddNumbering && i > 0)
            {
                finalName = string.Format(FinalNameFormat, finalName, i.ToString());
            }

            GameObject selectedGameObject = _selectedGameObjects[i];
            Undo.RecordObject(selectedGameObject, UndoRenameLabel);
            selectedGameObject.name = finalName;
        }
    }

    private String GetFinalName()
    {
        string finalName = String.Empty;
        
        finalName = AddToFinalName(finalName, _wantedPrefix);
        finalName = AddToFinalName(finalName, _wantedName);
        finalName = AddToFinalName(finalName, _wantedSuffix);

        return finalName;
    }

    private string AddToFinalName(string finalName , string nameSegment)
    {
        if (!string.IsNullOrEmpty(nameSegment))
        {
            finalName = string.IsNullOrEmpty(finalName)
                ? nameSegment
                : string.Format(FinalNameFormat, finalName, nameSegment);
        }

        return finalName;
    }
    
    #endregion
}
}

