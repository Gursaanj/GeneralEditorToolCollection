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
        private const string XValueLabel = "X:";
        private const string YValueLabel = "Y:";
        private const string ZValueLabel = "Z:";

        private const float HorizontalPadding = 10f;
        private const float ValuePadding = 3f;
        
        //Warning Labels
        private const string NothingSelectedWarning = "At least one object from the hierarchy needs to be chosen to align";
        private const string NoReferenceObjectWarning = "Please select an Object to align to!";

        //Undo Labels
        private const string UndoAlignLabel = "Alignment";
        
        private GameObject _referenceObject;
        private bool[] _positionAligners = new bool[3] {true, true, true};
        private bool[] _rotationAligners = new bool[3] {true, true, true};
        private bool[] _scaleAligners = new bool[3] {true, true, true};

        private bool isPositionGroupEnabled = true;
        private bool isRotationGroupEnabled = true;
        private bool isScaleGroupEnabled = true;
        
        #endregion
        
        #region Abstract Methods

        protected override void CreateGUI(string controlName)
        {
            _selectedGameObjects = Selection.gameObjects;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(HorizontalPadding);

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField($"{SelectionCountString} {_selectedGameObjects.Length.ToString(CastedCountFormat)}");
                }
                
                GUILayout.FlexibleSpace();
                
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(ReferenceObjectLabel, EditorStyles.boldLabel);
                }

                EditorGUILayout.Space(HorizontalPadding);
            }
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(AlignLabel))
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
        }
        
        #endregion
    }
}
