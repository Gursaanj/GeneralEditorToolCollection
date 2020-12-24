using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private const string ClearLabel = "Clear";

        private const float VerticalPadding = 5f;
        private const int MaxAmountOfObjects = 100;
        private const string SceneViewIdentifier = "**";

        //Warning Labels
        private const string ClearListMessage = "Are you sure you would like to clear the current list";
        private const string NoObjectToInsertMessage = "No object to insert into the list, Please try again!";

        [SerializeField] private ObjectInformation _selectedObject = null;
        [SerializeField] private List<ObjectInformation> _listOfSelectables = new List<ObjectInformation>();

        private Vector2 _scrollPosition = Vector2.zero;

        private GUIStyle _lockedButtonGUIStyle;
        private GUIStyle _selectObjectGUIStyle;
        private GUIContent _searchButtonGUIContent;

        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _lockedButtonGUIStyle = new GUIStyle("IN LockButton");
            _lockedButtonGUIStyle.margin.top = 3;
            _lockedButtonGUIStyle.margin.right = 10;
            _lockedButtonGUIStyle.margin.left = 10;

            _selectObjectGUIStyle = EditorStyles.miniButtonLeft;
            _selectObjectGUIStyle.alignment = TextAnchor.MiddleCenter;
            
            _searchButtonGUIContent = EditorGUIUtility.IconContent("d_ViewToolZoom", "Ping Object");
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
                EditorGUILayout.Space(VerticalPadding);

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

                    LayoutItem(objectOfInterest);
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
                DisplayDialogue(ErrorTitle, NoObjectToInsertMessage, false);
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
            EditorGUILayout.Space(VerticalPadding);

            bool wasListCleared = GUILayout.Button(ClearLabel, EditorStyles.miniButton);
            if (wasListCleared)
            {
                if (DisplayDialogue(AreYouSureTitle, ClearListMessage, true))
                {
                    for (int i = _listOfSelectables.Count - 1; i >= 0; i--)
                    {
                        if (!_listOfSelectables[i].IsObjectLocked)
                        {
                            _listOfSelectables.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    wasListCleared = false;
                }

                // for (int i = _listOfSelectables.Count - 1; i >= 0; i--)
                // {
                //     if (!_listOfSelectables[i].IsObjectLocked)
                //     {
                //         _listOfSelectables.RemoveAt(i);
                //     }
                // }
            }
            
            EditorGUILayout.Space(VerticalPadding);
            return wasListCleared;
        }

        private void LayoutItem(ObjectInformation information)
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

                    string objName = information.Object.name;

                    if (information.IsObjectInScene)
                    {
                        // Set name on Object to identify if in scene
                        objName = $"{objName}{SceneViewIdentifier}";
                    }

                    if (GUILayout.Button(objName, _selectObjectGUIStyle))
                    {
                        _selectedObject = information;

                        //if object is a scene, load the scene
                        if (information.Object is SceneAsset)
                        {
                            Scene[] currentScene = new Scene[1];
                            currentScene[0] = EditorSceneManager.GetActiveScene();
                            EditorSceneManager.SaveModifiedScenesIfUserWantsTo(currentScene);
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(information.Object));
                        }
                        else
                        {
                            Selection.activeObject = information.Object;
                        }
                    }

                    GUI.enabled = true;

                    if (GUILayout.Button(_searchButtonGUIContent, EditorStyles.miniButtonRight, GUILayout.MaxWidth(25),
                        GUILayout.MaxHeight(15)))
                    {
                        EditorGUIUtility.PingObject(information.Object);
                    }
                }
            }
        }

        #endregion
    }
}
