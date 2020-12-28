using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class ScreenShotCapture_Editor : GuiControlEditorWindow
    {
        #region Variables

        //GUI Labels

        //Warning Labels

        private const string GameViewMenuItem = "Window/General/Game";
        private const string SceneViewMenuItem = "Window/General/Scene";
        
        private List<Camera> _cameras = new List<Camera>();
        
        #endregion

        #region BuiltIn Methods

        protected override void CreateGUI(string controlName)
        {
            var a = FindObjectsOfType<Camera>();

            if (GUILayout.Button("Debug"))
            {
                PrintCameras(a);
            }

        }

        #endregion

        #region Custom Methods

        private void PrintCameras(Camera[] list)
        {
            Debug.Log("Number of SceneView Cameras = " + list.Length);
        }
        #endregion

    }
}
