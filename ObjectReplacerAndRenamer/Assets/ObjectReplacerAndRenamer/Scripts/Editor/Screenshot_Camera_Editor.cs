using UnityEditor;
using UnityEngine;

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
                    
                    Texture image = GetTexture();
                    if (image != null)
                    {
                        EditorGUI.DrawPreviewTexture(new Rect(0,0,100,100), image);
                    }

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

            _cameraObjectNames = GetCameraNames(_cameras);
        }

        private string[] GetCameraNames(Camera[] cameras)
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

            if (camera == null)
            {
                return null;
            }

            int width = _dimensions.x;
            int height = _dimensions.y;

            RenderTexture renderTexture = new RenderTexture(width, height, 0, _renderTextureFormat);
            camera.targetTexture = renderTexture;

            // CameraClearFlags clearFlags = camera.clearFlags;
            // Color bgColor = new Color(); //0 alpha

            camera.Render();
            
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
            
            Texture2D screenshot = new Texture2D(width, height, _textureFormat,false);
            screenshot.ReadPixels(new Rect(0,0,width,height),0,0,false );
            
            //Add Quality Settings here
            // if(QualitySettings.activeColorSpace == ColorSpace.Linear) {
            //     Color[] pixels = screenshot.GetPixels();
            //     for(int p = 0; p < pixels.Length; p++) {
            //         pixels[p] = pixels[p].gamma;
            //     }
            //     screenshot.SetPixels(pixels);
            // }
            
            screenshot.Apply(false);

            camera.targetTexture = null;
            RenderTexture.active = currentRT;

            return screenshot;
        }

        #endregion
    }
}
