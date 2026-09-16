using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RainbowBlockSaga.Presentation.Scripts.Utils
{
    public class DefineSymbolsUtils
    {
        public static void SwichSymbol(string symbol)
        {
            #if UNITY_EDITOR
            var _buildTargets = GetBuildTargets();
            foreach (var _target in _buildTargets)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_target);
                if (!defines.Contains(symbol))
                {
                    defines = defines + "; " + symbol;
                }
                else
                {
                    defines.Replace(symbol, "");
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(_target, defines);
            }
            #endif
        }

        public static void AddSymbol(string symbol)
        {
            #if UNITY_EDITOR
            var _buildTargets = GetBuildTargets();
            foreach (var _target in _buildTargets)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_target);
                AddDefine(symbol, _target, ref defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(_target, defines);
            }
            #endif
        }

        #if UNITY_EDITOR
        public static void DeleteSymbol(string symbol)
        {
            var _buildTargets = GetBuildTargets();
            foreach (var _target in _buildTargets)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_target);
                DeleteDefine(symbol, _target, ref defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(_target, defines);
            }
        }

        private static void DeleteDefine(string symbol, BuildTargetGroup buildTargetGroup, ref string defines)
        {
            defines = defines.Replace(symbol, "");
        }

        private static BuildTargetGroup[] GetBuildTargets()
        {
            var _targetGroupList = new ArrayList();
            _targetGroupList.Add(BuildTargetGroup.Standalone);
            _targetGroupList.Add(BuildTargetGroup.Android);
            _targetGroupList.Add(BuildTargetGroup.iOS);
            _targetGroupList.Add(BuildTargetGroup.WSA);
            return (BuildTargetGroup[])_targetGroupList.ToArray(typeof(BuildTargetGroup));
        }

        private static void AddDefine(string symbols, BuildTargetGroup _target, ref string defines)
        {
            if (!defines.Contains(symbols))
            {
                defines = defines + "; " + symbols;
            }
        }
        #endif
    }
}
