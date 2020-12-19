using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace GursaanjTools
{
    public class GroupObjects_Editor : GuiControlEditorWindow
    {
        #region Variables
    
        private static GroupObjects_Editor _window = null;
        private static readonly Vector2 MinSize = new Vector2(300,140);
        private static readonly Vector2 MaxSize = new Vector2(300,140);
        
        private const string GroupSelectedObjects = "Group Selected Objects";
        private const string SelectionCountString = "Selection Count: ";
        private const string CastedCountFormat = "000";
        private const string GroupObjectsLabel = "Enter Group Name";
        private const string CreateGroupLabel = "Group Objects";
        private const string UndoGroupingLabel = "Grouping";
        
        private const string ErrorTitle = "Error";
        private const string NothingSelectedWarning = "No objects to Group!";
        private const string NogroupNameWarning = "No Group name entered! Would you like to continue?";
        private const string DifferentParentsWarning = "Two or more objects have different parents! Unable to Group!";
        private const string ConfirmationMessage = "Sounds good";
        private const string CancellationMessage = "Actually, no!";

        private const string GroupNameControl = "groupNameControl";

        private GameObject[] _selectedGameObjects;
        private string _groupName = "Group";

        private bool _shouldFocusOnTextField = true;
        private int _currentControlIndex = 0;
        private List<string> _listOfControls = new List<string>();
        #endregion
    
        #region Unity Methods
    
        public static void InitWindow()
        {
            _window = GetWindow<GroupObjects_Editor>();
            _window.titleContent = new GUIContent(GroupSelectedObjects);
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

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField(GroupObjectsLabel, EditorStyles.boldLabel);
                    CreateControlledTextField(GroupNameControl, ref _groupName);
                    
                    EditorGUILayout.Space();

                    if (GUILayout.Button(CreateGroupLabel, GUILayout.ExpandWidth(true), GUILayout.Height(40f)) || IsReturnPressed())
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
        
        //TODO : Ensure new groups are parented appropriately 
        private void GroupObjects()
        {
            if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog(ErrorTitle, NothingSelectedWarning, ConfirmationMessage);
                return;
            }

            if (string.IsNullOrEmpty(_groupName))
            {
                if (!EditorUtility.DisplayDialog(ErrorTitle, NogroupNameWarning, ConfirmationMessage,
                    CancellationMessage))
                {
                    return;
                }
            }

            if (!CheckForSameParents())
            {
                EditorUtility.DisplayDialog(ErrorTitle, DifferentParentsWarning, ConfirmationMessage);
                return;
            }

            GameObject groupingObject = new GameObject(_groupName);
            groupingObject.transform.parent = _selectedGameObjects[0].transform.parent;
            Undo.RegisterCreatedObjectUndo(groupingObject, UndoGroupingLabel);

            for (int i = 0, count = _selectedGameObjects.Length; i < count; i++)
            {
                GameObject obj = _selectedGameObjects[i];
                Undo.SetTransformParent(obj.transform, groupingObject.transform, UndoGroupingLabel);
            }
        }

        private bool CheckForSameParents()
        {
            Transform parentToCheck = _selectedGameObjects[0].transform.parent;

            foreach (GameObject obj in _selectedGameObjects)
            {
                if (obj.transform.parent != parentToCheck)
                {
                    return false;
                }
            }

            return true;
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

