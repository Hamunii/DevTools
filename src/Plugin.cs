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

namespace DevTools {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(TestingLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin {
        public static Config DevToolsConfig { get; internal set; }
        internal static new ManualLogSource Logger;
        internal static string TestingLibLocation;
        private void Awake() {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // Should exist because HardDependency
            TestingLibLocation = BepInEx.Bootstrap.Chainloader.PluginInfos[TestingLib.Plugin.ModGUID].Location;

            DevToolsConfig = new(Config);
            Interface.Init();
        }
    }

    public class Config
    {
        // This is bad code, but it works for now.
        public static List<ConfigEntry<bool>> patchConfigsList;
        public static List<ConfigEntry<bool>> executeConfigsList;
        public static List<ConfigEntry<string>> toolsConfigsList;
        public static List<string> methodNames;
        public static List<string> methodDescriptions;
        public static List<string> patchMethods;
        public static List<string> executeMethods;
        public static List<string> toolsMethods;
        public Config(ConfigFile cfg)
        {
            patchConfigsList = new List<ConfigEntry<bool>>();
            executeConfigsList = new List<ConfigEntry<bool>>();
            toolsConfigsList = new List<ConfigEntry<string>>();
            methodNames = new List<string>();
            methodDescriptions = new List<string>();
            patchMethods = new List<string>();
            executeMethods = new List<string>();
            toolsMethods = new List<string>();
            // Temporary path

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

                if (!(type.Name == "Patch"
                   || type.Name == "Execute"
                   || type.Name == "Tools"))
                {
                    continue;
                }

                MemberInfo[] members = type.GetMethods();
                foreach (MemberInfo member in members)
                {
                    if(member.Name == "Equals"
                    || member.Name == "GetHashCode"
                    || member.Name == "GetType"
                    || member.Name == "ToString"
                    || member.Name == "RunAllPatchAndExecuteMethods"
                    || member.Name == "TeleportSelf") // need to make TeleportSelf work
                        continue;

                    AddConfigEntry(type, member, queryText, cfg);
                }
            }
        }
        static void AddConfigEntry(Type type, MemberInfo member, IEnumerable<string> queryText, ConfigFile cfg){
            string description;
            if(methodNames.Find(x => x.Contains(member.Name)) != null){
                // Format the description nicely.
                // I hate this code.
                description = queryText.ElementAt(methodNames.FindIndex(x => x.Contains(member.Name))).Trim();
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
            var listToAddTo = new List<ConfigEntry<bool>>();
            switch(type.Name){
                case "Patch":
                    patchMethods.Add(member.Name);
                    listToAddTo = patchConfigsList;
                    break;
                case "Execute":
                    executeMethods.Add(member.Name);
                    listToAddTo = executeConfigsList;
                    break;
                case "Tools":
                    toolsMethods.Add(member.Name);
                    toolsConfigsList.Add(
                        cfg.Bind($"{type.Name}.{member.Name}",
                            "Name",
                            "",
                            $"{description}"
                        )
                    );
                    return;
            }

            listToAddTo.Add(
                cfg.Bind($"{type.Name}.{member.Name}",
                    "Patch",
                    true,
                    $"{description}"
                )
            );
        }
    }
}