using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class ReplaceObjects_Editor : EditorWindow
    {
        #region Variabeles

        private static ReplaceObjects_Editor _window = null;
        private static readonly Vector2 MinSize = new Vector2(300,100);
        private static readonly Vector2 MaxSize = new Vector2(300,175);

        private const string ReplaceSelectedObjects = "Replace Selected Objects";
        private const string ObjectsToReplaceString = "Object to Replace";
        private const string SelectionCountString = "Selection Count: ";
        private const string ReplaceNameString = "Replace Name?";
        private const int LayoutButtonHeight = 40;
        
        //Display Dialogue Text
        private const string ErrorTitle = "Error";
        private const string NoSelectedObjectsError = "At least one object needs to be selected to be replaced with";
        private const string WantedObjectIsEmptyError = "The Replace object is empty, please assign something!";
        private const string ConfirmationMessage = "Sounds good";

        private const string UndoReplacementLabel = "Replacement";


        private int _currentSelectionCount = 0;
        private GameObject _wantedObject = null;
        private bool _shouldReplaceName = false;
        #endregion

        #region BuiltIn Methods
        
        public static void InitWindow()
        {
            _window = GetWindow<ReplaceObjects_Editor>();
            _window.titleContent = new GUIContent(ReplaceSelectedObjects);
            _window.minSize = MinSize;
            _window.maxSize = MaxSize;
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
                EditorGUILayout.LabelField($"{SelectionCountString}{_currentSelectionCount.ToString()}", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                _wantedObject =
                    (GameObject)EditorGUILayout.ObjectField(ObjectsToReplaceString, _wantedObject, typeof(GameObject), true);
                EditorGUILayout.Space();

                _shouldReplaceName = EditorGUILayout.Toggle(ReplaceNameString, _shouldReplaceName);
                
                EditorGUILayout.Space();
                if (GUILayout.Button(ReplaceSelectedObjects, GUILayout.ExpandWidth(true),
                    GUILayout.Height(LayoutButtonHeight)))
                {
                    ReplaceObjects();
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
        
        private void ReplaceObjects()
        {
            if (_currentSelectionCount <= 0)
            {
                DisplayCustomError(NoSelectedObjectsError);
                return;
            }

            if (_wantedObject == null)
            {
                DisplayCustomError(WantedObjectIsEmptyError);
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            for (int i = 0, count = selectedObjects.Length; i < count; i++)
            {
                GameObject selectedGameObject = selectedObjects[i];
                Transform selectedTransform = selectedGameObject.transform;
                GameObject newObject =
                    Instantiate(_wantedObject, selectedTransform.position, selectedTransform.rotation);

                if (!_shouldReplaceName)
                {
                    newObject.name = selectedGameObject.name;
                }

                newObject.transform.localScale = selectedTransform.localScale;
                newObject.tag = selectedGameObject.tag;
                newObject.layer = selectedGameObject.layer;
                newObject.hideFlags = selectedGameObject.hideFlags;
                
                Undo.RegisterCreatedObjectUndo(newObject, UndoReplacementLabel);
                
                Undo.DestroyObjectImmediate(selectedObjects[i]);
            }

        }

        private void DisplayCustomError(string message)
        {
            EditorUtility.DisplayDialog(ErrorTitle, message, ConfirmationMessage);
        }

        #endregion
    }
}
