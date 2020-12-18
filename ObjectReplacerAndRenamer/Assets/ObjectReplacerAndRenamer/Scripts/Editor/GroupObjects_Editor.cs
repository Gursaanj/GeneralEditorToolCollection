using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class GroupObjects_Editor : EditorWindow
    {
        #region Variables
    
        private static GroupObjects_Editor _window = null;
        private static readonly Vector2 _minSize = new Vector2(300,140);
        private static readonly Vector2 _maxSize = new Vector2(300,140);
        
        private const string _groupSelectedObjects = "Group Selected Objects";
        private const string _selectionCountString = "Selection Count: ";
        private const string _castedCountFormat = "000";
        private const string _groupObjectsLabel = "Enter Group Name";
        private const string _createGroupLabel = "Group Objects";
        
        private const string _errorTitle = "Error";
        private const string _nothingSelectedWarning = "No objects to Group!";
        private const string _nogroupNameWarning = "No Group name entered! Would you like to continue?";
        private const string _confirmationMessage = "Sounds good";
        private const string _cancellationMessage = "Actually, no!";

        private const string _groupNameControl = "groupNameControl";

        private GameObject[] _selectedGameObjects;
        private string _groupName = string.Empty;

        private bool _shouldFocusOnTextField = true;
        private int _currentControlIndex = 0;
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
            _selectedGameObjects = Selection.gameObjects;
            EditorGUILayout.LabelField($"{_selectionCountString}{_selectedGameObjects.Length.ToString(_castedCountFormat)}");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField(_groupObjectsLabel, EditorStyles.boldLabel);
                    CreateControlledTextField(_groupNameControl, ref _groupName);
                    
                    EditorGUILayout.Space();

                    if (GUILayout.Button(_createGroupLabel, GUILayout.ExpandWidth(true), GUILayout.Height(40f)) || IsReturnPressed())
                    {
                        GroupObjects();
                    }

                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
            }
            
            ChangeCurrentControl();
            FocusOnTextField();

            if (_window != null)
            {
                _window.Repaint();
            }
        }
    
        #endregion
    
        #region Custom Methods

        private void GroupObjects()
        {
            if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog(_errorTitle, _nothingSelectedWarning, _confirmationMessage);
                return;
            }

            if (string.IsNullOrEmpty(_groupName))
            {
                if (!EditorUtility.DisplayDialog(_errorTitle, _nogroupNameWarning, _confirmationMessage,
                    _cancellationMessage))
                {
                    return;
                }
            }
        }

        private bool IsReturnPressed()
        {
            Event currentEvent = Event.current;
            return currentEvent.isKey && currentEvent.keyCode == KeyCode.Return;
        }
        
        private void CreateControlledTextField(string controlName, ref string textField)
        {
            GUI.SetNextControlName(controlName);
            textField = EditorGUILayout.TextField(textField);
    
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

