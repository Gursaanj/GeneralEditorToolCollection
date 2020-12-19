﻿using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class GroupObjects_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        private static readonly Vector2 MinSize = new Vector2(300,140);
        private static readonly Vector2 MaxSize = new Vector2(300,140);
        
        //GUI Labels
        private const string GroupSelectedObjects = "Group Selected Objects";
        private const string GroupObjectsLabel = "Enter Group Name";
        private const string CreateGroupLabel = "Group Objects";
        
        //Warning Messages
        private const string NothingSelectedWarning = "No objects to Group!";
        private const string NoGroupNameWarning = "No Group name entered! Would you like to continue?";
        private const string DifferentParentsWarning = "Two or more objects have different parents! Unable to Group!";
        
        //Undo Labels
        private const string UndoGroupingLabel = "Grouping";
        
        private string _groupName = "Group";
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

        protected override void CreateGUI(string controlName)
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
                    
                    GUI.SetNextControlName(controlName);
                    _groupName = EditorGUILayout.TextField(_groupName);
                    
                    EditorGUILayout.Space();
            
                    if (GUILayout.Button(CreateGroupLabel, GUILayout.ExpandWidth(true), GUILayout.Height(40f)) || IsReturnPressed())
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

