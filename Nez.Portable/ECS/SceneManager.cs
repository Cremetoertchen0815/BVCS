using Microsoft.Xna.Framework.Input;
using Nez.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nez
{
    public class SceneManager : GlobalManager, ITelegramReceiver
    {
        private KeyboardState lastkstate;
        private string numstring;
        private bool numactive;
        private Dictionary<int, (Type, bool)> Scenes = new Dictionary<int, (Type, bool)>();
        public string Namespace;
        public Assembly BaseAssembly;

		public string TelegramSender => "sceneman";

		public SceneManager(Assembly baseAssembly, string rootNamespace)
        {
            Rescan(baseAssembly, rootNamespace); //First scan

            //Register telegram service
            this.TeleRegister();
        }

        public void Rescan(Assembly baseAssembly = null, string rootNamespace = null)
        {
            Namespace = rootNamespace ?? Namespace;
            BaseAssembly = baseAssembly ?? BaseAssembly;
            Scenes.Clear();

            //Fetch all scenes from assembly
            var allScenes = baseAssembly.GetTypes().Where(x => x.Namespace != null && x.Namespace.StartsWith(rootNamespace) & x.IsClass & !x.IsAbstract & x.IsSubclassOf(typeof(Scene))).ToArray();
            //Add the ones with SceneManArgs to list
            foreach (var element in allScenes)
            {
                var arg = element.GetAttribute<ManagedScene>();
                if (arg != null) Scenes.Add(arg.SceneNumber, (element, arg.AcceptsArgument));
            }
        }

#if DEBUG
        public override void Update()
        {
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.PageUp) & !lastkstate.IsKeyDown(Keys.PageUp)) { numactive = true; numstring = ""; } //Start listening
            else if (kstate.IsKeyDown(Keys.PageDown) & !lastkstate.IsKeyDown(Keys.PageDown) & numstring != "" & numactive)
            {
                SwitchToScene(numstring); //End listening & transition scene
            }
            else if (numactive) //Record key presses
            {
                foreach (var key in kstate.GetPressedKeys()) if (lastkstate.IsKeyUp(key) & key >= Keys.NumPad0 & key <= Keys.NumPad9) numstring += Convert.ToInt32(key - Keys.NumPad0).ToString();
            }
            //Ignore key press if was already pressed previous frame or is not a numberpad key, else add digit to string

            lastkstate = kstate;
        }
#endif

        public void SwitchToScene(string IDstr)
        {
            int ID = int.Parse(IDstr);

            //Is scene being called parameterless
            if (Scenes.ContainsKey(ID))
            {
                //Create object with parameterless constructor
                var constructor = Scenes[ID].Item1.GetConstructors().Where(x => x.GetParameters().Length == 0).ToArray(); //Grab parameterless constructors
                if (constructor.Length == 1) Core.Scene = (Scene)constructor[0].Invoke(null); //Invoke constructor and set current scene to the resulting object
            }
            else
            {
                //Is scene called with parameter or with invalid ID
                foreach (var scene in Scenes)
                {
                    if (scene.Value.Item2 && IDstr.StartsWith(scene.Key.ToString())) //If parameters are wanted and the number start with scene ID
                    {
                        string param = IDstr.Substring(scene.Key.ToString().Length); //Evaluate parameter
                        var constructors = scene.Value.Item1.GetConstructors().Where(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.Equals(typeof(string))).ToArray(); //Get appropriate constructor
                        if (constructors.Length == 1)
                        {
                            //Invoke constructor
                            Core.Scene = (Scene)constructors[0].Invoke(new string[] { param });
                            break;
                        }
                    }
                }
            }
        }



        [Command("scene", "Manage scenes in your assembly")]
        private static void ManageScene(string command = "", string param = "")
        {
            var man = Core.GetGlobalManager<SceneManager>();
            if (man == null)
            {
                DebugConsole.Instance.Log("Scene Manager is not loaded!");
                return;
            }

            var builder = new StringBuilder();

            switch (command)
            {
                case "count":
                    builder.AppendLine("Total scenes: " + man.Scenes.Count.ToString());
                    break;

                case "list":

                    builder.AppendLine("#id: name(usesArgs)");
                    foreach (var el in man.Scenes)
                    {
                        string name = el.Value.Item1.Name;
                        int id = el.Key;
                        bool args = el.Value.Item2;
                        builder.AppendLine("#" + id.ToString() + ": " + name + "(" + args.ToString() + ")");
                    }
                    break;

                case "refresh":
                    man.Rescan(null, param);
                    break;

                case "current":
                    var scne = Core.Scene;
                    var Sargs = scne.GetType().GetAttribute<ManagedScene>();

                    if (Sargs != null)
                    {
                        builder.AppendLine("Managed scene ");
                        builder.AppendLine("#" + Sargs.SceneNumber.ToString() + ": " + scne.GetType().Name + "(" + Sargs.AcceptsArgument.ToString() + ")");
                    }
                    break;

                case "load":
                    int i;
                    if (int.TryParse(param, out i) && man.Scenes.ContainsKey(i))
                        man.SwitchToScene(i.ToString());
                    else
                        builder.AppendLine("Invalid scene number!");
                    break;

                case "void":
                    Core.Scene = new Scene();
                    break;

                case "enable":
                    bool b;
                    if (bool.TryParse(param, out b) && Core.Scene != null)
                        Core.Scene.Enabled = b;
                    else
                         if (Core.Scene != null) builder.AppendLine("Scene is null!"); else builder.AppendLine("Invalid argument!");
                    break;
                default:
                    builder.AppendLine("Manages scenes in your assembly");
                    builder.AppendLine("Usage: scene [command] {parameter}");
                    builder.AppendLine();
                    builder.AppendLine("---Commands---");
                    builder.AppendLine("scene count");
                    builder.AppendLine("Logs amount of scenes in the assembly.");
                    builder.AppendLine();
                    builder.AppendLine("scene list");
                    builder.AppendLine("Lists all scenes");
                    builder.AppendLine();
                    builder.AppendLine("scene current");
                    builder.AppendLine("Return information about the currently loaded scene.");
                    builder.AppendLine();
                    builder.AppendLine("scene load {SceneID}");
                    builder.AppendLine("Loads a scene and sets it as current scene.");
                    builder.AppendLine();
                    builder.AppendLine("scene refresh {root_assembly}");
                    builder.AppendLine("Reload scenes from the detailed root assembly.");
                    builder.AppendLine();
                    builder.AppendLine("scene enable {true/false}");
                    builder.AppendLine("Enables/Disables the current scene.");
                    builder.AppendLine();
                    builder.AppendLine("scene void");
                    builder.AppendLine("Loads an empty scene.");
                    break;
            }


            DebugConsole.Instance.Log(builder.ToString());

        }

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "sc_switch":
                    SwitchToScene((string)message.Body);
                    break;
                default:
                    break;
            }
        }
    }
}