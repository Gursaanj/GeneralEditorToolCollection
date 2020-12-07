using System.Collections;
using System.Collections.Generic;
using GursaanjTools;
using UnityEditor;
using UnityEngine;

public static class EditorMenus
{
    [MenuItem("GursaanjTools/GameObject Tools/Replace Selected Objects")]
    public static void ReplaceObjectsTool()
    {
        ReplaceObjects_Editor.InitWindow();
    }

    [MenuItem("GursaanjTools/GameObject Tools/Rename Selected Objects")]
    public static void RenameObjectsTool()
    {
        
    }
}
