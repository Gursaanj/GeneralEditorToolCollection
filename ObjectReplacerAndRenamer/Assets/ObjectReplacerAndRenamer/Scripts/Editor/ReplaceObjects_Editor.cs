using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class ReplaceObjects_Editor : EditorWindow
    {
        #region Variabeles

        private static ReplaceObjects_Editor _window = null;
        private static readonly Vector2 _minSize = new Vector2(300,75);
        private static readonly Vector2 _maxSize = new Vector2(300,150);

        private const string _replaceSelectedObjects = "Replace Selected Objects";
        private const string _objectsToReplaceString = "Object to Replace";
        private const string _selectionCountString = "Selection Count: ";
        private const int _layoutButtonHeight = 40;
        
        //Display Dialogue Text
        private const string _errorTitle = "Error";
        private const string _noSelectedObjectsError = "At least one object needs to be selected to be replaced with";
        private const string _wantedObjectIsEmptyError = "The Replace object is empty, please assign something!";

        private const string _confirmationMessage = "Sounds good";
        private const string _cancellationMessage = "Actuall, no!";
        
        
        private int _currentSelectionCount = 0;
        private GameObject _wantedObject = null;
        #endregion

        #region Unity Methods
        
        public static void InitWindow()
        {
            _window = GetWindow<ReplaceObjects_Editor>();
            _window.titleContent = new GUIContent(_replaceSelectedObjects);
            _window.minSize = _minSize;
            _window.maxSize = _maxSize;
            _window.autoRepaintOnSceneChange = true;
            _window.Show();
        }

        //Todo: Add a flag asking if name change is desired
        private void OnGUI()
        {
            // Check the amount of selected Objects
            GetSelection();

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(string.Format("{0}{1}", _selectionCountString, _currentSelectionCount.ToString()), EditorStyles.boldLabel);
                EditorGUILayout.Space();

                _wantedObject =
                    (GameObject)EditorGUILayout.ObjectField(_objectsToReplaceString, _wantedObject, typeof(GameObject), true);
                EditorGUILayout.Space();

                if (GUILayout.Button(_replaceSelectedObjects, GUILayout.ExpandWidth(true),
                    GUILayout.Height(_layoutButtonHeight)))
                {
                    ReplaceSelectedObjects();
                }

                EditorGUILayout.Space();
            }


            Repaint();
        }

        #endregion

        #region Custom Methods

        private void GetSelection()
        {
            _currentSelectionCount = 0;
            _currentSelectionCount = Selection.gameObjects.Length;
        }
        
        private void ReplaceSelectedObjects()
        {
            if (_currentSelectionCount <= 0)
            {
                DisplayCustomError(_noSelectedObjectsError);
                return;
            }

            if (_wantedObject == null)
            {
                DisplayCustomError(_wantedObjectIsEmptyError);
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            for (int i = 0, count = selectedObjects.Length; i < count; i++)
            {
                GameObject selectedGameObject = selectedObjects[i];
                Transform selectedTransform = selectedGameObject.transform;
                GameObject newObject =
                    Instantiate(_wantedObject, selectedTransform.position, selectedTransform.rotation);
                newObject.transform.localScale = selectedTransform.localScale;
                newObject.tag = selectedGameObject.tag;
                newObject.layer = selectedGameObject.layer;
                newObject.hideFlags = selectedGameObject.hideFlags;
                
                Undo.RegisterCreatedObjectUndo(newObject, "Replacement");
                
                Undo.DestroyObjectImmediate(selectedObjects[i]);
            }

        }

        private void DisplayCustomError(string message)
        {
            EditorUtility.DisplayDialog(_errorTitle, message, _confirmationMessage);
        }

        #endregion
    }
}
