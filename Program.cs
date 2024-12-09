using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        
        public readonly MyIni Ini = new MyIni();
        private ConfigHandler PBConfigHandler;

        private List<IMyProjector> Projectors = new List<IMyProjector>();
        private List<ConfigHandler> ProjectorConfigs = new List<ConfigHandler>();
        private List<double> ProjectorUpdateTimes = new List<double>();

        private readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>
        {
            { "Tag", "BPA" }
        };

        private readonly Dictionary<string, string> DefaultProjectorSettings = new Dictionary<string, string>
        {
            { "repeat", "-1" },
            { "VerticalOffset", "0" },
            { "HorizontalOffset", "0" },
            { "ForwardOffset", "0" }
        };

        private string Tag;

        private readonly DateTime Epoch = new DateTime(1970, 1, 1);

        private double GetNow()
        {
            return (double)(DateTime.Now - Epoch).TotalSeconds;
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            PBConfigHandler = new ConfigHandler(Ini, "Breads Projection Advancer", Me.CustomData, DefaultSettings);
            Update();
        }

        public void Update()
        {
            PBConfigHandler.Load(Me.CustomData);
            Me.CustomData = PBConfigHandler.Save(Me.CustomData);
            Tag = PBConfigHandler.GetSetting("Tag");

            Projectors.Clear();
            GridTerminalSystem.GetBlocksOfType(Projectors, projector => projector.CustomData.Contains(Tag));
            Projectors.ForEach(projector => projector.SetValue("KeepProjection", true));
            ProjectorConfigs.Clear();
            for (int i = 0; i < Projectors.Count; i++)
            {
                ProjectorConfigs.Add(new ConfigHandler(Ini, Tag, Projectors[i].CustomData, DefaultProjectorSettings));
                Projectors[i].CustomData = ProjectorConfigs[i].Save(Projectors[i].CustomData);
            }
            ProjectorUpdateTimes = new List<double>(Projectors.Count);
            for (int i = 0; i < Projectors.Count; i++)
            {
                ProjectorUpdateTimes.Add(0);
            }

            string message = $@"[Color=#FFFFD700]Bread's Projection Advancer[/Color]
            Managed Projectors: [Color=#FF00FF00]{Projectors.Count}[/Color]
            Add Projectors by adding
            [Color=#FF00FFFF][[{Tag}]][/Color]
            to the Custom Data of a Projector
            Then run the script with the argument
            [Color=#FF00FFFF]update[/Color]";
            Echo(message);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == "update")
            {
                Update();
            }
            
            double now = GetNow();

            for (int i = 0; i < Projectors.Count; i++)
            {
                IMyProjector projector = Projectors[i];
                if (projector.Enabled == false)
                {
                    continue;
                }
                if (ProjectorUpdateTimes[i] > now)
                {
                    continue;
                }

                ConfigHandler config = ProjectorConfigs[i];

                bool projectionDone = projector.RemainingBlocks == 0;
                int repeat = int.Parse(config.GetSetting("repeat"));
                if (repeat == 0)
                {
                    projector.Enabled = false;
                    continue;
                }
                if (projectionDone)
                {   
                    int v = int.Parse(config.GetSetting("VerticalOffset"));
                    int h = int.Parse(config.GetSetting("HorizontalOffset"));
                    int f = int.Parse(config.GetSetting("ForwardOffset"));
                    Vector3I offset = new Vector3I(h, v, f);
                    projector.ProjectionOffset += offset;
                    projector.UpdateOffsetAndRotation();

                    if (repeat > 0)
                    {
                        repeat -= 1;
                        config.SetSetting("repeat", repeat.ToString());
                        if (repeat == 0)
                        {
                            projector.Enabled = false;
                        }

                        projector.CustomData = config.Save(projector.CustomData);
                    }

                    ProjectorUpdateTimes[i] = now + 3;
                } else {
                    ProjectorUpdateTimes[i] = now + 5;
                }
            }
        }
    }
}
