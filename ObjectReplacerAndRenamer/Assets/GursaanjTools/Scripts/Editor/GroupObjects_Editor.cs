using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class GroupObjects_Editor : GuiControlEditorWindow
    {
        #region Variables
        //GUI Labels
        private const string GroupObjectsLabel = "Enter Group Name";
        private const string CreateGroupLabel = "Group Objects";
        
        //Warning Messages
        private const string NothingSelectedWarning = "No objects to Group!";
        private const string NoGroupNameWarning = "No Group name entered! Would you like to continue?";
        private const string DifferentParentsWarning = "Two or more objects have different parents! Unable to Group!";
        
        //Undo Labels
        private const string UndoGroupingLabel = "Grouping";

        private const float ButtonHeightPadding = 40f;
        
        private string _groupName = "Group";
        #endregion
        
        #region Abstract Methods
        protected override void CreateGUI(string controlName)
        {
            _selectedGameObjects = Selection.gameObjects;
            EditorGUILayout.LabelField($"{SelectionCountString}{_selectedGameObjects.Length.ToString(CastedCountFormat)}", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField(GroupObjectsLabel, EditorStyles.boldLabel);
                    
                    GUI.SetNextControlName(controlName);
                    _groupName = EditorGUILayout.TextField(_groupName);
                    
                    EditorGUILayout.Space();
            
                    if (GUILayout.Button(CreateGroupLabel, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeightPadding)) || IsReturnPressed())
                    {
                        GroupObjects();
                    }
            
                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
            }
        }

        #endregion
    
        #region Custom Methods
        
        private void GroupObjects()
        {
            if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
            {
                DisplayDialogue(ErrorTitle, NothingSelectedWarning, false);
                return;
            }

            if (string.IsNullOrEmpty(_groupName))
            {
                if (!DisplayDialogue(ErrorTitle, NoGroupNameWarning, true))
                {
                    return;
                }
            }

            if (!CheckForSameParents())
            {
                DisplayDialogue(ErrorTitle, DifferentParentsWarning, false);
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
        
        #endregion
    }
}

