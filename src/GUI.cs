using System.Collections.Generic;
using UnityEngine;

namespace DevTools.GUI
{
    internal class ModMenu : MonoBehaviour
    {
        public bool isMenuOpen;
        private float MENUWIDTH = 800;
        private float MENUHEIGHT = 400;
        private float MENUX;
        private float MENUY;
        private float ITEMWIDTH = 300;
        private float CENTERX;
        private float scrollStart;

        private GUIStyle menuStyle;
        private GUIStyle enableButtonStyle;
        private GUIStyle hScrollStyle;
        private GUIStyle vScrollStyle;

        private Vector2 scrollPosition;
        internal static bool DevToolsMenuOpen = false;
        internal static bool canOpenDevToolsMenu = true;
        public static List<MethodListing> menuMethods;
        internal static bool menuExists = false;
        private void Awake()
        {
            isMenuOpen = false;
            MENUWIDTH = Screen.width / 6;
            MENUHEIGHT = Screen.width / 4;
            ITEMWIDTH = MENUWIDTH / 1.2f;

            // this is center at center of menu
            //MENUX = (Screen.width / 2) - (MENUWIDTH / 2);

            // this is center at left side of menu
            //MENUX = (Screen.width / 2);

            // this is right off the edge of the screen on the right side
            MENUX = Screen.width - MENUWIDTH;
            MENUY = (Screen.height / 2) - (MENUHEIGHT / 2);
            CENTERX = MENUX + ((MENUWIDTH / 2) - (ITEMWIDTH / 2));
            scrollStart = MENUY + 30;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void IntitializeMenu()
        {
            if (menuStyle == null)
            {
                menuStyle = new GUIStyle(UnityEngine.GUI.skin.box);
                enableButtonStyle = new GUIStyle(UnityEngine.GUI.skin.button);
                hScrollStyle = new GUIStyle(UnityEngine.GUI.skin.horizontalScrollbar);
                vScrollStyle = new GUIStyle(UnityEngine.GUI.skin.verticalScrollbar);

                menuStyle.normal.textColor = Color.white;
                menuStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.01f, 0.1f, .9f));
                menuStyle.fontSize = 18;
                menuStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

                enableButtonStyle.normal.textColor = Color.white;
                enableButtonStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 0.01f, 0.2f, .9f));
                enableButtonStyle.hover.background = MakeTex(2, 2, new Color(0.4f, 0.01f, 0.1f, .9f));
                enableButtonStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

                hScrollStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.01f, 0.1f, 0f)); ;
            }
        }

        private void OnGUI()
        {
            if(menuStyle == null) { IntitializeMenu(); }
            if(!canOpenDevToolsMenu) { return; }
            if(!DevToolsMenuOpen) return;
            UnityEngine.GUI.Box(new Rect(MENUX, MENUY, MENUWIDTH, MENUHEIGHT), "DevTools Menu", menuStyle);
            scrollPosition = UnityEngine.GUI.BeginScrollView(new Rect(MENUX, MENUY + 30, MENUWIDTH, MENUHEIGHT - 50), scrollPosition, new Rect(MENUX, scrollStart, ITEMWIDTH, menuMethods.Count * 30), false, true, hScrollStyle, vScrollStyle);

            int idx = 0;
            foreach (var method in menuMethods)
            {   
                // I know this code is not great. But it works for now.
                if(method.state == true){
                    if (UnityEngine.GUI.Button(new Rect(CENTERX, MENUY + 30 + (idx * 30), ITEMWIDTH, 30), $"{method.type.Name}.{method.methodBase.Name}", enableButtonStyle))
                    {
                        if(method.methodBase.Name != "PatchAll")
                            method.state = false;
                        menuMethods.Find(x => x.methodBase.Name.Equals("UnpatchAll")).methodBase.Invoke(null, null);
                        foreach(var _method in menuMethods)
                        {
                            if(_method.type.Name == "Patch"){
                                if(_method.state == true){
                                    if(_method.methodBase.Name != "PatchAll"){
                                        _method.methodBase.Invoke(null, null);
                                    }
                                }
                            }
                        }
                    }
                }
                else{
                    if (UnityEngine.GUI.Button(new Rect(CENTERX, MENUY + 30 + (idx * 30), ITEMWIDTH, 30), $"{method.type.Name}.{method.methodBase.Name}"))
                    {
                        method.methodBase.Invoke(null, null);
                        if(method.type.Name == "Patch" && method.methodBase.Name != "PatchAll"){
                            method.state = true;
                        }
                        if(method.methodBase.Name == "PatchAll"){
                            foreach(var _method in menuMethods)
                            {
                                if(_method.type.Name == "Patch"){
                                    if(_method.methodBase.Name != "UnpatchAll" && _method.methodBase.Name != "PatchAll")
                                        _method.state = true;
                                }
                            }
                        }
                        else if(method.methodBase.Name == "UnpatchAll"){
                            foreach(var _method in menuMethods)
                            {
                                _method.state = false;
                            }
                        }
                    }
                }
                idx++;
            }

            // End the scroll view that we began above.
            UnityEngine.GUI.EndScrollView();
        }
    }
}
