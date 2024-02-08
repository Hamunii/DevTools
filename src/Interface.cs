namespace DevTools {
    class Interface {
        internal static void Init(){
            int methodsCount = Config.patchMethods.Count;
            int methodsInvoked = 0;
            for(int i = 0; i < methodsCount; i++){
                if(Config.patchConfigsList[i].Value == false) continue;
                typeof(TestingLib.Patch).GetMethod(Config.patchMethods[i]).Invoke(null, null);
                methodsInvoked++;
            }
            Plugin.Logger.LogInfo($"Called {methodsInvoked} TestingLib.Patch methods.");

            if(Config.executeMethods.Count == 0 && Config.toolsMethods.Count == 0) return;
            TestingLib.OnEvent.PlayerSpawn += OnEvent_PlayerSpawn;

        }

        private static void OnEvent_PlayerSpawn()
        {
            int methodsCount = Config.executeMethods.Count;
            int methodsInvoked = 0;
            for(int i = 0; i < methodsCount; i++){
                if(Config.executeConfigsList[i].Value == false) continue;
                typeof(TestingLib.Execute).GetMethod(Config.executeMethods[i]).Invoke(null, null);
                methodsInvoked++;
            }
            Plugin.Logger.LogInfo($"Called {methodsInvoked} TestingLib.Execute methods.");

            methodsCount = Config.toolsMethods.Count;
            methodsInvoked = 0;
            for(int i = 0; i < methodsCount; i++){
                if(Config.toolsConfigsList[i].Value == "") continue;
                typeof(TestingLib.Tools).GetMethod(Config.toolsMethods[i]).Invoke(null, new object [] {Config.toolsConfigsList[i].Value});
                methodsInvoked++;
            }
            Plugin.Logger.LogInfo($"Called {methodsInvoked} TestingLib.Tools methods.");
        }
    }
}