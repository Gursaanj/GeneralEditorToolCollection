using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class UngroupObjects_Editor : Editor
    {
        #region Variables
    
        //Error Messages
        private const string ErrorTitle = "Error";
        private const string NoObjectsMessage = "No Objects to Ungroup!";
        private const string ConfirmationMessage = "Sure thing!";
    
        private const string UndoUngroupingLabel = "Ungrouping";
    
        private static GameObject[] _selectedGameObjects;
    
        #endregion
    
        #region Builtin Methods
    
        public static void Init()
        {
            _selectedGameObjects = Selection.gameObjects;
            UngroupObjects();
        }
    
        #endregion
    
        #region Custom Methods
        
        private static void UngroupObjects()
        {
            if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog(ErrorTitle, NoObjectsMessage, ConfirmationMessage);
                return;
            }
            
    
            for (int i = 0, count = _selectedGameObjects.Length; i < count; i++)
            {
                GameObject parent = _selectedGameObjects[i];
    
                if (parent.transform.childCount == 0)
                {
                    continue;
                }
    
                int childCount = parent.transform.childCount;
                Transform[] childrenTransforms = new Transform[childCount];
    
                for (int j = 0, len = childCount; j < len; j++)
                {
                    childrenTransforms[j] = parent.transform.GetChild(j);
                }
    
                for (int j = 0, len = childCount; j < len; j++)
                {
                    Undo.SetTransformParent(childrenTransforms[j], parent.transform.parent, UndoUngroupingLabel);
                }
                
                Undo.DestroyObjectImmediate(parent);
            }
    
    
        }
    
        #endregion
    }
}

