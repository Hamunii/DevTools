using System.Collections.Generic;
using System.Reflection;
using DevTools.GUI;
using TestingLib;
using UnityEngine;

namespace DevTools {
    class Patcher {
        internal static void Init() {
            On.GameNetworkManager.StartHost += GameNetworkManager_StartHost;
            On.MenuManager.StartAClient += MenuManager_StartAClient;
        }

        private static void MenuManager_StartAClient(On.MenuManager.orig_StartAClient orig, MenuManager self)
        {
            orig(self);
            // Disable patches in case previously host
            Patch.UnpatchAll();
            OnEvent.PlayerSpawn -= PlayerSpawn;
        }

        private static void GameNetworkManager_StartHost(On.GameNetworkManager.orig_StartHost orig, GameNetworkManager self)
        {
            orig(self);
            ModMenu.canOpenDevToolsMenu = true;
            if(!ModMenu.menuExists && DevConfig.addModMenu.Value){
                Plugin.myGUIObject = new GameObject("DevToolsGUI");
                Object.DontDestroyOnLoad(Plugin.myGUIObject);
                Plugin.myGUIObject.hideFlags = HideFlags.HideAndDontSave;
                Plugin.myGUIObject.AddComponent<ModMenu>();
                ModMenu.menuExists = true;

                // Add methods to menu
                ModMenu.menuMethods = new List<MethodListing>();
                foreach(var methodListing in DevConfig.allMethods){
                    if(!(methodListing.visibility == Attributes.Visibility.Whitelist
                      || methodListing.visibility == Attributes.Visibility.MenuOnly))
                        continue;
                    ModMenu.menuMethods.Add(methodListing);
                }
            }
            InvokeMethodsMarkedAs(Attributes.Available.Always);
            OnEvent.PlayerSpawn += PlayerSpawn;
        }

        private static void PlayerSpawn()
        {
            InvokeMethodsMarkedAs(Attributes.Available.PlayerSpawn);
        }

        private static void InvokeMethodsMarkedAs(Attributes.Available availability){
            int methodsInvoked = 0;
            foreach(var methodListing in DevConfig.allMethods){
                if(methodListing.type.GetCustomAttribute<Attributes.DevTools>().Time != availability)
                    continue;
                if(!(methodListing.visibility == Attributes.Visibility.Whitelist
                  || methodListing.visibility == Attributes.Visibility.ConfigOnly))
                    continue;
                // This is bad code.
                if(methodListing.valueType == typeof(bool)){
                    if(methodListing.value == true.ToString()){
                        methodListing.methodBase.Invoke(null, null);
                        methodsInvoked++;
                        if(!ModMenu.menuExists)
                            continue;
                        if(methodListing.type.Name == "Patch"){
                            methodListing.state = true;
                        }
                    }
                }
                else if(methodListing.valueType == typeof(string)){
                    if(methodListing.value != ""){
                        methodListing.methodBase.Invoke(null, new object[]{methodListing.value});
                        methodsInvoked++;
                        if(!ModMenu.menuExists)
                            continue;
                        if(methodListing.type.Name == "Patch"){
                            methodListing.state = true;
                        }
                    }
                }
                else if(methodListing.valueType == typeof(int)){
                    if(int.Parse(methodListing.value) != 0){
                        methodListing.methodBase.Invoke(null, new object[]{int.Parse(methodListing.value)});
                        methodsInvoked++;
                        if(!ModMenu.menuExists)
                            continue;
                        if(methodListing.type.Name == "Patch"){
                            methodListing.state = true;
                        }
                    }
                }
            }
            string[] attributeType = availability.GetType().ToString().Split(new char[] {'+'});
            Plugin.Logger.LogInfo($"Invoked {methodsInvoked} methods marked as: {attributeType[1]}.{availability}");
        }
    }
}