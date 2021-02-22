using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GursaanjTools
{
    public class ProjectSetup_Editor : GuiControlEditorWindow
    {
        #region Variables

        private const string ProjectNameLabel = "Project Name: ";
        private const string CreateProjectButtonLabel = "Create Project Structure";
        private const string DefaultProjectName = "Game";

        private const string SceneExtension = ".unity";
        private const string DirectoryCreationFormat = "{0}/{1}";
        private const string SceneCreationFormat = "{0}_{1}";
        
        //Error Messages
        private const string NoDirectoryNameError = "The Project needs a name for a directory to be created.";
        private readonly string GenericDirectoryError =  $"Are you sure you would like to create a directory for {DefaultProjectName}";

        private const float LabelSpacing = 5f;

        private string _projectName = DefaultProjectName;
        
        #endregion

        #region BuiltIn Methods

        protected override void CreateGUI(string controlName)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUI.SetNextControlName(controlName);
                GUILayout.Label(ProjectNameLabel);
                //GUILayout.Space(LabelSpacing);
                _projectName = GUILayout.TextField(_projectName, GUILayout.Width(200f));
            }
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CreateProjectButtonLabel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)) || IsReturnPressed())
            {
                CreateProjectDirectories();
            }
        }

        #endregion

        #region Custom Methods

        private void CreateProjectDirectories()
        {
            if (string.IsNullOrEmpty(_projectName))
            {
                DisplayDialogue(ErrorTitle, NoDirectoryNameError, false);
                return;
            }

            if (string.Equals(_projectName, DefaultProjectName))
            {
                if (!DisplayDialogue(AreYouSureTitle, GenericDirectoryError, true))
                {
                    return;
                }
            }

            _projectName = IndexProjectNameIfNeeded(_projectName);
            
            //Create Main Directory
            string rootPath = string.Format(DirectoryCreationFormat, Application.dataPath, _projectName);
            DirectoryInfo rootInfo = Directory.CreateDirectory(rootPath);
            if (!rootInfo.Exists)
            {
                return;
            }
            
            CreateSubFolders(rootPath);
            
            AssetDatabase.Refresh();
            CloseWindow();
        }
        
        private void CreateSubFolders(string root)
        {
            List<string> folderNames = new List<string>();
            
            SortedList<string, List<string>> directories = ProjectSetupData.GetProjectStructure();

            if (string.IsNullOrEmpty(root) || directories == null)
            {
                return;
            }

            foreach (string directory in directories.Keys)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    continue;
                }

                string newRootPath = string.Format(DirectoryCreationFormat, root, directory);
                DirectoryInfo rootInfo = Directory.CreateDirectory(newRootPath);
                List<string> subDirectories = directories[directory];

                if (rootInfo.Exists && subDirectories != null)
                {
                    folderNames.Clear();

                    foreach (string subDirectory in subDirectories)
                    {
                        if (string.IsNullOrEmpty(subDirectory))
                        {
                            continue;
                        }
                        
                        folderNames.Add(subDirectory);
                    }

                    CreateFolders(newRootPath, folderNames);
                    
                    //Special case for adding Scenes Folder
                    if (string.Equals(directory, ProjectSetupData.GetSceneDirectory()))
                    {
                        List<string> sceneNames = ProjectSetupData.GetSceneNames();

                        if (sceneNames != null)
                        {
                            foreach (string sceneName in sceneNames)
                            {
                                if (string.IsNullOrEmpty(sceneName))
                                {
                                    continue;
                                }

                                CreateScene(newRootPath, string.Format(SceneCreationFormat, _projectName, sceneName));
                            }
                        }
                    }
                }
            }
        }
        
        private string IndexProjectNameIfNeeded(string projectName)
        {
            string newProjectName = projectName;
            int successionIndex = 1;

            string[] mainDirectories = Directory.GetDirectories(Application.dataPath).GetDirectoryNames();

            if (mainDirectories != null)
            {
                while (mainDirectories.Contains(newProjectName))
                {
                    newProjectName = $"{projectName}{successionIndex}";
                    successionIndex++;
                }
            }

            return newProjectName;
        }

        private void CreateFolders(string path, List<string> folderNames)
        {
            if (string.IsNullOrEmpty(path) || folderNames == null || folderNames.Count == 0)
            {
                return;
            }

            foreach (string folderName in folderNames)
            {
                Directory.CreateDirectory(string.Format(DirectoryCreationFormat, path, folderName));
            }
        }

        private void CreateScene(string path, string sceneName)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            Scene currentScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            string fullSceneName = $"{sceneName}{SceneExtension}";
            EditorSceneManager.SaveScene(currentScene, String.Format(DirectoryCreationFormat, path, fullSceneName), true);
        }
        
        #endregion
        
    }
}
