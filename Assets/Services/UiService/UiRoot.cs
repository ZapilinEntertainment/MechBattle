using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZE.UiService
{
    public class UiRoot : MonoBehaviour
    {
        [field:SerializeField] public RectTransform ActiveWindowsContainer { get; private set; }
        [field:SerializeField] public RectTransform DisabledWindowsContainer { get; private set; }
    }
}
