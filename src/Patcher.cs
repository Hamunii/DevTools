using System.Reflection;
using TestingLib;

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
            InvokeMethodsMarkedAs(Attributes.Available.Always);
            OnEvent.PlayerSpawn += PlayerSpawn;
        }

        private static void PlayerSpawn()
        {
            InvokeMethodsMarkedAs(Attributes.Available.PlayerSpawn);
        }

        private static void InvokeMethodsMarkedAs(Attributes.Available availability){
            int methodsInvoked = 0;
            foreach (var type in DevConfig.allTypes){
                if (type.GetCustomAttribute<Attributes.DevTools>().Time != availability)
                    continue;
                MethodBase[] methods = type.GetMethods();
                foreach (MethodBase method in methods)
                {
                    if(DevConfig.IsMethodBlacklisted(method))
                        continue;
                    // We don't check if a method is not Available.Always because such
                    // a method should never exist in a class marked as always available.

                    // This code is awful.
                    var methodParams = method.GetParameters();
                    if (methodParams.Length == 0 || methodParams[0].ParameterType == typeof(bool)) {
                        foreach(var config in DevConfig.boolConfigs){
                            if($"{type.Name}.{method.Name}" == config.Definition.Section){
                                if (methodParams.Length == 0){
                                    methodsInvoked++;
                                    method.Invoke(null, null);
                                }
                                else{
                                    methodsInvoked++;
                                    method.Invoke(null, new object[]{config.Value});
                                }
                                break;
                            }
                        }
                    }
                    else if (methodParams[0].ParameterType == typeof(string)){
                        foreach(var config in DevConfig.stringConfigs){
                            if($"{type.Name}.{method.Name}" == config.Definition.Section){
                                if (config.Value == "")
                                    break;
                                methodsInvoked++;
                                method.Invoke(null, new object[]{config.Value});
                                break;
                            }
                        }
                    }
                    else if (methodParams[0].ParameterType == typeof(int) || methodParams[0].ParameterType.IsEnum){
                        foreach(var config in DevConfig.intConfigs){
                            if($"{type.Name}.{method.Name}" == config.Definition.Section){
                                methodsInvoked++;
                                method.Invoke(null, new object[]{config.Value});
                                break;
                            }
                        }
                    }
                }
            }
            string[] attributeType = availability.GetType().ToString().Split(new char[] {'+'});
            Plugin.Logger.LogInfo($"Invoked {methodsInvoked} methods marked as: {attributeType[1]}.{availability}");
        }
    }
}