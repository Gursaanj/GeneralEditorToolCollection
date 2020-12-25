using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class GistImporter_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI labels
        private const string GistURLLabel = "Gist URL";
        private const string ImportGistLabel = "Import Gist";

        private const string ImportGistProgressBar = "Importing Gist...";
        
        //Warning Labels
        private const string WrongURLWarning = "Not the appropriate URL for importing";
        private const string UnableToImportWarning = "Sorry, was unable to import the Gist!";
        
        private const string TempFolderName = "Gists";
        private const string ReadMeFile = "readme.txt";
        private const string UsingEditorImport = "using UnityEditor;";
        private const string EditorFolderName = "Editor";
        
        private readonly Regex GistURLInfo = new Regex("https://gist.github.com/(?<owner>.+)/(?<gistId>[a-z0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex GistDescription = new Regex(@"\<title\>(?<description>.+)\</title\>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex FileURL = new Regex("href=\"(?<url>.+/raw/[a-z0-9\\./\\-]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string _gistURL = "Enter Gist URL here!";

        #endregion

        #region BuiltIn Methods

        protected override void CreateGUI(string controlName)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _gistURL = EditorGUILayout.TextField(GistURLLabel, _gistURL, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

                if (GUILayout.Button(ImportGistLabel, EditorStyles.miniButtonMid))
                {
                    ImportGist(_gistURL);
                }
            }

        }

        #endregion

        #region Custom Methods
        
        /// <summary>
        /// retrieve data from URL and download appropriate files
        /// </summary>
        /// <param name="url"></param>
        private void ImportGist(string url)
        {
            if (!GistURLInfo.IsMatch(url))
            {
                DisplayDialogue(ErrorTitle, WrongURLWarning, false);
                return;
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    string pageContent = client.DownloadString(url);

                    if (string.IsNullOrEmpty(pageContent))
                    {
                        DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                        return;
                    }

                    MatchCollection fileMatches = FileURL.Matches(pageContent);
                    int fileMatchCount = fileMatches.Count;

                    if (fileMatchCount == 0)
                    {
                        DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                        return;
                    }

                    GroupCollection infoMatch = GistURLInfo.Match(url).Groups;
                    string gistOwner = infoMatch["owner"].Value;
                    string gistId = infoMatch["gistId"].Value;

                    string[] rawUrls = fileMatches.OfType<Match>()
                        .Select(match => $"https://gist.github.com{match.Groups["url"].Value}")
                        .OrderByDescending(info => info.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)).ToArray();

                    if (rawUrls == null || rawUrls.Length == 0)
                    {
                        DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                        return;
                    }

                    string destinationFolder = Path.Combine(Application.dataPath, TempFolderName, gistOwner,
                        $"{Path.GetFileNameWithoutExtension(rawUrls.First())} ({gistId})");

                    for (int i = 0; i < rawUrls.Length; i++)
                    {
                        string rawUrl = rawUrls[i];
                        if (string.IsNullOrEmpty(rawUrl))
                        {
                            Debug.LogError("Was unable to read a raw URL, please try again or manually import GIST!");
                            continue;
                        }

                        string fileName = Path.GetFileName(rawUrl);

                        EditorUtility.DisplayProgressBar(ImportGistProgressBar, fileName, i / (float) fileMatchCount);
                        DownloadFile(client, rawUrl, destinationFolder, fileName);
                    }

                    EditorUtility.ClearProgressBar();
                    CreateReadMe(url, pageContent, destinationFolder);
                }
            }
            catch (Exception e)
            {
                DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void DownloadFile(WebClient client, string url, string destinationFolder, string fileName)
        {
            string content = client.DownloadString(url);

            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(destinationFolder) || string.IsNullOrEmpty(fileName))
            {
                DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                return;
            }

            //Create Editor Folder If Needed: In reality only do this if Editor folder not already in destination folder
            if (content.IndexOf(UsingEditorImport, StringComparison.Ordinal) >= 0)
            {
                destinationFolder = Path.Combine(destinationFolder, EditorFolderName);
            }

            Directory.CreateDirectory(destinationFolder);
            File.WriteAllText(Path.Combine(destinationFolder, fileName), content);
            AssetDatabase.Refresh();

        }

        private void CreateReadMe(string url, string pageContent, string folder)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(folder))
            {
                DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                return;
            }

            string description = GistDescription.Match(pageContent).Groups["description"].Value;
            string readMePath = Path.Combine(folder, ReadMeFile);
            
            File.WriteAllText(readMePath, $"{description}\n\nUrl: {url}\nDate: {DateTime.Now:dd/MM/yyyy HH:mm}\n\nImported using Gist Importer");
            AssetDatabase.Refresh();
            
            //Ping the ReadMe after Completion
            var readMeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"Assets{readMePath.Replace(Application.dataPath, string.Empty)}");
            Selection.activeObject = readMeObject;
            EditorGUIUtility.PingObject(readMeObject);
            
        }

        #endregion
    }
}
