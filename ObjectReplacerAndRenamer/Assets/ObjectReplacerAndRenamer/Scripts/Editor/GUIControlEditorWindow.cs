using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    //TODO : Handle creation in another window - Not static
    public abstract class GuiControlEditorWindow : EditorWindow
    {
        #region Variables

        protected static GuiControlEditorWindow _window;
        
        //GUI Labels
        protected const string SelectionCountString = "Selection Count: ";
        protected const string CastedCountFormat = "000";
        
        //Warning Messages
        protected const string ErrorTitle = "Error";
        protected const string ConfirmationMessage = "Sounds good";
        protected const string CancellationMessage = "Actually, no!";


        protected GameObject[] _selectedGameObjects;

        protected List<string> _listOfControls = new List<string>();
        
        private bool _shouldFocusOnTextField = true;
        private int _currentControlIndex = 0;

        #endregion

        #region Properties

        #endregion
        
        #region Builtin Methods

        protected void OnGUI()
        {
            _selectedGameObjects = Selection.gameObjects;
            CreateGUI();
            FinishGUICycle();
        }

        #endregion

        #region Custom Methods

        protected abstract void CreateGUI();
        
        protected void FinishGUICycle()
        {
            FocusOnTextField();

            if (_window != null)
            {
                _window.Repaint();
            }
        }

        protected bool IsReturnPressed()
        {
            Event current = Event.current;
            return current.isKey && current.keyCode == KeyCode.Return;
        }

        protected void FocusOnTextField()
        {
            if (_listOfControls != null && _listOfControls.Count > 0)
            {
                ChangeCurrentControl();

                if (_shouldFocusOnTextField && _window != null)
                {
                    _window.Focus();
                    EditorGUI.FocusTextInControl(_listOfControls[_currentControlIndex]);
                    _shouldFocusOnTextField = false;
                }
            }
        }

        private void ChangeCurrentControl()
        {
            Event currentEvent = Event.current;

            if (currentEvent.isKey)
            {
                if (currentEvent.keyCode == KeyCode.DownArrow)
                {
                    if (_currentControlIndex < (_listOfControls.Count - 1))
                    {
                        _currentControlIndex++;
                        _shouldFocusOnTextField = true;
                    }
                }
                else if (currentEvent.keyCode == KeyCode.UpArrow)
                {
                    if (_currentControlIndex > 0)
                    {
                        _currentControlIndex--;
                        _shouldFocusOnTextField = true;
                    }
                }
            }
        }

        #endregion
    }
}

