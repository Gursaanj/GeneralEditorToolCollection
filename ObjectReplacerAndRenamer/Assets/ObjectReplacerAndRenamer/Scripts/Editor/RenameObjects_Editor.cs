using UnityEditor;
using UnityEngine;

public class RenameObjects_Editor : EditorWindow
{
    #region Variables

    private static RenameObjects_Editor _window = null;
    private static readonly Vector2 _minSize = new Vector2(300,100);
    private static readonly Vector2 _maxSize = new Vector2(300,175);

    private const string _renameSelectedObjects = "Rename Selected Objects";

    #endregion

    #region Unity Methods

    public static void InitWindow()
    {
        _window = GetWindow<RenameObjects_Editor>();
        _window.titleContent = new GUIContent(_renameSelectedObjects);
        _window.minSize = _minSize;
        _window.maxSize = _maxSize;
        _window.autoRepaintOnSceneChange = true;
        _window.Show();
    }

    #endregion

    #region Custom Methods

    

    #endregion
}
