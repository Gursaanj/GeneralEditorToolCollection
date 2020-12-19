using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public abstract class GuiControlEditorWindow : EditorWindow
    {
        //Have ReturnPressed
        //Have FocusControl and ControlSwitching
        //Handle creation in another window - Not static

        #region Variables

        //protected List<string> _listOfControls = new List<string>();

        #endregion
        
        #region Builtin Methods
        
        #endregion

        #region Custom Methods

        protected bool IsReturnPressed()
        {
            Event current = Event.current;
            return current.isKey && current.keyCode == KeyCode.Return;
        }
        
        #endregion
    }
}

