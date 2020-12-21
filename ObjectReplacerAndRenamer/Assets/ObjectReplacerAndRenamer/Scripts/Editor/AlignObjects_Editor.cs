using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class AlignObjects_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI Labels
        private const string ReferenceObjectLabel = "Object to Align to";
        private const string AlignLabel = "Align Selected Objects";
        private const string PositionLabel = "Position";
        private const string RotationLabel = "Rotation";
        private const string ScaleLabel = "Scale";
        private const string CurrentValueLabel = "Current Values";
        private const string XValueLabel = "X";
        private const string YValueLabel = "Y";
        private const string ZValueLabel = "Z";

        private const float HorizontalPadding = 10f;
        private const float FieldVerticalPadding = 15f;
        private const float VerticalPadding = 5f;
        private const float ValuePadding = 30f;
        
        //Warning Labels
        private const string NothingSelectedWarning = "At least one object from the hierarchy needs to be chosen to align";
        private const string NoReferenceObjectWarning = "Please select an Object to align to!";

        //Undo Labels
        private const string UndoAlignLabel = "Alignment";
        
        private GameObject _referenceObject;
        private Transform _referenceTransform;
        private bool[] _positionAligners = new bool[3] {true, true, true};
        private bool[] _rotationAligners = new bool[3] {true, true, true};
        private bool[] _scaleAligners = new bool[3] {true, true, true};

        private bool _isPositionGroupEnabled = true;
        private bool _isRotationGroupEnabled = true;
        private bool _isScaleGroupEnabled = true;
        
        #endregion
        
        #region BuiltIn Methods
        
        
        
        protected override void CreateGUI(string controlName)
        {
            _selectedGameObjects = Selection.gameObjects;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(HorizontalPadding);

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField($"{SelectionCountString} {_selectedGameObjects.Length.ToString(CastedCountFormat)}");
                    EditorGUILayout.Space(VerticalPadding);
                    CreateToggleGroup(ref _isPositionGroupEnabled, ref _positionAligners, PositionLabel);
                    EditorGUILayout.Space(FieldVerticalPadding);
                    CreateToggleGroup(ref _isRotationGroupEnabled, ref _rotationAligners, RotationLabel);
                    EditorGUILayout.Space(FieldVerticalPadding);
                    CreateToggleGroup(ref _isScaleGroupEnabled, ref _scaleAligners, ScaleLabel);
                }
                
                GUILayout.FlexibleSpace();
                
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(ReferenceObjectLabel, EditorStyles.boldLabel);
                    _referenceObject = (GameObject)EditorGUILayout.ObjectField(_referenceObject, typeof(GameObject), true, GUILayout.Width(120f));
                }

                EditorGUILayout.Space(HorizontalPadding);
            }
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(AlignLabel, GUILayout.ExpandHeight(true)))
            {
                AlignObjects();
            }
        }

        #endregion

        #region Custom Methods

        private void AlignObjects()
        {
            if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
            {
                DisplayDialogue(ErrorTitle, NothingSelectedWarning, false);
                return;
            }

            if (_referenceObject == null)
            {
                DisplayDialogue(ErrorTitle, NoReferenceObjectWarning, false);
                return;
            }

            _referenceTransform = _referenceObject.transform;

            foreach (GameObject obj in _selectedGameObjects)
            {
                if (obj == null)
                {
                    continue;
                }
                
                obj.transform.position = GetAlignedVector(_isPositionGroupEnabled, _positionAligners,
                    obj.transform.position, _referenceTransform.position);
                
                obj.transform.rotation = Quaternion.Euler(GetAlignedVector(_isRotationGroupEnabled, _rotationAligners,
                    obj.transform.rotation.eulerAngles, _referenceTransform.rotation.eulerAngles));

                obj.transform.localScale = GetAlignedVector(_isScaleGroupEnabled, _scaleAligners,
                    obj.transform.localScale, _referenceTransform.transform.localScale);
            }
            
        }

        private void CreateToggleGroup(ref bool parentToggle, ref bool[] toggleArray, string parentLabel)
        {
            if (toggleArray == null || toggleArray.Length != 3 || string.IsNullOrEmpty(parentLabel))
            {
                Debug.LogWarning("Cannot Create Toggle Group");
            }

            parentToggle = EditorGUILayout.ToggleLeft(parentLabel, parentToggle, EditorStyles.boldLabel);
                    
            EditorGUILayout.Space(VerticalPadding);

            GUI.enabled = parentToggle;

            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(HorizontalPadding)))
            {
                GUILayout.FlexibleSpace();
                toggleArray[0] = EditorGUILayout.ToggleLeft(XValueLabel, toggleArray[0], GUILayout.Width(ValuePadding));
                toggleArray[1] = EditorGUILayout.ToggleLeft(YValueLabel, toggleArray[1], GUILayout.Width(ValuePadding));
                toggleArray[2] = EditorGUILayout.ToggleLeft(ZValueLabel, toggleArray[2], GUILayout.Width(ValuePadding));
                GUILayout.FlexibleSpace();
            }
                    
            GUI.enabled = true;
        }

        private Vector3 GetAlignedVector(bool parentToggle, bool[] toggleArray, Vector3 objVector, Vector3 refVector)
        {
            Vector3 newVector = objVector;

            if (toggleArray != null && toggleArray.Length == 3 && parentToggle)
            {
                if (toggleArray[0])
                {
                    newVector.x = refVector.x;
                }
                
                if (toggleArray[1])
                {
                    newVector.y = refVector.y;
                }

                if (toggleArray[2])
                {
                    newVector.z = refVector.z;
                }
            }

            return newVector;
        }

        #endregion
    }
}
