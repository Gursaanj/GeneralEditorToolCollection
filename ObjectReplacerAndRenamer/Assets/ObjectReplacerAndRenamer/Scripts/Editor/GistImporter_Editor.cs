using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    //Let Users Choose Where to import
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
        private const string AddedEditorFolderMessage =
            "Usage of the UnityEditor package was detected within the source file, Creating an Editor sub-folder to place this file within!";
        
        private const string MainFolderName = "Gists";
        private const string ReadMeFile = "readme.txt";
        private const string UsingEditorImport = "using UnityEditor;";
        private const string EditorFolderName = "Editor";
        private const string AssetsFolderName = "Assets";
        
        //Regex valueIds
        private const string regexURL = "url";
        private const string regexOwner = "owner";
        private const string regexGistId = "gistId";
        private const string regesxDescription = "description";

        private const string GithubMainURL = "https://gist.github.com";
        private const string ProperFileEnding = ".cs";
        private const string ImportMessage = "Imported using Gist Importer from the GursaanjTools Toolset";

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
                GUILayout.Label(GistURLLabel, EditorStyles.boldLabel, GUILayout.Width(30f));
                GUI.SetNextControlName(controlName);
                _gistURL = GUILayout.TextField(_gistURL, GUILayout.ExpandWidth(false));
                if (GUILayout.Button(ImportGistLabel, EditorStyles.miniButtonMid) || IsReturnPressed())
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
        public void ImportGist(string url)
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
                    string gistOwner = infoMatch[regexOwner].Value;
                    string gistId = infoMatch[regexGistId].Value;

                    string[] rawUrls = fileMatches.OfType<Match>()
                        .Select(match => $"{GithubMainURL}{match.Groups[regexURL].Value}")
                        .OrderByDescending(info => info.EndsWith(ProperFileEnding, StringComparison.OrdinalIgnoreCase)).ToArray();

                    if (rawUrls == null || rawUrls.Length == 0)
                    {
                        DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                        return;
                    }

                    string destinationFolder = Path.Combine(Application.dataPath, MainFolderName, gistOwner,
                        $"{Path.GetFileNameWithoutExtension(rawUrls.First())} ({gistId})");

                    for (int i = 0; i < rawUrls.Length; i++)
                    {
                        string rawUrl = rawUrls[i];
                        if (string.IsNullOrEmpty(rawUrl))
                        {
                            DisplayDialogue(ErrorTitle, UnableToImportWarning, false);
                            return;
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
                Debug.LogError(e);
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
            
            if (content.IndexOf(UsingEditorImport, StringComparison.Ordinal) >= 0)
            {
                DisplayDialogue(UpdateTitle, AddedEditorFolderMessage, false);
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

            string description = GistDescription.Match(pageContent).Groups[regesxDescription].Value;
            string readMePath = Path.Combine(folder, ReadMeFile);
            
            File.WriteAllText(readMePath, $"{description}\n\nUrl: {url}\nDate: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n{ImportMessage}");
            AssetDatabase.Refresh();
            
            //Ping the ReadMe after Completion
            var readMeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"{AssetsFolderName}{readMePath.Replace(Application.dataPath, string.Empty)}");
            Selection.activeObject = readMeObject;
            EditorGUIUtility.PingObject(readMeObject);
        }
        
        #endregion
    }
}
