using UnityEditor;
using UnityEngine;

public class UngroupObjects_Editor : Editor
{
    #region Variables

    //Error Messages
    private const string _errorTitle = "Error";
    private const string _noObjectsMessage = "No Objects to Ungroup!";
    private const string _confirmationMessage = "Sure thing!";

    private const string _undoUngroupingLabel = "Ungrouping";

    private static GameObject[] _selectedGameObjects;

    #endregion

    #region Builtin Methods

    public static void Init()
    {
        _selectedGameObjects = Selection.gameObjects;
        UngroupObjects();
    }

    #endregion

    #region Custom Methods
    
    private static void UngroupObjects()
    {
        if (_selectedGameObjects == null || _selectedGameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(_errorTitle, _noObjectsMessage, _confirmationMessage);
            return;
        }
        

        for (int i = 0, count = _selectedGameObjects.Length; i < count; i++)
        {
            GameObject parent = _selectedGameObjects[i];

            if (parent.transform.childCount == 0)
            {
                continue;
            }

            int childCount = parent.transform.childCount;
            Transform[] childrenTransforms = new Transform[childCount];

            for (int j = 0, len = childCount; j < len; j++)
            {
                childrenTransforms[j] = parent.transform.GetChild(j);
            }

            for (int j = 0, len = childCount; j < len; j++)
            {
                Undo.SetTransformParent(childrenTransforms[j], parent.transform.parent, _undoUngroupingLabel);
            }
            
            Undo.DestroyObjectImmediate(parent);
        }


    }

    #endregion
}
