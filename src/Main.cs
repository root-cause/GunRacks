using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using Newtonsoft.Json;

namespace GunRack
{
    public class Main : Script
    {
        // edit meta.xml instead
        public static string RackDir = "RackSaveData";
        public static int RackPrice = 2500;
        public static int RackLimit = 5;
        public static int RackSaveInterval = 120;

        public static List<GunRackEntity> GunRacks = new List<GunRackEntity>();

        public Vector3 XYInFrontOfPoint(Vector3 pos, float angle, float distance)
        {
            angle *= (float)Math.PI / 180;
            pos.X += (distance * (float)Math.Sin(-angle));
            pos.Y += (distance * (float)Math.Cos(-angle));
            return pos;
        }

        public Main()
        {
            API.onResourceStart += GunRack_Init;
            API.onResourceStop += GunRack_Exit;
            API.onClientEventTrigger += GunRack_ClientEvent;
        }

        public void GunRack_Init()
        {
            if (API.hasSetting("rackDir")) RackDir = API.getSetting<string>("rackDir");
            if (API.hasSetting("rackPrice")) RackPrice = API.getSetting<int>("rackPrice");
            if (API.hasSetting("rackLimit")) RackLimit = API.getSetting<int>("rackLimit");
            if (API.hasSetting("rackSaveInterval")) RackSaveInterval = API.getSetting<int>("rackSaveInterval");

            API.consoleOutput("GunRack Loaded");
            API.consoleOutput("-> Rack Dir Name: {0}", RackDir);
            API.consoleOutput("-> Rack Price: ${0:n0}", RackPrice);
            API.consoleOutput("-> Rack Limit: {0}", (RackLimit > 0) ? RackLimit.ToString() : "None");
            API.consoleOutput("-> Rack Save Interval: {0}", TimeSpan.FromSeconds(RackSaveInterval).ToString(@"hh\:mm\:ss"));

            RackDir = API.getResourceFolder() + Path.DirectorySeparatorChar + RackDir + Path.DirectorySeparatorChar;
            if (!Directory.Exists(RackDir)) Directory.CreateDirectory(RackDir);

            foreach (string file in Directory.GetFiles(RackDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                GunRackEntity rackEnt = JsonConvert.DeserializeObject<GunRackEntity>(File.ReadAllText(file));
                GunRacks.Add(rackEnt);

                rackEnt.CreateGuns();
            }

            API.consoleOutput("[Gun Rack] Loaded {0} gun racks.", GunRacks.Count);
        }

        public void GunRack_Exit()
        {
            foreach (GunRackEntity rack in GunRacks)
            {
                rack.Save(true);
                rack.Remove(true);
            }

            GunRacks.Clear();
        }

        public void GunRack_ClientEvent(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "Rack_Interact":
                {
                    if (!player.hasData("RackID")) return;
                    GunRackEntity rack = GunRacks.FirstOrDefault(gr => gr.ID == player.getData("RackID"));

                    if (rack == null)
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not near a gun rack.");
                        return;
                    }

                    if (rack.IsPrivate && !rack.Owner.Equals(player.socialClubName))
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You can't access this gun rack.");
                        return;
                    }

                    player.setData("RackID", rack.ID);
                    player.triggerEvent("Rack_ShowMenu", API.toJson(new { Locked = rack.IsPrivate, Weapons = rack.Weapons }));
                    break;
                }

                case "Rack_SetLock":
                {
                    if (arguments.Length < 1 || !player.hasData("RackID")) return;
                    GunRackEntity rack = GunRacks.FirstOrDefault(gr => gr.ID == player.getData("RackID"));
                    
                    if (rack == null)
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not near a gun rack.");
                        return;
                    }

                    if (!rack.Owner.Equals(player.socialClubName))
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not the owner of this gun rack.");
                        return;
                    }

                    rack.SetPrivacy(Convert.ToBoolean(arguments[0]));

