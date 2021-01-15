using UnityEditor;

namespace GursaanjTools
{
    public class IconContentReference_Editor : GuiControlEditorWindow, IHasCustomMenu
    {
        #region Variables
        
        private const string IconsPath = "iconsPath";

        private const string ContentName = "Icon";
        private const string PluralContent = "Icons";
        private const string SubDirectory = "Icons";
        
        private EditorResourceLogic _logic;
        private EditorIconContentCreator _contentCreator;
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            InternalEditorResourceImageInformation info = new InternalEditorResourceImageInformation(GetIconPath(),
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

        private string GetIconPath()
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.EditorResources.iconsPath;
#else
            return _logic.GetPath(IconsPath);
#endif
        }

        #endregion
    }

}