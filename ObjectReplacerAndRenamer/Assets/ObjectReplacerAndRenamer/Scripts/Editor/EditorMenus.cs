using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    // TODO: Create Static Class to hold titles and window Sizes
    public static class EditorMenus
    {
        #region Object EditorWindows

        #region Object Replacer Tool
        
        [MenuItem("GursaanjTools/GameObject Tools/Replace Selected Objects")]
        public static void ReplaceObjectsTool()
        {
            GUIContent title = new GUIContent("Replace Selected Objects");
            Vector2 minSize = new Vector2(300,100);
            Vector2 maxSize = new Vector2(300,175);
            ReplaceObjects_Editor.Init(typeof(ReplaceObjects_Editor), title, minSize, maxSize);
        }

        #endregion

        #region Object Renamer

        [MenuItem("GursaanjTools/GameObject Tools/Rename Selected Objects")]
        public static void RenameObjectsTool()
        {
            GUIContent title = new GUIContent("Rename Selected Objects");
            Vector2 minSize = new Vector2(300,140);
            Vector2 maxSize = new Vector2(300,180);
            RenameObjects_Editor.Init(typeof(RenameObjects_Editor), title, minSize, maxSize);
        }

        #endregion

        #region Object Grouper

        [MenuItem("GursaanjTools/GameObject Tools/Group Selected Objects %#z")]
        public static void GroupObjectsTool()
        {
            GUIContent title = new GUIContent("Group Selected Objects");
            Vector2 minSize = new Vector2(300,140);
            Vector2 maxSize = new Vector2(300,140);
            GroupObjects_Editor.Init(typeof(GroupObjects_Editor), title, minSize, maxSize);
        }

        #endregion

        #region Object Ungrouper
        
        [MenuItem("GursaanjTools/GameObject Tools/Ungroup Selected Objects %#q")]
        public static void UngroupObjectsTool()
        {
            UngroupObjects_Editor.Init();
        }

        #endregion

        #region Object Aligner

        [MenuItem("GursaanjTools/GameObject Tools/Align Selected Objects")]
        public static void AlignObjectsTool()
        {
            GUIContent title = new GUIContent("Align Selected Objects");
            Vector2 minSize = new Vector2(350,290);
            Vector2 maxSize = new Vector2(350,290);
            AlignObjects_Editor.Init(typeof(AlignObjects_Editor), title, minSize, maxSize);
        }

        #endregion

        #endregion
    }
}

