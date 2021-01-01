using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class SpotlightSearch_Editor : GuiControlEditorWindow, IHasCustomMenu
    {
        #region Variables

        //GUI Labels

        //Warning Labels


        //GUI Styles And Content

        private const string InitialInput = "Open Asset...";
        private const string SearchTimelineKey = "SearchTimeline";

        private SearchTimeline _timeline;
        private string _input;
        private int _selectedResultIndex;
        private List<string> _results = new List<string>();
        

        #endregion


        #region BuiltIn Methods

        private void OnEnable()
        {
            //Initiate all the GuiContents and GUIstyles
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
            
        }

        #endregion

        #region Custom Methods

        private void Reset()
        {
            _input = string.Empty;
            _results.Clear();
            string json = EditorPrefs.GetString(SearchTimelineKey, JsonUtility.ToJson(new SearchTimeline()));
            _timeline = JsonUtility.FromJson<SearchTimeline>(json);
            Focus();
        }

        #endregion
    }
}
