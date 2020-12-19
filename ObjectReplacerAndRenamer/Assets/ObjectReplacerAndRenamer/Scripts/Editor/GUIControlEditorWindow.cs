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

        //Control Name
        private const string PrimaryControlName = "Control";
        
        protected GameObject[] _selectedGameObjects;
        private bool _shouldFocusOnField = true;

        #endregion

        #region Builtin Methods

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
        
        #endregion
    }
}

