using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class ReplaceObjects_Editor : GuiControlEditorWindow
    {
        #region Variabeles
        
        //GUI Labels
        private const string ObjectsToReplaceString = "Object to Replace";
        private const string ReplaceNameString = "Replace Name?";
        private const int LayoutButtonHeight = 40;
        
        //Warning Messages
        private const string NoSelectedObjectsError = "At least one object needs to be selected to be replaced with";
        private const string WantedObjectIsEmptyError = "The Replace object is empty, please assign something!";
        
        //Undo Labels
        private const string UndoReplacementLabel = "Replacement";
        
        private GameObject _wantedObject = null;
        private bool _shouldReplaceName = false;
        #endregion

        #region Abstracts Methods
        protected override void CreateGUI(string controlName)
        {
            _selectedGameObjects = Selection.gameObjects;
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"{SelectionCountString}{_selectedGameObjects.Length.ToString(CastedCountFormat)}", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                _wantedObject =
                    (GameObject)EditorGUILayout.ObjectField(ObjectsToReplaceString, _wantedObject, typeof(GameObject), true);
                EditorGUILayout.Space();

                _shouldReplaceName = EditorGUILayout.Toggle(ReplaceNameString, _shouldReplaceName);
                
                EditorGUILayout.Space();
                if (GUILayout.Button(TitleContent.text, GUILayout.ExpandWidth(true),
                    GUILayout.Height(LayoutButtonHeight)))
                {
                    ReplaceObjects();
                }

                EditorGUILayout.Space();
            }
        }

        #endregion

        #region Custom Methods
        
        private void ReplaceObjects()
        {
            if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
            {
                DisplayDialogue(ErrorTitle, NoSelectedObjectsError, false);
                return;
            }

            if (_wantedObject == null)
            {
                DisplayDialogue(ErrorTitle, WantedObjectIsEmptyError, false);
                return;
            }
            
            for (int i = 0, count = _selectedGameObjects.Length; i < count; i++)
            {
                GameObject selectedGameObject = _selectedGameObjects[i];
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
                newObject.isStatic = selectedGameObject.isStatic;
                newObject.hideFlags = selectedGameObject.hideFlags;
                
                Undo.RegisterCreatedObjectUndo(newObject, UndoReplacementLabel);
                
                Undo.DestroyObjectImmediate(selectedGameObject);
            }

        }
        #endregion
    }
}
