﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace ELS_Server
{
    public class Class1 : BaseScript
    {
        Dictionary<int, object> _cachedData = new Dictionary<int, object>();

        public Class1()
        {
            Debug.WriteLine("Welcome to ELS+ for FiveM");
            foreach(string s in Configuration.ElsVehicleGroups)
            {
                API.ExecuteCommand($"add_ace group.{s} command.elscar allow");
            }
            API.RegisterCommand("vcfrefresh", new Action<int, List<object>, string>((source, arguments, raw) =>
            {
                Debug.WriteLine($"{Players[source].Name} has activated a VCF Refresh");
                foreach (Player p in Players)
                {
                    TriggerEvent("ELS:VcfSync:Server", p.Handle);
                }
            }), false);

            API.RegisterCommand("elscar", new Action<int, List<object>, string>(async (source, arguments, raw) =>
            {
                if (arguments.Count < 1 || String.IsNullOrEmpty(arguments[0].ToString()))
                {
                    Utils.ReleaseWriteLine("No vehicle specified please try again");
                    return;
                }
                Utils.ReleaseWriteLine($"{Players[source].Name} has attempted to spawn {arguments[0]}");
                TriggerClientEvent(Players[source], "ELS:SpawnCar", arguments[0]);
            }), Configuration.ElsCarAdminOnly);

            EventHandlers["ELS:VcfSync:Server"] += new Action<int>(async (int source) =>
            {
                Utils.DebugWriteLine($"Sending Data to {Players[source].Name}");
                 

                TriggerClientEvent(Players[source], "ELS:VcfSync:Client", VcfSync.VcfData);
                TriggerClientEvent(Players[source], "ELS:PatternSync:Client", CustomPatterns.Patterns);

            });

            EventHandlers["baseevents:enteredVehicle"] += new Action<int,int,string>((veh,seat,name) =>
            {
                Utils.DebugWriteLine("Vehicle Entered");
                TriggerClientEvent("ELS:VehicleEntered", veh);
            });
            EventHandlers["ELS:FullSync:Unicast"] += new Action(() => { });
            EventHandlers["ELS:FullSync:Broadcast"] += new Action<System.Dynamic.ExpandoObject, Int16>((dataDic, playerID) =>
             {
                 var dd = (IDictionary<string, object>)dataDic;
#if DEBUG
                 Debug.WriteLine($"NetworkID {dd["NetworkID"]}");
#endif
                 _cachedData[int.Parse(dd["NetworkID"].ToString())] = dd;
                 BroadcastMessage(dataDic, playerID);
             });
            EventHandlers["ELS:FullSync:Request:All"] += new Action<int>((int source) =>
            {
#if DEBUG
                Debug.WriteLine($"{source} is requesting Sync Data");
#endif
                TriggerClientEvent(Players[source], "ELS:FullSync:NewSpawnWithData", _cachedData);
            });
            API.RegisterCommand("resync", new Action<int, System.Collections.IList, string>((a, b, c) =>
            {
#if DEBUG
                Debug.WriteLine($"{a}, {b}, {c}");
#endif
                TriggerClientEvent(Players[(int.Parse((string)b[0]))], "ELS:FullSync:NewSpawnWithData", _cachedData);
            }), false);

            PreloadSyncData();
        }

        async Task PreloadSyncData()
        {
            await VcfSync.CheckResources();
            await CustomPatterns.CheckCustomPatterns();
        }

        void BroadcastMessage(System.Dynamic.ExpandoObject dataDic, int SourcePlayerID)
        {
            TriggerClientEvent("ELS:NewFullSyncData", dataDic,SourcePlayerID);
        }
    }
}
