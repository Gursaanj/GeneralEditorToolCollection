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
            EnforceSize();
            HandleEvents();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(15);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(15);
                    
                    GUI.SetNextControlName(controlName);
                    string previousInput = _input;
                    _input = GUILayout.TextField(_input, _inputFieldStyle, GUILayout.Height(60));
                    GUI.FocusControl(controlName);

                    if (_input != previousInput) //string.CompareOrdinal(_input, previousInput) != 0
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
                        GUILayout.Space(6);

                        if (!string.IsNullOrEmpty(_input))
                        {
                            VisualizeResults();
                        }
                        
                        GUILayout.Space(6);
                    }
                    
                    GUILayout.Space(15);
                }
                
                GUILayout.Space(15);
            }
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
            _oddEntryStyle = new GUIStyle("CN EntryBackodd");
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

            _results = _results.Take(10).ToList();
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
            currentRect.height = 90;
            
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(5f);
                int numberOfResults = _results.Count;

                if (numberOfResults == 0)
                {
                    currentRect.height += EditorGUIUtility.singleLineHeight;
                    GUILayout.Label("No Results");
                }

                for (int i = 0; i < numberOfResults; i++)
                {
                    GUIStyle style = i % 2 == 0 ? _evenEntryStyle : _oddEntryStyle;
                    
                    GUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight * 2),GUILayout.ExpandWidth(true)); 
                    Rect resultRect = GUILayoutUtility.GetRect(0,0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); 
                    GUILayout.EndHorizontal();
                    
                    currentRect.height += EditorGUIUtility.singleLineHeight * 2;

                    if (currentEvent.type == EventType.Repaint)
                    {
                        style.Draw(resultRect, false, false, i == _selectedResultIndex, false);
                        string assetPath = AssetDatabase.GUIDToAssetPath(_results[i]);
                        Texture icon = AssetDatabase.GetCachedIcon(assetPath);

                        Rect iconRect = resultRect;
                        iconRect.x = 30;
                        iconRect.width = 25;
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
                        labelRect.x = 60;
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

                currentRect.height += 5;
                position = currentRect;
            }
        }

        #endregion
    }
}
