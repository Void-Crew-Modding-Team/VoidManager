using CG.Input;
using System;

namespace VoidManager.Chat.Router
{
    internal class CursorUnlock : IShowCursorSource
    {
        internal static readonly CursorUnlock Instance = new();
        private CursorUnlock() { }

        internal static void OnChatOpened(object sender, EventArgs e)
        {
            CursorUtility.ShowCursor(Instance, true);
        }

        internal static void OnChatClosed(object sender, EventArgs e)
        {
            CursorUtility.ShowCursor(Instance, false);
        }
    }
}
