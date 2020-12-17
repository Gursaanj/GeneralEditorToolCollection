using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GroupObjects_Editor : EditorWindow
{
    #region Variables

    private static GroupObjects_Editor _window = null;
    private static readonly Vector2 _minSize = new Vector2(300,140);
    private static readonly Vector2 _maxSize = new Vector2(300,180);
    
    private const string _groupSelectedObjects = "Group Selected Objects";
    
    private List<string> _listOfControls = new List<string>();
    #endregion

    #region Unity Methods

    public static void InitWindow()
    {
        _window = GetWindow<GroupObjects_Editor>();
        _window.titleContent = new GUIContent(_groupSelectedObjects);
        _window.minSize = _minSize;
        _window.maxSize = _maxSize;
        _window.autoRepaintOnSceneChange = true;
        _window.Focus();
        _window.Show();
    }

    private void OnGUI()
    {
        
    }

    #endregion

    #region Custom Methods

    private bool IsReturnPressed()
    {
        Event currentEvent = Event.current;
        return currentEvent.isKey && currentEvent.keyCode == KeyCode.Return;
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

    #endregion
}
