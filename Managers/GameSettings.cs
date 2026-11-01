using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework.GameObjects;
using Coursework.GameObjects.Units;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Coursework.Managers
{
    // preferences live outside a match, so retry can reset gameplay without losing the player's choices
    internal class GameSettings
    {
        public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeadSteelMarch", "settings.json");
        private static readonly JsonSerializerOptions json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
            Converters = { new JsonStringEnumConverter() }
        };
        public static GameSettings Current { get; private set; } = Load(DefaultPath);
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public float EffectsVolume { get; set; } = 1f;
        public bool Muted { get; set; }
        public AiDifficulty Difficulty { get; set; } = AiDifficulty.Normal;
        public MatchConfig Match { get; set; } = new MatchConfig();
        [JsonIgnore] public string SaveStatus { get; private set; } = "Using default preferences";

        // missing or damaged preferences fall back to playable defaults rather than preventing startup
        public static GameSettings Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return new GameSettings();
                var settings = JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(path), json) ?? new GameSettings();
                settings.Validate();
                settings.SaveStatus = "Preferences loaded";
                return settings;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is JsonException || error is ArgumentException || error is NotSupportedException)
            {
                return new GameSettings { SaveStatus = "Could not read preferences - using defaults" };
            }
        }

        // saving beside the preferences file first avoids leaving half a json file if writing fails
        public bool Save(string path = null)
        {
            try
            {
                Validate();
                path = Path.GetFullPath(path ?? DefaultPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(this, json));
                File.Move(path + ".tmp", path, true);
                SaveStatus = "Preferences saved";
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is JsonException || error is ArgumentException || error is NotSupportedException)
            {
                SaveStatus = "Could not save preferences - changes still work for this session";
                return false;
            }
        }

        // a separate copy keeps later settings edits from changing this match or its future spawns
        public MatchConfig CreateMatchConfig()
        {
            Validate();
            return Match.Copy();
        }

        public void Validate()
        {
            MasterVolume = Limit(MasterVolume, 0, 1, 1);
            MusicVolume = Limit(MusicVolume, 0, 1, 1);
            EffectsVolume = Limit(EffectsVolume, 0, 1, 1);
            if (!Enum.IsDefined(typeof(AiDifficulty), Difficulty)) Difficulty = AiDifficulty.Normal;
            Match ??= new MatchConfig();
            Match.Validate();
        }

        // both existing songs and sound effects use these shared volume controls
        public void ApplyAudio()
        {
            Validate();
            MediaPlayer.Volume = Muted ? 0 : MasterVolume * MusicVolume;
            SoundEffect.MasterVolume = Muted ? 0 : MasterVolume * EffectsVolume;
        }

        internal static float Limit(float value, float minimum, float maximum, float fallback) =>
            float.IsFinite(value) ? Math.Clamp(value, minimum, maximum) : fallback;
    }

    // these defaults match the existing constructors, so a new preferences file does not rebalance the game
    internal class MatchConfig
    {
        public UnitConfig Soldier { get; set; } = new UnitConfig { Health = 100, Damage = 10, Speed = 2f };
        public UnitConfig MachineGunner { get; set; } = new UnitConfig { Health = 120, Damage = 15, Speed = 1.8f };
        public UnitConfig Tank { get; set; } = new UnitConfig { Health = 300, Damage = 40, Speed = 1.2f };
        public int PlayerTowerHealth { get; set; } = 1000;
        public int EnemyTowerHealth { get; set; } = 1000;
        public int StartingMoney { get; set; } = 500;
        public int SoldierCost { get; set; } = 10;
        public int MachineGunnerCost { get; set; } = 25;
        public int TankCost { get; set; } = 50;
        public float UpgradeRefreshSeconds { get; set; } = 10f;

        public void Validate()
        {
            Soldier ??= new UnitConfig { Health = 100, Damage = 10, Speed = 2f };
            MachineGunner ??= new UnitConfig { Health = 120, Damage = 15, Speed = 1.8f };
            Tank ??= new UnitConfig { Health = 300, Damage = 40, Speed = 1.2f };
            Soldier.Validate(2f); MachineGunner.Validate(1.8f); Tank.Validate(1.2f);
            PlayerTowerHealth = Math.Clamp(PlayerTowerHealth, 100, 100000);
            EnemyTowerHealth = Math.Clamp(EnemyTowerHealth, 100, 100000);
            StartingMoney = Math.Clamp(StartingMoney, 0, 100000);
            SoldierCost = Math.Clamp(SoldierCost, 0, 10000);
            MachineGunnerCost = Math.Clamp(MachineGunnerCost, 0, 10000);
            TankCost = Math.Clamp(TankCost, 0, 10000);
            UpgradeRefreshSeconds = GameSettings.Limit(UpgradeRefreshSeconds, 1, 120, 10);
        }

        public MatchConfig Copy()
        {
            var copy = (MatchConfig)MemberwiseClone();
            copy.Soldier = Soldier.Copy(); copy.MachineGunner = MachineGunner.Copy(); copy.Tank = Tank.Copy();
            return copy;
        }

        // applying base stats once at spawn leaves movement, attack timing and collision sizes alone
        public void ApplyTo(BaseUnit unit)
        {
            UnitConfig stats = unit is Soldier ? Soldier : unit is MachineGunner ? MachineGunner : unit is Tank ? Tank : null;
            if (stats == null) return;
            unit.Health = unit.MaxHealth = stats.Health;
            unit.Damage = stats.Damage;
            unit.Speed = stats.Speed;
        }
    }

    // grouping one unit's base values makes it easier to copy them safely for a new match
    internal class UnitConfig
    {
        public int Health { get; set; }
        public int Damage { get; set; }
        public float Speed { get; set; }
        public UnitConfig Copy() => (UnitConfig)MemberwiseClone();
        public void Validate(float defaultSpeed)
        {
            Health = Math.Clamp(Health, 1, 10000);
            Damage = Math.Clamp(Damage, 1, 1000);
            Speed = GameSettings.Limit(Speed, .1f, 10f, defaultSpeed);
        }
    }
}
