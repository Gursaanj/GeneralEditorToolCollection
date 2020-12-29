using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace GursaanjTools
{
    public class Screenshot_Camera_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI Labels
        private const string CameraLabel = "Camera";
        private const string CameraTooltip = "Choose which (enabled) camera you would like to take the Screen capture from";
        private const string ResolutionLabel = "Resolution";
        private const string ResolutionTooltip = "Designate the Specific Resolution you would like for the Screen capture";
        private const string RefreshCameraLabel = "Refresh Camera Selection";

        private const string TextureFormatLabel = "Texture Format";
        private const string TextureFormatTooltip = "";
        private const string RenderTextureLabel = "RenderTexture Format";
        private const string RenderTextureTooltip = "";

        //Warning Labels
        private const string NoCamerasWarning = "No cameras in current Scene";

        private const float VerticalPadding = 10f;
        
        private Camera[] _cameras;
        private string[] _cameraObjectNames;
        private int _chosenCameraIndex;
        private Camera _chosenCamera;
        private Vector2Int _dimensions = new Vector2Int(1024, 512);
        private RenderTextureFormat _renderTextureFormat = RenderTextureFormat.Default;
        private TextureFormat _textureFormat = TextureFormat.RGB565;
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            GetAllCameras();
        }

        protected override void CreateGUI(string controlName)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    GUI.SetNextControlName(controlName);
                    _chosenCameraIndex = EditorGUILayout.Popup(new GUIContent(CameraLabel, CameraTooltip), _chosenCameraIndex, _cameraObjectNames);
                    
                    EditorGUILayout.Space(VerticalPadding);
                    
                    _dimensions = EditorGUILayout.Vector2IntField(new GUIContent(ResolutionLabel, ResolutionTooltip), _dimensions);
                    
                    EditorGUILayout.Space(VerticalPadding);
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        //_dimensions = EditorGUILayout.Vector2IntField(new GUIContent(ResolutionLabel, ResolutionTooltip), _dimensions);
                        _textureFormat = (TextureFormat)EditorGUILayout.EnumPopup(new GUIContent(TextureFormatLabel, TextureFormatTooltip), _textureFormat);
                        EditorGUILayout.Space();
                        _renderTextureFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup(new GUIContent(RenderTextureLabel, RenderTextureTooltip), _renderTextureFormat);
                    }
                    
                    EditorGUILayout.Space(VerticalPadding);

                    // if (GUILayout.Button("DrawTexture"))
                    // {
                    //     Texture image = GetTexture();
                    //     EditorGUI.DrawPreviewTexture(new Rect(Vector2.zero, new Vector2(100,100)), image);
                    // }

                    Texture image = GetTexture();
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(150,150), new Vector2(100, 100)), image);

                }
            }
        }

        #endregion

        #region Custom Methods

        private void GetAllCameras()
        {
            _cameras = Camera.allCameras;

            if (_cameras == null || _cameras.Length == 0)
            {
                DisplayDialogue(ErrorTitle, NoCamerasWarning, false);
                Close();
            }

            _cameraObjectNames = GetNames(_cameras);
        }

        private string[] GetNames(Camera[] cameras)
        {
            string[] cameraNames = new string[cameras.Length];

            for (int i = 0; i < cameras.Length; i++)
            {
                cameraNames[i] = _cameras[i].gameObject.name;
            }

            return cameraNames;
        }

        private Texture2D GetTexture()
        {
            Camera camera = _cameras[_chosenCameraIndex];
            
            //Most Likely, impossible to reach
            if (camera == null)
            {
                return null;
            }

            int width = _dimensions.x;
            int height = _dimensions.y;
            
            // camera.targetTexture = new RenderTexture(width, height, 0, _renderTextureFormat);
            // RenderTexture renderTexture = camera.targetTexture;
            
            RenderTexture renderTexture = new RenderTexture(width, height, 0, _renderTextureFormat);

            camera.Render();

            Texture2D outputTexture = new Texture2D(width, height, _textureFormat, false);
            RenderTexture.active = renderTexture;
            outputTexture.ReadPixels(new Rect(0,0,width,height),0,0);
            outputTexture.Apply();
            
            return outputTexture;
        }

        #endregion
    }
}
