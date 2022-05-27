using System;
using System.Collections.Generic;
using System.Text;


namespace Nez.Console
{
    /// <summary>
    /// add this attribute to any static method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string Name;
        public string Help;


        public CommandAttribute(string name, string help)
        {
            Name = name;
            Help = help;
        }
    }
}


#if DEBUG
namespace Nez.Console
{
    public partial class DebugConsole
    {


        [Command("exit", "Exits the game.")]
        private static void Exit() => Core.Exit();


        [Command("inspect",
            "Inspects the Entity with the passed in name, or pass in 'pp' or 'postprocessors' to inspect all PostProccessors in the Scene. Pass in no name to close the inspector.")]
        private static void InspectEntity(string entityName = "")
        {
            // clean up no matter what
            if (Instance._runtimeInspector != null)
            {
                Instance._runtimeInspector.Dispose();
                Instance._runtimeInspector = null;
            }

            if (entityName == "pp" || entityName == "postprocessors")
            {
                Instance._runtimeInspector = new RuntimeInspector();
                Instance.IsOpen = false;
            }
            else if (entityName != "")
            {
                var entity = Core.Scene.FindEntity(entityName);
                if (entity == null)
                {
                    Instance.Log("could not find entity named " + entityName);
                    return;
                }

                Instance._runtimeInspector = new RuntimeInspector(entity);
                Instance.IsOpen = false;
            }
        }


        [Command("console", "Sets the scale that the console is rendered. Defaults to 1 and has a max of 5.")]
        private static void Console(string command = "", string param = "")
        {

            var builder = new StringBuilder();
            switch (command)
            {
                case "clear":
                    Instance._drawCommands.Clear();
                    break;
                case "scale":
                    if (param == "show") { builder.AppendLine(string.Format("Console scale: {0}", RenderScale)); break; }
                    float f;
                    if (float.TryParse(param, out f)) RenderScale = Mathf.Clamp(f, 0.2f, 5f); else RenderScale = 1F;
                    break;
                case "close":
                    Instance.IsOpen = false;
                    break;
                default:
                    builder.AppendLine("Controlls settings about the console itself.");
                    builder.AppendLine("Usage: console [command] {parameter}");
                    builder.AppendLine();
                    builder.AppendLine("---Commands---");
                    builder.AppendLine("console clear");
                    builder.AppendLine("Clears the terminal.");
                    builder.AppendLine();
                    builder.AppendLine("console scale {scale/show}");
                    builder.AppendLine("Sets the scale that the console is rendered. Defaults to 1 and has a max of 5.");
                    builder.AppendLine();
                    builder.AppendLine("console close");
                    builder.AppendLine("Closes the terminal.");
                    break;
            }

            DebugConsole.Instance.Log(builder.ToString());

        }

        [Command("time", "Changes parameters of the game's time.")]
        private static void TimeCmd(string command = "", string param = "")
        {

            var builder = new StringBuilder();
            switch (command)
            {
                case "mode":
                    TimeMode t;
                    if (Enum.TryParse<TimeMode>(param, out t)) Time.Mode = t;
                    break;
                case "scale":
                    if (param == "show") { builder.AppendLine(string.Format("Time scale: {0}", Time.TimeScale)); break; }
                    float f;
                    if (float.TryParse(param, out f)) Time.TimeScale = f; else RenderScale = 1F;
                    break;
                case "step":
                    if (param == "show") { builder.AppendLine(string.Format("Time step: {0}", 1 / Time.TargetTimeStep)); break; }
                    if (float.TryParse(param, out f)) Time.TargetTimeStep = 1 / f; else Time.TargetTimeStep = 1 / 60F;
                    break;
                case "analyzer":
                    bool b;
                    if (!bool.TryParse(param, out b)) b = false;
                    if (Core.Scene == null) return;

                    if (b && Core.Scene.Entities.FindEntity("delta_analyzer") == null) Core.Scene.CreateEntity("delta_analyzer").AddComponent<DeltaAnalyzer>();
                    else if (!b) Core.Scene.Entities.FindEntity("delta_analyzer")?.Destroy();
                    DeltaAnalyzer.Active = b;

                    break;
                default:
                    builder.AppendLine("Controlls settings about the console itself.");
                    builder.AppendLine("Usage: console [command] {parameter}");
                    builder.AppendLine();
                    builder.AppendLine("---Commands---");
                    builder.AppendLine("time mode {Unlocked/LockedFramerate/LockedTimestep}");
                    builder.AppendLine("Changes the time mode the game is operating in");
                    builder.AppendLine();
                    builder.AppendLine("time scale {scale/show}");
                    builder.AppendLine("Sets the timescale. Defaults to 1");
                    builder.AppendLine();
                    builder.AppendLine("time analyzer {true/false}");
                    builder.AppendLine("Hides/shows the DeltaAnalyzer");
                    builder.AppendLine();
                    builder.AppendLine("time step {framerate/show}");
                    builder.AppendLine("Sets the target time step. Defaults to 60.");
                    break;
            }

            DebugConsole.Instance.Log(builder.ToString());

        }

