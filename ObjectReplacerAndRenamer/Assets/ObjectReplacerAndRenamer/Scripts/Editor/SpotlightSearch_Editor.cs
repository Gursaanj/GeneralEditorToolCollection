using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GursaanjTools
{
    public class SpotlightSearch_Editor : GuiControlEditorWindow, IHasCustomMenu
    {
        #region Variables

        //GUI Labels

        //Warning Labels
        
        private const string SkinHighlightColor = "eeeeee";
        private const string SkinNormalColor = "222222";
        private const string InitialInput = "Open Asset...";
        private const string SearchTimelineKey = "SearchTimeline";
        
        //GUI Styles And Content
        private GUIStyle _inputFieldStyle;
        private GUIStyle _initialInputStyle;
        private GUIStyle _resultLabelStyle;
        private GUIStyle _evenEntryStyle;
        private GUIStyle _oddEntryStyle;

        private SearchTimeline _timeline;
        private string _input;
        private int _selectedResultIndex;
        private List<string> _results = new List<string>();
        
        #endregion


        #region BuiltIn Methods
        
        //Create On Init Function
        
        private void OnEnable()
        {
            CreateGUIStyles();
        }

        protected override void CreateGUI(string controlName)
        {
            
            
            
        }
        
        #endregion

        private void OnLostFocus()
        {
            Close();
        }

        #region IHasCustomMenu Interface

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Clear History"), false, () =>
            {
                EditorPrefs.SetString(SearchTimelineKey, JsonUtility.ToJson(new SearchTimeline()));
                Reset();
            });
            
            menu.AddItem(new GUIContent("Debug History", "Log current history within the console"), false, () =>
            {
                string currentHistory = EditorPrefs.GetString(SearchTimelineKey, JsonUtility.ToJson(new SearchTimeline()));
                Debug.Log(currentHistory);
            });
        }

        #endregion

        #region Custom Methods

        private void CreateGUIStyles()
        {
            _inputFieldStyle = new GUIStyle(EditorStyles.textField)
            {
                contentOffset = new Vector2(10,10),
                fontSize = 32,
                focused = new GUIStyleState()
            };

            _initialInputStyle = new GUIStyle(_inputFieldStyle)
            {
                normal =
                {
                    textColor = new Color(0.2f, 0.2f, 0.2f, 0.4f)
                }
            };

            _resultLabelStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            
            _evenEntryStyle = new GUIStyle("CN EntryBackEven");
            _oddEntryStyle = new GUIStyle("CN EntryBackOdd");
        }

        private void EnforceSize()
        {
            Rect currentPosition = position;
            currentPosition.width = 500;
            currentPosition.height = 90;
            position = currentPosition;
        }

        private void Reset()
        {
            _input = string.Empty;
            _results.Clear();
            string initialTimeline = EditorPrefs.GetString(SearchTimelineKey, JsonUtility.ToJson(new SearchTimeline()));
            _timeline = JsonUtility.FromJson<SearchTimeline>(initialTimeline);
            Focus();
        }

        private Object GetSelectedAsset()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(_results[_selectedResultIndex]);

            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            
            return AssetDatabase.LoadMainAssetAtPath(assetPath);
        }
        
        //always close window after opening asset
        private void OpenAsset()
        {
            Close();

            if (_results == null || _results.Count < _selectedResultIndex)
            {
                return;
            }
            
            bool couldOpen = AssetDatabase.OpenAsset(GetSelectedAsset());

            if (!couldOpen)
            {
                DisplayDialogue(ErrorTitle, "No Asset Path Found!!", false);
                return;
            }

            string currentGuid = _results[_selectedResultIndex];

            if (currentGuid == null)
            {
                DisplayDialogue(ErrorTitle, "No Asset Name!!", false);
                return;
            }

            if (!_timeline.ClickHistory.ContainsKey(currentGuid))
            {
                _timeline.ClickHistory[currentGuid] = 0;
            }

            _timeline.ClickHistory[currentGuid]++;
            EditorPrefs.SetString(SearchTimelineKey, JsonUtility.ToJson(_timeline));
        }

        private void Process()
        {
            _input = _input.ToLower();
            string[] hits = AssetDatabase.FindAssets(_input) ?? new string[0];
            _results = hits.ToList();
            
            //Sort for better selection visibility
            _results.Sort((first, second) =>
            {
                int firstScore;
                _timeline.ClickHistory.TryGetValue(first, out firstScore);
                int secondScore;
                _timeline.ClickHistory.TryGetValue(second, out secondScore);
                
                //Sort files based on searched with higher scores (alphabetically) Taken from:
                if (firstScore != 0 && secondScore != 0)
                {
                    string firstAssetName = Path.GetFileName(AssetDatabase.GUIDToAssetPath(first).ToLower());
                    string secondAssetName = Path.GetFileName(AssetDatabase.GUIDToAssetPath(second).ToLower());

                    if (firstAssetName.StartsWith(_input) && !secondAssetName.StartsWith(_input))
                    {
                        return -1;
                    }

                    if (!firstAssetName.StartsWith(_input) && secondAssetName.StartsWith(_input))
                    {
                        return 1;
                    }
                }

                return secondScore - firstScore;
            });

        }

        private void HandleEvents()
        {
            Event currentEvent = Event.current;
            
        }

        #endregion
    }
}
