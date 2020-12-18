using UnityEditor;

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

        [MenuItem("GursaanjTools/GameObject Tools/Rename Selected Objects")]
        public static void RenameObjectsTool()
        {
            RenameObjects_Editor.InitWindow();
        }

        #endregion

        #region Object Grouper

        [MenuItem("GursaanjTools/GameObject Tools/Group Selected Objects")]
        public static void GroupObjectsTool()
        {
            GroupObjects_Editor.InitWindow();
        }

        #endregion
    
    
    }
}