        private static ITimer _drawCallTimer;
        [Command("graphics", "Enables access to the game's visual debug functions.")]
        private static void GrphxCmd(string command = "", string param = "")
        {

            var builder = new StringBuilder();
            switch (command)
            {
                case "vsync":
                    if (param == "show") { builder.AppendLine(Screen.SynchronizeWithVerticalRetrace.ToString()); break; }
                    bool b;
                    if (bool.TryParse(param, out b)) Screen.SynchronizeWithVerticalRetrace = b; else Screen.SynchronizeWithVerticalRetrace = true;
                    break;
                case "drawlog":
                    float f = float.TryParse(param, out f) ? f : 1;

                    if (_drawCallTimer != null)
                    {
                        _drawCallTimer.Stop();
                        _drawCallTimer = null;
                        Debug.Log("Draw call logging stopped");
                    }
                    else
                    {
                        _drawCallTimer = Core.Schedule(f, true, timer => { Debug.Log("Draw Calls: {0}", Core.drawCalls); });
                    }
                    break;
                case "framegraph":
                    if (param == "show") { builder.AppendLine(FramerateGraph.isGraphInScene.ToString()); break; }
                    FramerateGraph.isGraphInScene = bool.TryParse(param, out b) ? b : true;
                    break;
                case "renderable-count":
                    if (Core.Scene == null)
                    {
                        Instance.Log("Current Scene is null!");
                        return;
                    }

                    int i = int.TryParse(param, out i) ? i : int.MinValue;

                    if (i != int.MinValue)
                    {
                        Instance.Log("Total renderables with tag [" + i + "] " +
                                                  Core.Scene.RenderableComponents.ComponentsWithRenderLayer(i).Length
                                                      .ToString());
                    }
                    else
                    {
                        Instance.Log("Total renderables: " + Core.Scene.RenderableComponents.Count.ToString());
                    }

                    break;
                case "renderable-log":
                    if (Core.Scene == null)
                    {
                        Instance.Log("Current Scene is null!");
                        return;
                    }

                    i = int.TryParse(param, out i) ? i : int.MinValue;

                    for (int j = 0; j < Core.Scene.RenderableComponents.Count; j++)
                    {
                        var renderable = Core.Scene.RenderableComponents[j];
                        if (i == int.MinValue || renderable.RenderLayer == i)
                            builder.AppendFormat("{0}\n", renderable);
                    }
                    break;
                case "debug":
                    if (param == "show") { builder.AppendLine(string.Format("Debug rendering {0}", Core.DebugRenderEnabled ? "enabled" : "disabled")); break; }
                    b = bool.TryParse(param, out b) ? b : true;

                    Core.DebugRenderEnabled = b;
                    break;
                default:
                    builder.AppendLine("Controlls settings about the console itself.");
                    builder.AppendLine("Usage: graphics [command] {parameter}");
                    builder.AppendLine();
                    builder.AppendLine("---Commands---");
                    builder.AppendLine("graphics vsync {true/false/show}");
                    builder.AppendLine("Enables framerate syncronisation with the vertical blanking. Defaults to true.");
                    builder.AppendLine();
                    builder.AppendLine("graphics drawlog {delay}");
                    builder.AppendLine("Enables/disables logging of draw calls in the standard console. Call once to enable and again to disable. delay is how often they should be logged and defaults to 1s.");
                    builder.AppendLine();
                    builder.AppendLine("graphics framegraph {true/false/show}");
                    builder.AppendLine("Toggles the framerate graph for the current scene.");
                    builder.AppendLine();
                    builder.AppendLine("graphics renderable-count {renderLayer}");
                    builder.AppendLine("Logs amount of Renderables in the Scene. Pass a renderLayer to count only Renderables in that layer");
                    builder.AppendLine();
                    builder.AppendLine("graphics renderable-log {renderLayer}");
                    builder.AppendLine("Logs the Renderables in the Scene. Pass a renderLayer to log only Renderables in that layer");
                    builder.AppendLine();
                    builder.AppendLine("graphics debug {true/false/show}");
                    builder.AppendLine("enables/disables debug rendering.");
                    break;
            }

            Instance.Log(builder.ToString());

        }


