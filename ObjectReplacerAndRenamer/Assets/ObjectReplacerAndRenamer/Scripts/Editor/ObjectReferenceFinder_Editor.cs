using System;
using System.Collections;
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
        private const string ReferenceObjectLabel = "Reference Object:";
        private const string NumberOfFoundReferencesLabel = "Number of Found References:";
        private const string RightArrowLabel = "\u25B6";

        private const string ProgressBarTitle = "Searching";
        private const string ProgressBarInitialMessage = "Getting all file paths";
        private const string ProgressBarDependencyMessage = "Searching Dependancies";
        private const string ProgressBarRemovalMessage = "Removing redundant messages";
        
        //Warning Labels
        
        //Undo Labels

        private Vector2 _scrollPosition = Vector2.zero;
        private List<GameObject> _referenceObjects = new List<GameObject>();
        private List<string> _paths = new List<string>();
        private Object _objectToFind;
        private Object _queueOfReferences = null;
        
        #endregion

        #region BuiltIn Methods

        protected override void CreateGUI(string controlName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                _objectToFind = EditorGUILayout.ObjectField(ReferenceObjectLabel, _objectToFind, typeof(Object), false, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space(5f);

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

                        if (GUILayout.Button("Clear List", EditorStyles.miniButton))
                        {
                            Clear();
                        }
                    }
                    
                    EditorGUILayout.Space(5f);

                    if (_referenceObjects == null || _referenceObjects.Count == 0)
                    {
                        EditorGUILayout.LabelField("No GameObjects found containing desired reference");
                    }
                    else
                    {
                        for (int i = 0, count = _referenceObjects.Count; i < count; i++)
                        {
                            LayoutItem(_referenceObjects[i]);
                        }
                    }
                }

                if (_queueOfReferences != null)
                {
                    FindObjectReferences(_queueOfReferences);
                }
            }
        }

        #endregion

        #region Custom Methods

        private void FindObjectReferences(Object objectToFind)
        {
            
        }

        private void GetFilePathsFromExtension(string startingDirectory, string extension, ref List<string> listOfPaths)
        {
            try
            {
                // Go over main directory
                string[] initialFiles = Directory.GetFiles(startingDirectory);

                foreach (string file in initialFiles)
                {
                    if (string.IsNullOrEmpty(file) && file.EndsWith(extension))
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

                if (GUILayout.Button(RightArrowLabel, EditorStyles.miniButtonRight, GUILayout.MaxWidth(20f)))
                {
                    _queueOfReferences = obj;
                }
            }
        }

        private void Clear()
        {
            //Clear References here
        }

        #endregion
    }
}
