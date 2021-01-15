using System.Collections;
using System.Collections.Generic;
using GursaanjTools;
using UnityEditor;
using UnityEngine;

public class BuiltinSkinReference_Editor : GuiControlEditorWindow, IHasCustomMenu
{
    #region Variables
    
    private const string SkinsPath = "BuiltinSkinsPath";

    private const string ContentName = "Skin";
    private const string PluralContent = "Skins";
    private const string SubDirectory = "Builtin Skins";
        
    private EditorResourceLogic _logic;
    private EditorIconContentCreator _contentCreator;
    
    #endregion

    #region Builtin Methods

    private void OnEnable()
    {
        InternalEditorResourceImageInformation info = new InternalEditorResourceImageInformation(GetBuiltinSkinsPath(),
            ContentName, PluralContent, SubDirectory, () => Close());
        _logic = new EditorResourceLogic();
        _contentCreator = new EditorIconContentCreator(info);
    }

    protected override void CreateGUI(string controlName)
    {
        _contentCreator.CreateWindowGUI(controlName, position);
    }

    #endregion

    #region IHasCustomMenu Implementation 
    
    public void AddItemsToMenu(GenericMenu menu)
    {
        _contentCreator?.AddItemsToMenu(menu);
    }

    #endregion

    #region Custom Methods

    private string GetBuiltinSkinsPath()
    {
#if UNITY_2018_3_OR_NEWER
        return SubDirectory;
#else
            return _logic.GetPath(IconsPath);
#endif
    }

    #endregion
}