        [Command("assets", "Logs all loaded assets. Pass 's' for scene assets or 'g' for global assets")]
        private static void LogLoadedAssets(string whichAssets = "s")
        {
            if (whichAssets == "s")
                Instance.Log(Core.Scene.Content.LogLoadedAssets());
            else if (whichAssets == "g")
                Instance.Log(Core.Content.LogLoadedAssets());
            else
                Instance.Log("Invalid parameter");
        }



        [Command("entity", "Get information about the currently loaded entities")]
        private static void EntityCount(string command = "", string param = "")
        {

            if (Core.Scene == null)
            {
                Instance.Log("Current Scene is null!");
                return;
            }

            var builder = new StringBuilder();

            switch (command)
            {
                case "count":
                    int nr;
                    if (!int.TryParse(param, out nr) || nr < 0)
                    {
                        builder.AppendLine("Total entities: " + Core.Scene.Entities.Count.ToString());
                    }
                    else
                    {
                        builder.AppendLine("Total entities with tag [" + param + "] " +
                                          Core.Scene.FindEntitiesWithTag(nr).Count.ToString());
                    }

                    break;

                case "list":


                    for (int i = 0; i < Core.Scene.Entities.Count; i++)
                        builder.AppendLine(Core.Scene.Entities[i].ToString());

                    break;

                default:
                    builder.AppendLine("Gets information about the currently loaded entities");
                    builder.AppendLine("Usage: entity [command] {parameter}");
                    builder.AppendLine();
                    builder.AppendLine("---Commands---");
                    builder.AppendLine("entity count {tagIndex}");
                    builder.AppendLine("Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag");
                    builder.AppendLine("");
                    builder.AppendLine("entity list");
                    builder.AppendLine("Lists all entities");
                    break;
            }


            Instance.Log(builder.ToString());

        }

        [Command("physics", "Logs the total Collider count in the spatial hash")]
        private static void Physics(float secondsToDisplay = 5f)
        {
            // store off the current state so we can reset it when we are done
            bool debugRenderState = Core.DebugRenderEnabled;
            Core.DebugRenderEnabled = true;

            float ticker = 0f;
            Core.Schedule(0f, true, null, timer =>
            {
                Nez.Physics.DebugDraw(0f);
                ticker += Time.DeltaTime;
                if (ticker >= secondsToDisplay)
                {
                    timer.Stop();
                    Core.DebugRenderEnabled = debugRenderState;
                }
            });

            Instance.Log("Physics system collider count: " +
                                      ((HashSet<Collider>)Nez.Physics.GetAllColliders()).Count);
        }


        [Command("help", "Shows usage help for a given command")]
        private static void Help(string command)
        {
            if (command == "command")
            {
                Instance.Log("Not literally, ye cheekey bastard!");
                return;
            }

            if (Instance._sorted.Contains(command))
            {
                var c = Instance._commands[command];
                var str = new StringBuilder();

                //Title
                str.Append(":: ");
                str.Append(command);

                //Usage
                if (!string.IsNullOrEmpty(c.Usage))
                {
                    str.Append(" ");
                    str.Append(c.Usage);
                }

                Instance.Log(str.ToString());

                //Help
                if (string.IsNullOrEmpty(c.Help))
                    Instance.Log("No help info set");
                else
                    Instance.Log(c.Help);
            }
            else
            {
                var str = new StringBuilder();
                str.Append("Commands list: ");
                str.Append(string.Join(", ", Instance._sorted));
                Instance.Log(str.ToString());
                Instance.Log("Type 'help command' for more info on that command!");
            }
        }
    }
}
#endif