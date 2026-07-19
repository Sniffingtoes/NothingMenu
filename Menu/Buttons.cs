using Nothing.Classes;
using Nothing.Mods;
using Nothing.Notifications;
using NothingMenu.Menu;
using NothingMenu.Mods;
using Oculus.Interaction;
using UnityEngine;
using static Nothing.Menu.GunTemplate;
using static Nothing.Menu.Main;
using static Nothing.Menu.SaveData;
using static Nothing.Mods.Movement;
using static Nothing.Notifications.NotifiLib;
using static Nothing.Settings;
using static NothingMenu.Mods.Fun;

namespace Nothing.Menu
{
    public partial class Buttons
    {
        public static ButtonInfo[][] buttons;

        static Buttons()
        {
            ForceInitializeButtons();
        }

        public static void Init() => ForceInitializeButtons();

        public static void ForceInitializeButtons()
        {
            if (buttons != null) return;

            buttons = new ButtonInfo[][]
            {
                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Settings", method =() => currentCategory = 1, isTogglable = false},
                    new ButtonInfo { buttonText = "Enabled", method =() => { EnabledMods.RefreshEnabledTab(); currentCategory = 2; }, isTogglable = false},
                    new ButtonInfo { buttonText = "<color=yellow>Favorite</color>", method =() => currentCategory = 3, isTogglable = false},
                    new ButtonInfo { buttonText = "Movement", method =() => currentCategory = 4, isTogglable = false},
                    new ButtonInfo { buttonText = "Visuals", method =() => currentCategory = 5, isTogglable = false},
                    new ButtonInfo { buttonText = "Fun", method =() => currentCategory = 6, isTogglable = false},
                    new ButtonInfo { buttonText = "Usefull", method =() => currentCategory = 7, isTogglable = false},
                    new ButtonInfo { buttonText = "Soundboard", method = () => { SoundboardHandler.RefreshSoundboardButtons(); currentCategory = 8; }, isTogglable = false },
                    new ButtonInfo { buttonText = "Player", method =() => currentCategory = 9, isTogglable = false},
                    new ButtonInfo { buttonText = "Beta", method =() => currentCategory = 10, isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                    new ButtonInfo { buttonText = "Auto Save", method =() => SaveSystem.AutoSave(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Save Settings", method =() => SaveSystem.Save(), toolTip = "", isTogglable = false},
                    ValueButton("Theme", ThemeManager.GetCurrentThemeName, CycleTheme),
                    ValueButton("Click Sound", Settings.GetClickSound, CycleClickSound),
                    new ButtonInfo { buttonText = "Disconnect [RT]", method =() => Settingss.RTDisconnect(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Equip Gun", method =() => GunTemplate.GunTest(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Disable Notifications", method = () => NotifiLib.SetEnabled(), toolTip = "", isTogglable = true },
                    new ButtonInfo { buttonText = "Disable PC Notifications", method = () => NotifiLib.SetGuiEnabled(), toolTip = "", isTogglable = true },
                    new ButtonInfo { buttonText = "Disable PC Array List", enableMethod = () => { Settings.pcArrayList = false; SaveSystem.Save(); }, disableMethod = () => { Settings.pcArrayList = true; SaveSystem.Save(); }, toolTip = "", isTogglable = true },
                    new ButtonInfo { buttonText = "Disable PC Watermark", enableMethod = () => Settings.pcWatermark = false, disableMethod = () => Settings.pcWatermark = true, enabled = !Settings.pcWatermark, toolTip = "", isTogglable = true },
                    new ButtonInfo { buttonText = "Disable PC Room Joiner", enableMethod = () => Settings.pcRoomJoiner = false, disableMethod = () => Settings.pcRoomJoiner = true, enabled = !Settings.pcRoomJoiner, toolTip = "", isTogglable = true },
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                },
                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                    new ButtonInfo { buttonText = "Platforms", method =() => Movement.Platforms(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Check Point", method =() => Movement.Checkpoint(), toolTip = "", isTogglable = true},
                    ValueButton("Fly Speed", () => FlySettings.labels[FlySettings.index], CycleFlySpeed),
                    new ButtonInfo { buttonText = "Fly [B]", method =() => Movement.Fly(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Noclip Fly [B]", method =() => Movement.NoclipFly(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Trigger Noclip Fly [RT]", method =() => Movement.NoclipFlyTrigger(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Trigger Fly [RT]", method =() => Movement.TriggerFly(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Hand Fly [B]", method =() => Movement.HandFly(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Joystick Fly", method =() => Movement.JoystickFly(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Slingshot Fly [B]", method =() => Movement.SlingshotFly(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "WASD Fly", method =() => Movement.WASD(), toolTip = "", isTogglable = true},
                    ValueButton("Speed Boost", () => BoostSettings.labels[BoostSettings.index], CycleSpeedBoost),
                    new ButtonInfo { buttonText = "Speed Boost", method =() => Movement.SpeedBoost(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Grip Speed Boost", method =() => Movement.GripSpeedBoost(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Slide Control", method =() => Movement.SlideControl(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "TP Gun", method =() => Movement.TeleportGun(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "No Tag Freeze", method =() => Movement.NoTagFreeze(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Force Tag Freeze", method =() => Movement.ForceTagFreeze(), toolTip = "", isTogglable = true},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                    new ButtonInfo { buttonText = "Tracers", method =() => Visuals.Tracer(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Box ESP", method =() => Visuals.BoxESP(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "2D Box ESP", method =() => Visuals.Box2DESP(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Bone ESP", method =() => Visuals.BoneESP(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Beacon ESP", method =() => Visuals.Beacons(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Nametags", method =() => Visuals.AdvNametags(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Fake Unban", method =() => Visuals.FakeUnbanSelf(), toolTip = "", isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                    new ButtonInfo { buttonText = "Frozone", method =() => Fun.Frozone(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Dash [RG]", method =() => Fun.Dash(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Schizophrenia", method =() => Fun.Scitzo(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Fuck You", method =() => Fun.FuckYou(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Grab Bat [RG] [LG]", method =() => Fun.GrabBat(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Grab Bug [RG] [LG]", method =() => Fun.GrabBug(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Draw [RG]", method =() => Fun.Draw(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Chase Gun", method =() => Fun.ChaseGun(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Max Quest Score [SS]", method =() => Fun.MaxQuestScore(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Hover Board [SS]", method =() => Fun.HoverBoard(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Unlock All Gadgets", method =() => Fun.UnlockAllGadgets(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Fix Monke", method =() => Fun.FixMonke(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Long Head", method =() => Fun.TallHead(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "short Head", method =() => Fun.ShortHead(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "2D Monke", method =() => Fun._2DMonke(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Tall Monke", method =() => Fun.TallMonke(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Short Monke", method =() => Fun.ShortMonke(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Big Monke", method =() => Fun.BigMonke(), toolTip = "", isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                    new ButtonInfo { buttonText = "Anti Report", method =() => Usefull.AntiReportLogic(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "No Finger Movement", method =() => Usefull.NoFingerMovement(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Tp To Stump", method =() => Usefull.TPStump(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Tp To Stump [RT]", method =() => Usefull.TPStumpRT(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Quit App", method =() => Usefull.QuitGTAG(), toolTip = "", isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                    new ButtonInfo { buttonText = "Ghost [A]", method =() => Player.Ghostmonke(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Invis [B]", method =() => Player.invis(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "No Clip [RT]", method =() => Player.NoClip(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Wall Walk [RG]", method =() => Player.WallWalk(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Upside Down Head", method =() => Player.UpsideDownHead(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Backwards Head", method =() => Player.BackwardsHead(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Sideways Head", method =() => Player.SidewaysHead(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Broken Neck", method =() => Player.BrokenNeck(), toolTip = "", isTogglable = false},
                    new ButtonInfo { buttonText = "Grab Rig [RG]", method =() => Player.GrabRig(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Fake Lag Self [RG]", method =() => Player.FakeLag(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "T Pose [RG]", method =() => Player.Tpose(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Spaz Hands", method =() => Player.SpazHands(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Spaz", method =() => Player.Spaz(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Spin [RG]", method =() => Player.Spin(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Ascent [RG]", method =() => Player.Ascend(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Helicopter [RG]", method =() => Player.Helicopter(), toolTip = "", isTogglable = true},
                    new ButtonInfo { buttonText = "Freeze Rig [RG]", method =() => Player.FreezeRig(), toolTip = "", isTogglable = true},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                },

                new ButtonInfo[] {
                    new ButtonInfo { buttonText = "Return to Main", method =() => currentCategory = 0, isTogglable = false},
                },
            };
        }

        public static void RefreshEnabledTab() => EnabledMods.RefreshEnabledTab();
    }
}
