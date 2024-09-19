using System;
using UnityEngine;
using VoidManager.CustomGUI;
using VoidManager.Utilities;

namespace VoidManager.Progression
{
    internal class ProgressionDisabledGUI : MonoBehaviour
    {
        internal static ProgressionDisabledGUI Instance { get; private set; }

        void Update()
        {
            WindowPos = new Rect(Screen.width - 200, 0, 140, 50);
        }

        GameObject PDCanvas;

        void Awake()
        {
            Instance = this;
            PDCanvas = new GameObject("ModManagerCanvas", new Type[] { typeof(Canvas) });
            Canvas canvasComponent = PDCanvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1000;
            canvasComponent.transform.SetAsLastSibling();
            DontDestroyOnLoad(PDCanvas);
        }

        Rect WindowPos;

        static Color invisColor = new Color(0f, 0f, 0f, 0f);
        Texture2D Background = GUITools.BuildTexFrom1Color(invisColor);

        void OnGUI()
        {
            if (!ProgressionHandler.ProgressionEnabled)
            {
                GUI.backgroundColor = invisColor;
                WindowPos = GUI.Window(999909, WindowPos, WindowFunction, $"<b><color={GUIMain.AllMPTypeColorCode}>Progress Disabled</color></b>");
            }
        }

        void WindowFunction(int WindowID)
        {
            //GUILayout.Label($"<b><color={GUIMain.AllMPTypeColorCode}>Progress Disabled</color></b>");
        }
    }
}
