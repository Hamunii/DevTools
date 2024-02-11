using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using TestingLib;

namespace DevTools {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(TestingLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin {
        public static DevConfig DevToolsConfig { get; internal set; }
        internal static new ManualLogSource Logger;
        internal static string TestingLibLocation;
        private void Awake() {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // Should exist because HardDependency
            TestingLibLocation = BepInEx.Bootstrap.Chainloader.PluginInfos[TestingLib.Plugin.ModGUID].Location;

            DevToolsConfig = new(Config);
            Patcher.Init();
        }
    }

    public class DevConfig
    {
        // This is bad code, but it works for now.
        public static List<ConfigEntry<bool>> boolConfigs;
        public static List<ConfigEntry<string>> stringConfigs;
        public static List<ConfigEntry<int>> intConfigs;
        public static List<Type> allTypes;
        private static List<string> methodNames;
        public DevConfig(ConfigFile cfg)
        {
            // new stuff
            boolConfigs = new List<ConfigEntry<bool>>();
            stringConfigs = new List<ConfigEntry<string>>();
            intConfigs = new List<ConfigEntry<int>>();
            
            allTypes = new List<Type>();
            methodNames = new List<string>();

            Assembly ass = Assembly.LoadFile(Plugin.TestingLibLocation);
            XDocument doc = new XDocument();
            try{
                doc = XDocument.Load(Path.Combine(Path.GetDirectoryName(Plugin.TestingLibLocation), "TestingLib.xml"));
            }
            catch(Exception e){
                Plugin.Logger.LogError($"Could not load 'TestingLib.xml'!\n{e}");
                // We are going to crash, but I don't care.
            }

            // Also this for some reason makes <c> tags cancel <br> tags?????? 
            var queryText = from c in doc.Root.Descendants("member")
                where c.Attribute("name") != null
                select c.Element("summary").Value;
            var queryName = from c in doc.Root.Descendants("member")
                where c.Attribute("name") != null
                select c.Attribute("name").Value;

            foreach(var item in queryName){
                methodNames.Add(item);
            }

            Type[] types = ass.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetCustomAttribute<Attributes.DevTools>() == null
                ||  type.GetCustomAttribute<Attributes.DevTools>().Visibility != Attributes.Visibility.Whitelist)
                {
                    continue;
                }
                allTypes.Add(type);

                MethodBase[] methods = type.GetMethods();
                foreach (MethodBase method in methods)
                {
                    if(IsMethodBlacklisted(method))
                        continue;

                    AddConfigEntry(type, method, queryText, cfg);
                }
            }

            // Thanks to Kittenji for this piece of code:
            PropertyInfo orphanedEntriesProp = cfg.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg, null);
            orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
            cfg.Save(); // Save the config file
        }

        public static bool IsMethodBlacklisted(MethodBase method){
            if(method.Name == "Equals"
                || method.Name == "GetHashCode"
                || method.Name == "GetType"
                || method.Name == "ToString"
                || (method.GetCustomAttribute<Attributes.DevTools>() != null
                && method.GetCustomAttribute<Attributes.DevTools>().Visibility == Attributes.Visibility.Blacklist))
                return true;
            return false;
        }

        static void AddConfigEntry(Type type, MethodBase method, IEnumerable<string> queryText, ConfigFile cfg){
            string description;
            if(methodNames.Find(x => x.Contains(method.Name)) != null){
                // Format the description nicely.
                // I hate this code.
                description = queryText.ElementAt(methodNames.FindIndex(x => x.Contains(method.Name))).Trim();
                string[] lines = description.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                List<string> strings = new List<string>();
                foreach(var line in lines){
                    StringBuilder sb = new StringBuilder(line);
                    strings.Add(sb.ToString().Trim());
                }
                description = string.Join( "\n", strings);
            }
            else{
                description = "Description not found.";
            }


            var methodParams = method.GetParameters();
            // I hate this code too. It's horrible.
            if (methodParams.Length == 0) {
                boolConfigs.Add(
                    cfg.Bind($"{type.Name}.{method.Name}",
                        "Patch",
                        true,
                        $"{description}"
                    )
                );
            }
            // We assume no method has more than 1 parameter lol
            else if(methodParams[0].ParameterType == typeof(bool)){
                boolConfigs.Add(
                    cfg.Bind($"{type.Name}.{method.Name}",
                        methodParams[0].Name,
                        true,
                        $"{description}"
                    )
                );
            }
            else if(methodParams[0].ParameterType == typeof(string)){
                stringConfigs.Add(
                    cfg.Bind($"{type.Name}.{method.Name}",
                        methodParams[0].Name,
                        "",
                        $"{description}"
                    )
                );
            }
            else if(methodParams[0].ParameterType == typeof(int) || methodParams[0].ParameterType.IsEnum){
                intConfigs.Add(
                    cfg.Bind($"{type.Name}.{method.Name}",
                        methodParams[0].Name,
                        0,
                        $"{description}"
                    )
                );
            }
        }
    }
}