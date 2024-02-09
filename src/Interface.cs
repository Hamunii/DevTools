using System.Reflection;

namespace DevTools {
    class Interface {
        internal static void Init() {
            InvokeMethodsMarkedAs(TestingLib.Available.Always);
            TestingLib.OnEvent.PlayerSpawn += OnEvent_PlayerSpawn;
        }

        private static void OnEvent_PlayerSpawn() {
            InvokeMethodsMarkedAs(TestingLib.Available.PlayerSpawn);
        }

        private static void InvokeMethodsMarkedAs(TestingLib.Available availability){
            int methodsInvoked = 0;
            foreach (var type in Config.allTypes){
                if (type.GetCustomAttribute<TestingLib.DevTools>().Time != availability)
                    continue;
                MethodBase[] methods = type.GetMethods();
                foreach (MethodBase method in methods)
                {
                    if(Config.IsMethodBlacklisted(method))
                        continue;
                    // We don't check if a method is not Available.Always because such
                    // a method should never exist in a class marked as always available.

                    // This code is awful.
                    var methodParams = method.GetParameters();
                    if (methodParams.Length == 0 || methodParams[0].ParameterType == typeof(bool)) {
                        foreach(var config in Config.boolConfigs){
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
                        foreach(var config in Config.stringConfigs){
                            if($"{type.Name}.{method.Name}" == config.Definition.Section){
                                methodsInvoked++;
                                method.Invoke(null, new object[]{config.Value});
                                break;
                            }
                        }
                    }
                }
            }
            
            Plugin.Logger.LogInfo($"Invoked: {methodsInvoked} methods of type: {availability.GetType()}");
        }
    }
}