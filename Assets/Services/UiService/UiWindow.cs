using System;
using UnityEngine;
using ZE.Utils;

namespace ZE.UiService
{
    public abstract class UiWindow : MonoBehaviour
    {
        public static string GetWindowDefaultAssetName<T>() where T : UiWindow => StringExtension.ToSnakeCaseWithAcronyms(typeof(T).Name);
        public string GetWindowDefaultAssetName() => StringExtension.ToSnakeCaseWithAcronyms( GetType().Name);
    }
}
