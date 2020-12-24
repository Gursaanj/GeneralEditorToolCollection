using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GursaanjTools
{
    public class SelectionLog_Editor : GuiControlEditorWindow
    {

        [System.Serializable]
        private class ObjectInformation
        {
            public Object Object = null;
            public bool IsObjectLocked = false;
            public bool IsObjectInScene = false;
        }

        #region Variables

        //GUI Labels

        //Warning Labels

        //UndoLabels

        private const int MaxAmountOfObjects = 100;
        private const string SceneViewIdentifier = "^^";

        [SerializeField] private ObjectInformation _selectedObject = null;
        [SerializeField] private List<ObjectInformation> _listOfSelectables = new List<ObjectInformation>();

        private Vector2 _scrollPosition = Vector2.zero;

        private GUIStyle _lockedButtonGUIStyle;
        private GUIContent _searchButtonGUIContent;

        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _lockedButtonGUIStyle = new GUIStyle("IN LockButton");
            _lockedButtonGUIStyle.margin.top = 3;
            _lockedButtonGUIStyle.margin.right = 10;
            _lockedButtonGUIStyle.margin.left = 10;

        }

        // Called when a selection changes in the list
        private void OnSelectionChange()
        {
            Object currentlySelected = Selection.activeObject;

            if (currentlySelected == null)
            {
                _selectedObject = null;
                return;
            }

            // If object is already in list then move it to the top! Unless if it has been locked in position
            // If object is not in the list, then add it so and move it to the top
            if ((_selectedObject == null || _selectedObject.Object != currentlySelected) && _listOfSelectables != null)
            {
                _selectedObject = _listOfSelectables.Find(obj => obj.Object == currentlySelected);

                if (_selectedObject == null)
                {
                    _selectedObject = new ObjectInformation()
                    {
                        Object = currentlySelected,
                        // will return true if object is not registered in assetDatabase
                        IsObjectInScene = AssetDatabase.Contains(Selection.activeInstanceID) == false
                    };

                    InsertAtTopOfList(_selectedObject);
                }
                else if (!_selectedObject.IsObjectLocked)
                {
                    _listOfSelectables.Remove(_selectedObject);
                    InsertAtTopOfList(_selectedObject);
                }

                //Ensure Object Limit is respected

                while (_listOfSelectables.Count > MaxAmountOfObjects)
                {
                    _listOfSelectables.RemoveAt(0);
                }
            }
        }

        protected override void CreateGUI(string controlName)
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.Space(5f);

                if (_listOfSelectables == null)
                {
                    return;
                }

                bool processingLocked = false;
                bool shownClear = false;

                for (int i = _listOfSelectables.Count - 1; i >= 0; i--)
                {
                    ObjectInformation objectOfInterest = _listOfSelectables[i];

                    if (!objectOfInterest.IsObjectLocked)
                    {
                        if (processingLocked)
                        {
                            shownClear = true;
                            if (ClearButtonLayout())
                            {
                                break;
                            }

                            processingLocked = false;
                        }
                    }
                    else
                    {
                        processingLocked = true;
                    }

                    LayoutItem(i, objectOfInterest);
                }
                
                //If Clear button hasnt shown, thus no lockled buttons existing, show it at the end
                if (!shownClear)
                {
                    ClearButtonLayout();
                }
            }
        }

        #endregion

        #region Custom Methods

        private void InsertAtTopOfList(ObjectInformation objectToInsert)
        {
            if (objectToInsert == null || _listOfSelectables == null)
            {
                Debug.Log("Uhoh");
                return;
            }

            int firstNonLockedObject = _listOfSelectables.FindIndex(obj => obj.IsObjectLocked);

            if (firstNonLockedObject >= 0)
            {
                _listOfSelectables.Insert(firstNonLockedObject, objectToInsert);
            }
            else
            {
                _listOfSelectables.Add(objectToInsert);
            }
        }

        private bool ClearButtonLayout()
        {
            EditorGUILayout.Space(5f);

            bool wasListCleared = GUILayout.Button("Clear", EditorStyles.miniButton);
            if (wasListCleared)
            {
                for (int i = _listOfSelectables.Count - 1; i >= 0; i--)
                {
                    if (!_listOfSelectables[i].IsObjectLocked)
                    {
                        _listOfSelectables.RemoveAt(i);
                    }
                }
            }
            
            EditorGUILayout.Space(5f);
            return wasListCleared;
        }

        private void LayoutItem(int index, ObjectInformation information)
        {
            if (information != null && information.Object != null)
            {
                using (new GUILayout.HorizontalScope())
                {
                    bool wasLocked = information.IsObjectLocked;
                    information.IsObjectLocked = GUILayout.Toggle(information.IsObjectLocked, GUIContent.none,
                        _lockedButtonGUIStyle);

                    if (wasLocked != information.IsObjectLocked)
                    {
                        _listOfSelectables.Remove(information);
                        InsertAtTopOfList(information);
                    }

                    if (information == _selectedObject)
                    {
                        GUI.enabled = false;
                    }
                }
            }
        }

        #endregion
    }
}
