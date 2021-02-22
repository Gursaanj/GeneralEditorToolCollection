using System.Collections.Generic;

namespace GursaanjTools
{
    public static class ProjectSetupData
    {
        //Main Directories
        private const string m_art = "Art";
        private const string m_scripts = "Scripts";
        private const string m_audio = "Audio";
        private const string m_prefabs = "Prefbas";
        private const string m_scenes = "Scenes";
        
        //Sub Directories
        private static List<string> m_artSubDirectories = new List<string>
            {"Animation", "Objects", "Materials", "Shaders", "Fonts"};

        private static List<string> m_scriptsSubDirectories =
            new List<string> {"Editor", "Managers", "Shaders", "UI", "Misc"};
        
        private static List<string> m_audioSubDirectories =
            new List<string> {"Sound Effects", "Background Music", "UI"};
        
        private static List<string> m_prefabsSubDirectories =
            new List<string> {"Characters", "Environment", "Props", "UI", "Misc"};
        
        private static List<string> m_scenesSubDirectories = new List<string>();
        
        //List of Scene Names
        private static List<string> m_sceneNames = new List<string>{"StartScreen", "SandBox", "Loading"};

        public static SortedList<string, List<string>> GetProjectStructure()
        {
            SortedList<string, List<string>> directories = new SortedList<string, List<string>>();
            
            //Add directories to dictionary
            directories.Add(m_art, m_artSubDirectories);
            directories.Add(m_scripts, m_scriptsSubDirectories);
            directories.Add(m_audio, m_audioSubDirectories);
            directories.Add(m_prefabs, m_prefabsSubDirectories);
            directories.Add(m_scenes, m_scenesSubDirectories);

            return directories;
        }

        public static string GetSceneDirectory()
        {
            return m_scenes;
        }

        public static List<string> GetSceneNames()
        {
            return m_sceneNames;
        }

        public static string[] GetDirectoryNames(this string[] directoryPaths)
        {
            if (directoryPaths == null || directoryPaths.Length == 0)
            {
                return null;
            }

            string[] tempHolder = null;
            string[] directoryNames = new string[directoryPaths.Length];

            for (int i = 0, count = directoryPaths.Length; i < count; i++)
            {
                tempHolder = directoryPaths[i].Split('\\');
                directoryNames[i] = tempHolder[tempHolder.Length - 1];
            }

            return directoryNames;
        }
    }
}

