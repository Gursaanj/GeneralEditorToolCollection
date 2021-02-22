using System;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    //Todo: Pressing Enter on DisplayDialogue presses Enter on main window as well, potentially creating infinite loop, Fix with Bool
    public abstract class GuiControlEditorWindow : EditorWindow
    {
        #region Variables
        protected static GuiControlEditorWindow _window;
        
        //GUI Labels
        protected const string SelectionCountString = "Number of Selected Objects: ";
        protected const string CastedCountFormat = "000";
        
        //Warning Messages
        protected const string ErrorTitle = "Error";
        protected const string UpdateTitle = "Update";
        protected const string AreYouSureTitle = "Are you sure";
        
        //Window Content
        protected static GUIContent TitleContent = new GUIContent();
        protected static Vector2 MinSize = new Vector2(300, 140);
        protected static Vector2 MaxSize = new Vector2(300, 180);
        
        private const string ConfirmationMessage = "Sounds good";
        private const string CancellationMessage = "Actually, no!";
        
        //Control Name
        private const string PrimaryControlName = "Control";
        
        protected GameObject[] _selectedGameObjects;
        protected bool _shouldFocusOnField = true;

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

            if (windowInformation.ShowUtility)
            {
                _window.ShowUtility();
            }
            else
            {
                _window.Show();
            }
        }

        protected void OnGUI()
        {
            CreateGUI(PrimaryControlName);
            FinishGUICycle();
        }

        #endregion

        #region Custom Methods

        protected abstract void CreateGUI(string controlName);
        
        private void FinishGUICycle()
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

        protected void CloseWindow()
        {
            if (_window != null)
            {
                _window.Close();
            }
        }

        private void FocusOnTextField()
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
        
        protected static void SetWindowInformation(EditorWindowInformation information)
        {
            TitleContent = information.Title;
            MinSize = information.MinSize;
            MaxSize = information.MaxSize;
        }

        #endregion
    }
}

