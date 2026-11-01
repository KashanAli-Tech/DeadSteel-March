using System;
using System.Collections.Generic;
using Coursework.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Coursework.UI
{
    // small plus and minus controls avoid a separate text-input system for the settings
    internal class SettingsMenu
    {
        private readonly MenuScreens menus;
        private readonly List<(string Label, Func<string> Value, Action<int> Change)>[] pages;
        private readonly string settingsPath;
        private int page;

        public SettingsMenu(MenuScreens menus, string settingsPath = null)
        {
            this.menus = menus;
            this.settingsPath = settingsPath;
            GameSettings settings = GameSettings.Current;
            var audio = new List<(string, Func<string>, Action<int>)>
            {
                ("MASTER VOLUME", () => $"{settings.MasterVolume * 100:0}%", d => settings.MasterVolume += d * .05f),
                ("MUSIC VOLUME", () => $"{settings.MusicVolume * 100:0}%", d => settings.MusicVolume += d * .05f),
                ("SOUND EFFECTS", () => $"{settings.EffectsVolume * 100:0}%", d => settings.EffectsVolume += d * .05f),
                ("MUTE AUDIO", () => settings.Muted ? "MUTED" : "ON", d => settings.Muted = !settings.Muted),
                ("NEXT MISSION DIFFICULTY", () => settings.Difficulty.ToString().ToUpperInvariant(), d =>
                    settings.Difficulty = (AiDifficulty)(((int)settings.Difficulty + d + 3) % 3))
            };
            var units = new List<(string, Func<string>, Action<int>)>();
            AddUnitRows(units, "SOLDIER", settings.Match.Soldier);
            AddUnitRows(units, "MACHINE GUNNER", settings.Match.MachineGunner);
            AddUnitRows(units, "TANK", settings.Match.Tank);
            MatchConfig match = settings.Match;
            var rules = new List<(string, Func<string>, Action<int>)>
            {
                ("PLAYER TOWER HP (100-100000)", () => match.PlayerTowerHealth.ToString(), d => match.PlayerTowerHealth += d * 100),
                ("ENEMY TOWER HP (100-100000)", () => match.EnemyTowerHealth.ToString(), d => match.EnemyTowerHealth += d * 100),
                ("STARTING MONEY (0-100000)", () => $"${match.StartingMoney}", d => match.StartingMoney += d * 50),
                ("SOLDIER COST (0-10000)", () => $"${match.SoldierCost}", d => match.SoldierCost += d * 5),
                ("MACHINE GUNNER COST (0-10000)", () => $"${match.MachineGunnerCost}", d => match.MachineGunnerCost += d * 5),
                ("TANK COST (0-10000)", () => $"${match.TankCost}", d => match.TankCost += d * 5),
                ("UPGRADE REFRESH (1-120 SECONDS)", () => $"{match.UpgradeRefreshSeconds:0}s", d => match.UpgradeRefreshSeconds += d)
            };
            pages = new[] { audio, units, rules };
        }

        private static void AddUnitRows(List<(string, Func<string>, Action<int>)> rows, string name, UnitConfig stats)
        {
            rows.Add(($"{name} HEALTH (1-10000)", () => stats.Health.ToString(), d => stats.Health += d * 10));
            rows.Add(($"{name} DAMAGE (1-1000)", () => stats.Damage.ToString(), d => stats.Damage += d));
            rows.Add(($"{name} SPEED (0.1-10)", () => $"{stats.Speed:0.0}", d => stats.Speed += d * .1f));
        }

        // changes to audio are immediate, while the selected difficulty is read only by a new match
        public bool Update(MouseState mouse, MouseState previous)
        {
            if (menus.Clicked(new Rectangle(760, 940, 400, 100), mouse, previous)) return true;
            for (int i = 0; i < pages.Length; i++)
                if (menus.Clicked(new Rectangle(320 + i * 440, 210, 400, 55), mouse, previous)) page = i;
            var rows = pages[page];
            for (int i = 0; i < rows.Count; i++)
            {
                int direction = menus.Clicked(new Rectangle(1150, 300 + i * 64, 90, 54), mouse, previous) ? -1
                    : menus.Clicked(new Rectangle(1380, 300 + i * 64, 90, 54), mouse, previous) ? 1 : 0;
                if (direction == 0) continue;
                rows[i].Change(direction);
                GameSettings.Current.ApplyAudio();
                GameSettings.Current.Save(settingsPath);
            }
            return false;
        }

        public void Draw(SpriteBatch batch, MouseState mouse)
        {
            menus.Dim(batch, .9f);
            menus.Text(batch, "SETTINGS", 960, 95, 1.9f, new Color(155, 185, 135), true);
            menus.Text(batch, "Audio changes now - difficulty and base stats apply to new missions", 960, 165, .8f, Color.LightGray, true);
            string[] tabs = { "AUDIO / DIFFICULTY", "UNIT STATS", "MATCH RULES" };
            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == page) menus.Panel(batch, new Rectangle(316 + i * 440, 206, 408, 63), Color.Black, new Color(155, 185, 135));
                menus.Button(batch, new Rectangle(320 + i * 440, 210, 400, 55), tabs[i], mouse);
            }
            menus.Panel(batch, new Rectangle(300, 275, 1320, 610), new Color(10, 17, 20, 240), new Color(110, 135, 100));
            var rows = pages[page];
            for (int i = 0; i < rows.Count; i++)
            {
                int y = 300 + i * 64;
                menus.Text(batch, rows[i].Label, 380, y + 12, .7f, Color.White, false, 710);
                menus.Button(batch, new Rectangle(1150, y, 90, 54), "-", mouse);
                menus.Text(batch, rows[i].Value(), 1310, y + 12, .8f, Color.White, true, 125);
                menus.Button(batch, new Rectangle(1380, y, 90, 54), "+", mouse);
            }
            menus.Text(batch, GameSettings.Current.SaveStatus, 960, 900, .65f, Color.LightGray, true, 1500);
            menus.Button(batch, new Rectangle(760, 940, 400, 100), "BACK", mouse);
        }
    }
}