                    player.sendChatMessage(string.Format("~b~[GUN RACK] ~w~This gun rack is now {0}.", (rack.IsPrivate) ? "locked" : "unlocked"));
                    player.triggerEvent("SetLockState", rack.IsPrivate);
                    break;
                }

                case "Rack_Remove":
                {
                    if (!player.hasData("RackID")) return;
                    GunRackEntity rack = GunRacks.FirstOrDefault(gr => gr.ID == player.getData("RackID"));

                    if (rack == null)
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not near a gun rack.");
                        return;
                    }

                    if (!rack.Owner.Equals(player.socialClubName))
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not the owner of this gun rack.");
                        return;
                    }

                    if (rack.Weapons.Count(w => w != null) > 0)
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~Take your guns first.");
                        return;
                    }

                    rack.Remove();
                    break;
                }

                case "Rack_TakeSelected":
                {
                    if (arguments.Length < 1 || !player.hasData("RackID")) return;
                    GunRackEntity rack = GunRacks.FirstOrDefault(gr => gr.ID == player.getData("RackID"));

                    if (rack == null)
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not near a gun rack.");
                        return;
                    }

                    if (rack.IsPrivate && !rack.Owner.Equals(player.socialClubName))
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You can't access this gun rack.");
                        return;
                    }

                    int index = Convert.ToInt32(arguments[0]);
                    TakeGunResult result = rack.TakeGun(player, index);

                    if (result != TakeGunResult.Success)
                    {
                        player.sendChatMessage(string.Format("~b~[GUN RACK] ~w~Operation failed. ~r~({0})", result));
                    }
                    else
                    {
                        player.triggerEvent("Rack_UpdateWeapons", API.toJson(new { Weapons = rack.Weapons }));
                    }

                    break;
                }

                case "Rack_PutToSelected":
                {
                    if (arguments.Length < 1 || !player.hasData("RackID")) return;
                    GunRackEntity rack = GunRacks.FirstOrDefault(gr => gr.ID == player.getData("RackID"));

                    if (rack == null)
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You're not near a gun rack.");
                        return;
                    }

                    if (rack.IsPrivate && !rack.Owner.Equals(player.socialClubName))
                    {
                        player.sendChatMessage("~b~[GUN RACK] ~w~You can't access this gun rack.");
                        return;
                    }

                    int index = Convert.ToInt32(arguments[0]);
                    PutGunResult result = rack.PutGun(player, player.currentWeapon, player.getWeaponAmmo(player.currentWeapon), index);

                    if (result != PutGunResult.Success)
                    {
                        player.sendChatMessage(string.Format("~b~[GUN RACK] ~w~Operation failed. ~r~({0})", result));
                    }
                    else
                    {
                        player.triggerEvent("Rack_UpdateWeapons", API.toJson(new { Weapons = rack.Weapons }));
                    }

                    break;
                }
            }
        }

        [Command("buyrack")]
        public void CMD_BuyRack(Client player)
        {
            if (API.exported.MoneyAPI.GetMoney(player) < RackPrice)
            {
                player.sendChatMessage("~b~[GUN RACK] ~w~You can't afford a gun rack.");
                return;
            }

            if (player.hasData("RackID"))
            {
                player.sendChatMessage("~b~[GUN RACK] ~w~There's a gun rack nearby.");
                return;
            }

            if (RackLimit > 0 && GunRacks.Count(gr => gr.Owner == player.socialClubName) >= RackLimit)
            {
                player.sendChatMessage("~b~[GUN RACK] ~w~You can't buy any more gun racks.");
                return;
            }

            GunRackEntity newRack = new GunRackEntity(Guid.NewGuid(), player.socialClubName, XYInFrontOfPoint(player.position - new Vector3(0f, 0f, 0.65f), player.rotation.Z, 1f), player.rotation.Z, true, true);
            GunRacks.Add(newRack);

            API.exported.MoneyAPI.ChangeMoney(player, -RackPrice);
            player.sendChatMessage(string.Format("~b~[GUN RACK] ~w~Bought a gun rack for ~g~${0:n0}.", RackPrice));
        }
    }
}