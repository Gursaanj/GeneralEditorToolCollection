using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    [InitializeOnLoad]
    public class GistImporter_Shortcut
    {
        static GistImporter_Editor window = ScriptableObject.CreateInstance<GistImporter_Editor>();
        
        static GistImporter_Shortcut()
        {
            SceneView.duringSceneGui += CheckIfShortCutWasPressed;
        }

        ~GistImporter_Shortcut()
        {
            SceneView.duringSceneGui -= CheckIfShortCutWasPressed;
        }
        
        private static void CheckIfShortCutWasPressed(SceneView view)
        {
            Event current = Event.current;
            bool correctModifiersPressed = current.modifiers == (EventModifiers.Control | EventModifiers.Shift) ||
                                           current.modifiers == (EventModifiers.Command | EventModifiers.Shift);
            
            if (current.isKey && correctModifiersPressed && current.keyCode == KeyCode.G)
            {
                window.ImportGist(EditorGUIUtility.systemCopyBuffer);
            }
        }
    }
}
