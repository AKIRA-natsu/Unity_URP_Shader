using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static partial class Utility {
        #region Editor
#if UNITY_EDITOR
    /// <summary>
    /// <para>编辑器模式下，加载T</para>
    /// <para> AssetDatabase.LoadAllAssetsAtPath </para>
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T LoadAssetAtPath<T>(this string path) where T : UnityEngine.Object {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    /// <summary>
    /// <para>编辑器模式下，加载T[]</para>
    /// <para> AssetDatabase.LoadAllAssetsAtPath </para>
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> LoadAllAssetsAtPath<T>(this string path) where T : UnityEngine.Object {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>();
    }
#endif
    #endregion
    
    public static void Log(this object obj) => Debug.Log($"<color=#FFFFFF><b>{obj}</b></color>");
    public static void Warn(this object obj) => Debug.LogWarning($"<color=#FFFF00><b>{obj}</b></color>");
    public static void Error(this object obj) => Debug.LogError($"<color=#FF0000><b>{obj}</b></color>");
}