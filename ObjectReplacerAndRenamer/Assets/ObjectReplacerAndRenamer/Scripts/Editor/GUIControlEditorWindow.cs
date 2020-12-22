using System;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
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
        
        //Window Content
        protected static GUIContent TitleContent = new GUIContent();
        protected static Vector2 MinSize = new Vector2(300, 140);
        protected static Vector2 MaxSize = new Vector2(300, 180);
        
        
        //Control Name
        private const string PrimaryControlName = "Control";
        
        protected GameObject[] _selectedGameObjects;
        private bool _shouldFocusOnField = true;

        #endregion

        #region Builtin Methods
        
        public static void Init(Type type, EditorWindowInformation windowInformation)
        {
            _window = (GuiControlEditorWindow) GetWindow(type);
            SetWindowInformation(windowInformation);
            _window.titleContent = windowInformation.Title;
            _window.minSize = windowInformation.MinSize;
            _window.maxSize = windowInformation.MaxSize;
            _window.Focus();
            _window.Show();
        }

        protected void OnGUI()
        {
            CreateGUI(PrimaryControlName);
            FinishGUICycle();
        }

        #endregion

        #region Custom Methods

        protected abstract void CreateGUI(string controlName);
        
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

        protected bool DisplayDialogue(string subject, string message, bool canCancel)
        {
            _shouldFocusOnField = true;

            if (canCancel)
            {
                return EditorUtility.DisplayDialog(subject, message, ConfirmationMessage, CancellationMessage);
            }
            
            return EditorUtility.DisplayDialog(subject, message, ConfirmationMessage);
        }
        
        protected void FocusOnTextField()
        {
            if (_shouldFocusOnField && _window != null) 
            { 
                _window.Focus();
                
                //Use both GUI and EditorGUI to ensure focus is made regardless of choice of GUI
                GUI.FocusControl(PrimaryControlName);
                EditorGUI.FocusTextInControl(PrimaryControlName);
                _shouldFocusOnField = false;
            }
        }
        
        private static void SetWindowInformation(EditorWindowInformation information)
        {
            TitleContent = information.Title;
            MinSize = information.MinSize;
            MaxSize = information.MaxSize;
        }

        #endregion
    }
}

