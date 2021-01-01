using System;
using System.Collections.Generic;
using UnityEngine;

namespace GursaanjTools
{
    [Serializable]
    public class SearchTimeline : ISerializationCallbackReceiver
    {
        public readonly Dictionary<string, int> ClickHistory = new Dictionary<string, int>();
        
        [SerializeField] List<string> _keys = new List<string>();
        [SerializeField] List<int> _values = new List<int>();

        #region ISerializationCallbackReceiver
        
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var click in ClickHistory)
            {
                _keys.Add(click.Key);
                _values.Add(click.Value);
            }
        }

        public void OnAfterDeserialize()
        { 
            ClickHistory.Clear();

            if (_keys == null || _values == null || _keys.Count != _values.Count)
            {
                Debug.LogError("Search History clicks dont add up");
                return;
            }

            for (int i = 0, count = _keys.Count; i < count; i++)
            {
                ClickHistory.Add(_keys[i], _values[i]);
            }
        }
        #endregion

        
    }
}
