using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GursaanjTools
{
    public class SpotlightSearch_Editor : GuiControlEditorWindow, IHasCustomMenu
    {
        #region Variables
        
        private static float _xPosition;
        private static float _yPosition;
        
        //GUI Labels
        private const string NoResultsLabel = "No Results";
        private const string InitialInput = "Search For Asset...";

        private const int MainVerticalPadding = 15;
        private const int InputLabelHeight = 60;
        private const int LayoutPadding = 6;
        private const int ResultPadding = 5;
        private const int BaseHeight = 90;
        private const int IconXPosition = 30;
        private const int IconWidth = 25;
        
        //Warning Labels
        private const string NoAssetPathMessage = "No Asset Path Found!!";
        private const string NoAssetNameMessage = "No Asset Name!!";

        private const string SkinHighlightColor = "eeeeee";
        private const string SkinNormalColor = "222222";
        private const string SearchTimelineKey = "SearchTimeline";
        private const int NumberOfResultsToShow = 10;

        private const string ClearHistoryLabel = "Clear History";
        private const string DebugHistoryLabel = "Debug History";
        private const string DebugHistoryTooltip = "Log current history within the console";

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

        public static void Init(EditorWindowInformation windowInformation)
        {
            _window = GetWindow<SpotlightSearch_Editor>();
            _window.titleContent = windowInformation.Title;
            SetWindowInformation(windowInformation);
            
            _window.minSize = windowInformation.MinSize;
            _window.maxSize = windowInformation.MaxSize;
            
            _xPosition = (Screen.currentResolution.width / 2) - windowInformation.MaxSize.x;
            _yPosition = (Screen.currentResolution.height * 0.3f) - (windowInformation.MaxSize.y/2);
            
            Rect windowPosition = _window.position;
            windowPosition.x = _xPosition;
            windowPosition.y = _yPosition;
            _window.position = windowPosition; 
            
            EnforceSize();
            _window.ShowUtility();
        }

        private void OnEnable()
        {
            CreateGUIStyles();
            Reset();
        }

        protected override void CreateGUI(string controlName)
        {
            EnforceSize();
            HandleEvents();
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(MainVerticalPadding);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(MainVerticalPadding);
                    
                    GUI.SetNextControlName(controlName);
                    string previousInput = _input;
                    _input = GUILayout.TextField(_input, _inputFieldStyle, GUILayout.Height(InputLabelHeight));

                    if (string.CompareOrdinal(_input, previousInput) != 0)
                    {
                        Process();
                    }

                    if (_selectedResultIndex >= _results.Count)
                    {
                        _selectedResultIndex = _results.Count - 1;
                    }
                    else if (_selectedResultIndex < 0)
                    {
                        _selectedResultIndex = 0;
                    }

                    if (string.IsNullOrEmpty(_input))
                    {
                        GUI.Label(GUILayoutUtility.GetLastRect(), InitialInput, _initialInputStyle);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(LayoutPadding);

                        if (!string.IsNullOrEmpty(_input))
                        {
                            VisualizeResults();
                        }
                        
                        GUILayout.Space(LayoutPadding);
                    }
                    
                    GUILayout.Space(MainVerticalPadding);
                }
                
                GUILayout.Space(MainVerticalPadding);
            }
        }
        private void OnLostFocus()
        {
            Close();
        }
        
        #endregion
        

        #region IHasCustomMenu Interface

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(ClearHistoryLabel), false, () =>
            {
                EditorPrefs.SetString(SearchTimelineKey, JsonUtility.ToJson(new SearchTimeline()));
                Reset();
            });
            
            menu.AddItem(new GUIContent(DebugHistoryLabel, DebugHistoryTooltip), false, () =>
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
            _oddEntryStyle = new GUIStyle("CN EntryBackodd");
        }

        private static void EnforceSize()
        {
            Rect currentPosition = _window.position;
            currentPosition.x = _xPosition;
            currentPosition.y = _yPosition;
            _window.position = currentPosition;
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
                DisplayDialogue(ErrorTitle, NoAssetPathMessage, false);
                return;
            }

            string currentGuid = _results[_selectedResultIndex];

            if (currentGuid == null)
            {
                DisplayDialogue(ErrorTitle, NoAssetNameMessage, false);
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
                
                //Sort files based on searched with higher scores (based on Timeline Values). Taken from:
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

            _results = _results.Take(NumberOfResultsToShow).ToList();
        }

        private void HandleEvents()
        {
            Event currentEvent = Event.current;
            bool acceptInput = true;
            
            if (currentEvent.type == EventType.KeyUp)
            {
                acceptInput = true;
            }

            if (currentEvent.type == EventType.KeyDown && acceptInput)
            {
                KeyCode currentKeyCode = currentEvent.keyCode;
                
                switch (currentKeyCode)
                {
                    case KeyCode.UpArrow:
                        currentEvent.Use();
                        _selectedResultIndex--;
                        break;
                    case KeyCode.DownArrow:
                        currentEvent.Use();
                        _selectedResultIndex++;
                        break;
                    case KeyCode.Return:
                        OpenAsset();
                        currentEvent.Use();
                        break;
                    case KeyCode.Tab:
                        currentEvent.Use();
                        _selectedResultIndex++;
                        if (_selectedResultIndex >= _results.Count)
                        {
                            _selectedResultIndex = 0;
                        }
                        _shouldFocusOnField = true;
                        break;
                    case KeyCode.Escape:
                        Close();
                        break;
                }

                acceptInput = false;
            }
        }

        //Change to use EditorGuiLayout Scopes
        private void VisualizeResults()
        {
            Event currentEvent = Event.current;
            Rect currentRect = position;
            currentRect.height = BaseHeight;
            float rectHeight = EditorGUIUtility.singleLineHeight * 2;
            
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(ResultPadding);
                int numberOfResults = _results.Count;

                if (numberOfResults == 0)
                {
                    currentRect.height += EditorGUIUtility.singleLineHeight;
                    GUILayout.Label(NoResultsLabel);
                }

                for (int i = 0; i < numberOfResults; i++)
                {
                    GUIStyle style = i % 2 == 0 ? _evenEntryStyle : _oddEntryStyle;
                    
                    GUILayout.BeginHorizontal(GUILayout.Height(rectHeight),GUILayout.ExpandWidth(true)); 
                    Rect resultRect = GUILayoutUtility.GetRect(0,0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); 
                    GUILayout.EndHorizontal();
                    
                    currentRect.height += rectHeight;

                    if (currentEvent.type == EventType.Repaint)
                    {
                        style.Draw(resultRect, false, false, i == _selectedResultIndex, false);
                        string assetPath = AssetDatabase.GUIDToAssetPath(_results[i]);
                        Texture icon = AssetDatabase.GetCachedIcon(assetPath);

                        Rect iconRect = resultRect;
                        iconRect.x = IconXPosition;
                        iconRect.width = IconWidth;
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                        string assetName = Path.GetFileName(assetPath);
                        
                        StringBuilder fullAssetName = new StringBuilder();
                        int startOfName = assetName.ToLower().IndexOf(_input, StringComparison.Ordinal);
                        int endOfName = startOfName + _input.Length;
                        
                        // Sometimes the AssetDatabase finds assets without the search input in it.
                        if (startOfName == -1)
                        {
                            fullAssetName.Append($"<color=#{SkinNormalColor}>{assetName}</color>");
                        }
                        else
                        {
                            if (startOfName != 0)
                            {
                                fullAssetName.Append($"<color=#{SkinNormalColor}>{assetName.Substring(0, startOfName)}</color>");
                            }
                            
                            fullAssetName.Append($"<color=#{SkinHighlightColor}><b>{assetName.Substring(startOfName, endOfName - startOfName)}</b></color>");

                            if (endOfName != assetName.Length - endOfName)
                            {
                                fullAssetName.Append($"<color=#{SkinNormalColor}>{assetName.Substring(endOfName, assetName.Length - endOfName)}</color>");
                            }
                        }

                        Rect labelRect = resultRect;
                        labelRect.x = InputLabelHeight;
                        GUI.Label(labelRect, fullAssetName.ToString(), _resultLabelStyle);
                    }

                    if (currentEvent.type == EventType.MouseDown && resultRect.Contains(currentEvent.mousePosition))
                    {
                        _selectedResultIndex = i;
                        
                        //open asset on multiClick
                        if (currentEvent.clickCount >= 2)
                        {
                            OpenAsset();
                        }
                        else
                        {
                            Selection.activeObject = GetSelectedAsset();
                            EditorGUIUtility.PingObject(Selection.activeObject);
                        }
                        
                        Repaint();
                    }
                }

                currentRect.height += ResultPadding;
                position = currentRect;
            }
        }

        #endregion
    }
}
