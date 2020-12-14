using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public static class EditorMenus
    {
        #region Object Replacer Tool

        [MenuItem("GursaanjTools/GameObject Tools/Replace Selected Objects")]
        public static void ReplaceObjectsTool()
        {
            ReplaceObjects_Editor.InitWindow();
        }

        #endregion

        #region Object Renamer

        [MenuItem("GursaanjTools/GameObject Tools/Rename Selected Objects", true)]
        public static bool RenameObjectsToolValidator()
        {
            GameObject[] selection = Selection.gameObjects;
            return selection != null && selection.Length != 0;
        }
    
        [MenuItem("GursaanjTools/GameObject Tools/Rename Selected Objects")]
        public static void RenameObjectsTool()
        {
            RenameObjects_Editor.InitWindow();
        }

        #endregion
    
    
    }
}

