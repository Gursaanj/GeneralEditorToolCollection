using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    //Todo: Options needed : Include Transparent background, Do you just want just the green/blue/red, so on. Place Texture Format, RenderTexture Format in advanced options
    //Todo: Allow bool for Alpha or Inverted Colors
    //Todo: Change entire guilayout to use scopes if possible
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
        private Texture _texture = null;
        private Vector2Int _dimensions = new Vector2Int(1024, 1024);
        private RenderTextureFormat _renderTextureFormat = RenderTextureFormat.Default;
        private TextureFormat _textureFormat = TextureFormat.RGB565;

        private bool _canDrawTexture = true;
        private bool doOnce = false;

        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            GetAllCameras();
            doOnce = true;
        }

        protected override void CreateGUI(string controlName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName(controlName);
                    _chosenCameraIndex = EditorGUILayout.Popup(new GUIContent(CameraLabel, CameraTooltip), _chosenCameraIndex, _cameraObjectNames);
                    _chosenCamera = _cameras[_chosenCameraIndex];
                    DoStuffWithCamera(_chosenCamera);

                    EditorGUILayout.Space(VerticalPadding);
                    
                    _dimensions = EditorGUILayout.Vector2IntField(new GUIContent(ResolutionLabel, ResolutionTooltip), _dimensions);
                    
                    EditorGUILayout.Space(VerticalPadding);
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _textureFormat = (TextureFormat)EditorGUILayout.EnumPopup(new GUIContent(TextureFormatLabel, TextureFormatTooltip), _textureFormat, x => SystemInfo.SupportsTextureFormat((TextureFormat)x), false);
                        EditorGUILayout.Space();
                        _renderTextureFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup(new GUIContent(RenderTextureLabel, RenderTextureTooltip), _renderTextureFormat, x => SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)x), false);
                    }

                    if (check.changed)
                    {
                        _canDrawTexture = true;
                    }
                }
                
                EditorGUILayout.Space(VerticalPadding);

                if (_canDrawTexture)
                {
                    _texture = GetTexture(_chosenCamera);
                    _canDrawTexture = false;
                }

                if (_texture != null)
                {
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(20, 125), new Vector2(position.width-40, position.height - 200)), _texture, null, ScaleMode.ScaleToFit);
                }
                //Todo : Add bold label at bottom of image stating the the image scene might not accurately depict the image and extreme resolutions
                //Todo : Add Manual Refresh Button

            }
        }

        private void Update()
        {
            if (_chosenCamera != null && _chosenCamera.transform.hasChanged)
            {
                _canDrawTexture = true;
                _chosenCamera.transform.hasChanged = false;
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

        private Texture2D GetTexture(Camera camera)
        {
            Debug.Log("In Use");
            
            //Camera camera = _cameras[_chosenCameraIndex];

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
            
            //RenderTexture Is too large, RenderTexture.Create, requested size is too large
            //SystemInfo.GetLargestTexture or something
            
            screenshot.Apply(false);

            camera.targetTexture = null;
            RenderTexture.active = currentRT;

            return screenshot;
        }
        
        private void DoStuffWithCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            if (doOnce)
            {
                doOnce = false;

                // PropertyInfo[] properties = camera.GetType().GetProperties();
                // PropertyInfo clearFlagPropertyInfo = camera.GetType().GetProperty("clearFlags");
                // FieldInfo[] fieldInfos = camera.GetType()
                //     .GetFields();
                //
                // foreach (var field in fieldInfos)
                // {
                //     Debug.Log($"Field: {field}");
                // }

                //Debug.Log(clearFlagPropertyInfo.GetValue(camera).ToString());
                //Todo Create Custom Editor (inheriting from Editor) for Camera to do Camera Editing
            }
        }

        #endregion
    }
}
