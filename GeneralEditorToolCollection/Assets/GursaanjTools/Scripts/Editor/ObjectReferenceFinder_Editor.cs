using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace GursaanjTools
{
    public class ObjectReferenceFinder_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI Labels
        private const string ReferenceObjectLabel = "Object to find:";
        private const string NumberOfFoundReferencesLabel = "Number of Found References:";
        private const string FindReferencesLabel = "Find References";
        private const string ClearListLabel = "Clear List";
        private const string RightArrowLabel = "\u25B6";
        private const string RightArrowTooltip = "Get Further Dependencies";
        private const string NoGameObjectsFoundLabel = "No GameObjects found";

        private const string ProgressBarTitle = "Searching";
        private const string ProgressBarInitialMessage = "Getting all file paths";
        private const string ProgressBarDependencyMessage = "Searching Dependancies";
        private const string ProgressBarRemovalMessage = "Removing redundant messages";

        private const float VerticalPadding = 5f;
        private const float RightButtonMaxWidth = 20f;
        
        //Warning Labels
        private const string GeneralErrorMessage = "Something Went Wrong with the search, try again!";
        private const string NoReferenceObjectWarning = "No Object To Find References for!";
        
        //Data
        private const string AssetDirectory = "Assets";
        private const string PrefabExtension = ".prefab";
        private const int IterationConstant = 5;
        private const float ProgressIncrement = 0.01f;

        private static Object _objectToFind;
        private static bool _enableAutoFind = false;

        private Vector2 _scrollPosition = Vector2.zero;
        private List<GameObject> _referenceObjects = new List<GameObject>();
        private List<string> _paths = null;
        private Object _referenceObjAfterLayout = null;
        private GUIContent _rightArrowContent;
        private GUIStyle _overflowGUIStyle;
        
        #endregion
        
        #region BuiltIn Methods

        public static void AssetInit(EditorWindowInformation windowInformation)
        {
            _window = GetWindow<ObjectReferenceFinder_Editor>();
            SetWindowInformation(windowInformation);
            _window.titleContent = windowInformation.Title;
            _window.minSize = windowInformation.MinSize;
            _window.maxSize = windowInformation.MaxSize;
            _window.Focus();
            _window.Show();

            _objectToFind = Selection.activeObject;
            _enableAutoFind = true;
        }

        private void OnEnable()
        {
            _rightArrowContent = new GUIContent(RightArrowLabel, RightArrowTooltip);
            _overflowGUIStyle = new GUIStyle{clipping = TextClipping.Overflow, alignment = TextAnchor.MiddleCenter};
        }

        protected override void CreateGUI(string controlName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(ReferenceObjectLabel);
                    _objectToFind = EditorGUILayout.ObjectField(_objectToFind, typeof(Object), false);

                    if (GUILayout.Button(FindReferencesLabel, EditorStyles.miniButtonRight) || _enableAutoFind)
                    {
                        _enableAutoFind = false;
                        FindObjectReferences(_objectToFind);
                    }
                }

                EditorGUILayout.Space(VerticalPadding);

                if (_objectToFind == null)
                {
                    return;
                }

                using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                {
                    _scrollPosition = scrollView.scrollPosition;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"{NumberOfFoundReferencesLabel} {_referenceObjects.Count}", EditorStyles.boldLabel);

                        if (GUILayout.Button(ClearListLabel, EditorStyles.miniButton))
                        {
                            Clear();
                        }
                    }
                    
                    EditorGUILayout.Space(VerticalPadding);

                    if (_referenceObjects.Count == 0)
                    {
                        GUILayout.Label(NoGameObjectsFoundLabel, _overflowGUIStyle);
                    }
                    else
                    {
                        for (int i = 0, count = _referenceObjects.Count; i < count; i++)
                        {
                            LayoutItem(_referenceObjects[i]);
                        }
                    }
                }

                if (_referenceObjAfterLayout != null)
                {
                    FindObjectReferences(_referenceObjAfterLayout);
                    _referenceObjAfterLayout = null;
                }
            }
        }

        #endregion

        #region Custom Methods

        private void FindObjectReferences(Object objectToFind)
        {
            if (objectToFind == null)
            {
                DisplayDialogue(ErrorTitle, NoReferenceObjectWarning, false);
                return;
            }

            EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarInitialMessage, 0.0f);
            
            //Get All Prefabs
            if (_paths == null)
            {
                _paths = new List<string>();
                GetFilePathsFromExtension(AssetDirectory, PrefabExtension, ref _paths);
            }

            float progresspercentage = 0;
            int prefabCount = _paths.Count;
            int iteration = Mathf.Max(1, prefabCount / (IterationConstant == 0 ? 1 : IterationConstant));

            string nameOfObject = AssetDatabase.GetAssetPath(objectToFind);

            if (string.IsNullOrEmpty(nameOfObject))
            {
                DisplayDialogue(ErrorTitle, GeneralErrorMessage, false);
            }

            nameOfObject = Path.GetFileNameWithoutExtension(nameOfObject);
            Object[] tempObjects = new Object[1]; // Need to make array to use CollectDependencies Method
            _referenceObjects.Clear();
            
            //Loop over files
            for (int i = 0; i < prefabCount; i++)
            {
                tempObjects[0] = AssetDatabase.LoadMainAssetAtPath(_paths[i]);
                Object tempObject = tempObjects[0];
                if (tempObject != null && tempObject != objectToFind) //Don't add self
                {
                    Object[] dependencies = EditorUtility.CollectDependencies(tempObjects);
                    
                    //Dont add Object if another of the dependencies is already there
                    if (Array.Exists(dependencies, dependant => (Object)dependant == objectToFind))
                    {
                        _referenceObjects.Add(tempObject as GameObject);
                    }
                }

                if (i % iteration == 0)
                {
                    progresspercentage += ProgressIncrement;
                    EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarDependencyMessage, progresspercentage);
                }
            }
            
            EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarRemovalMessage, 1.0f);
            
            //Retrieve Direct Dependencies Only
            for (int i = _referenceObjects.Count - 1; i >= 0; i--)
            {
                tempObjects[0] = _referenceObjects[i];
                Object[] dependencies = EditorUtility.CollectDependencies(tempObjects);

                bool shouldRemoveObject = false;

                for (int j = 0; j < dependencies.Length && !shouldRemoveObject; j++)
                {
                    Object dependency = dependencies[j];
                    shouldRemoveObject =
                        _referenceObjects.Find(reference => reference == dependency && reference != tempObjects[0]) != null;
                }

                if (shouldRemoveObject)
                {
                    _referenceObjects.RemoveAt(i);
                }
            }
            
            EditorUtility.ClearProgressBar();
        }

        private void GetFilePathsFromExtension(string startingDirectory, string extension, ref List<string> listOfPaths)
        {
            try
            {
                // Go over main directory
                string[] initialFiles = Directory.GetFiles(startingDirectory);

                foreach (string file in initialFiles)
                {
                    if (!string.IsNullOrEmpty(file) && file.EndsWith(extension))
                    {
                        listOfPaths.Add(file);
                    }
                }
                
                //Then Recurse over sub-directories
                string[] subDirectories = Directory.GetDirectories(startingDirectory);

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    GetFilePathsFromExtension(subDirectories[i], extension, ref listOfPaths);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void LayoutItem(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            GUIStyle style = EditorStyles.miniButtonLeft;
            style.alignment = TextAnchor.MiddleCenter;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(obj.name, style))
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button(_rightArrowContent, EditorStyles.miniButtonRight, GUILayout.MaxWidth(RightButtonMaxWidth)))
                {
                    _referenceObjAfterLayout = obj;
                }
            }
        }

        private void Clear()
        {
            _referenceObjects.Clear();
        }

        #endregion
    }
}
