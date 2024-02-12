using DevTools.GUI;

namespace DevTools {
    class ToggleGUI {
        internal static void Init() {
            On.QuickMenuManager.OpenQuickMenu += QuickMenuManager_OpenQuickMenu;
            On.QuickMenuManager.CloseQuickMenu += QuickMenuManager_CloseQuickMenu;
            On.QuickMenuManager.LeaveGameConfirm += QuickMenuManager_LeaveGameConfirm;
        }

        private static void QuickMenuManager_OpenQuickMenu(On.QuickMenuManager.orig_OpenQuickMenu orig, QuickMenuManager self)
        {
            orig(self);
            ModMenu.DevToolsMenuOpen = true;
        }

        private static void QuickMenuManager_CloseQuickMenu(On.QuickMenuManager.orig_CloseQuickMenu orig, QuickMenuManager self)
        {
            orig(self);
            ModMenu.DevToolsMenuOpen = false;
        }

        private static void QuickMenuManager_LeaveGameConfirm(On.QuickMenuManager.orig_LeaveGameConfirm orig, QuickMenuManager self)
        {
            orig(self);
            ModMenu.DevToolsMenuOpen = false;
            ModMenu.canOpenDevToolsMenu = false;
        }
    }
}