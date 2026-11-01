using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Coursework.Managers
{
    // keeps offers, purchases and bought bonuses together so showing an offer cannot apply its effect
    internal class UpgradeManager
    {
        // costs, percentages and durations are kept together for balancing; percentage bonuses add against original stats
        public float UpgradeRefreshSeconds = 10f;
        public int TowerHealthCost = 100;
        public float TowerHealthPercent = 0.10f;
        public int TowerShieldCost = 75;
        public float TowerShieldSeconds = 10f;
        public int UnitDamageCost = 100;
        public float UnitDamagePercent = 0.05f;
        public int UnitSpeedCost = 75;
        public float UnitSpeedPercent = 0.05f;

        public enum UpgradeType { TowerHealth, TowerShield, UnitDamage, UnitSpeed }

        // towers do not have an attack mechanic here, so the offer list only includes effects the game supports
        private readonly UpgradeType[] availableUpgrades =
        {
            UpgradeType.TowerHealth, UpgradeType.TowerShield,
            UpgradeType.UnitDamage, UpgradeType.UnitSpeed
        };
        // a dictionary records a separate purchase count for each upgrade type
        private readonly Dictionary<UpgradeType, int> purchasedCounts = new Dictionary<UpgradeType, int>();
        // saving each unit's original stats prevents bonuses being multiplied onto already upgraded values
        private readonly Dictionary<BaseUnit, (int Damage, float Speed, int Revision)> originalUnitStats =
            new Dictionary<BaseUnit, (int Damage, float Speed, int Revision)>();
        private readonly Tower playerTower;
        private readonly UnitManager unitManager;
        private readonly MoneyManager moneyManager;
        private readonly Random random = new Random();
        private readonly int originalTowerMaxHealth;
        private int currentOfferIndex = -1;
        private int unitUpgradeRevision;
        private double offerElapsedSeconds;
        private double shieldSecondsRemaining;
        private float towerHealthBonus;
        private float unitDamageBonus;
        private float unitSpeedBonus;
        private Texture2D buttonTexture;
        private Texture2D upgradeIcon;
        private SpriteFont font;

        public UpgradeType CurrentOffer => availableUpgrades[currentOfferIndex];
        public int CurrentOfferCost => GetCost(CurrentOffer);
        public string CurrentOfferName => GetUpgradeName(CurrentOffer);
        public int TotalPurchases
        {
            get
            {
                int total = 0;
                foreach (int count in purchasedCounts.Values) total += count;
                return total;
            }
        }

        // storing the player's tower and original maximum keeps these upgrades separate from the enemy tower
        public UpgradeManager(Tower playerTower, UnitManager unitManager, MoneyManager moneyManager)
        {
            this.playerTower = playerTower;
            this.unitManager = unitManager;
            this.moneyManager = moneyManager;
            originalTowerMaxHealth = playerTower.MaxTowerHealth;
            // choosing the first offer only changes what is shown, not any tower or unit stats
            RefreshOffer();
        }

        // sharing the existing font and content assets avoids a separate ui system for the shop
        public void LoadContent(ContentManager content, SpriteFont existingFont)
        {
            buttonTexture = content.Load<Texture2D>("Button");
            upgradeIcon = content.Load<Texture2D>("UpgradeIcon");
            font = existingFont;
        }

        // counting down the shield and checking purchases with elapsed game time
        public void Update(GameTime gameTime, KeyboardState keyboard, KeyboardState previousKeyboard)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalSeconds;
            if (shieldSecondsRemaining > 0d)
            {
                shieldSecondsRemaining = Math.Max(0d, shieldSecondsRemaining - elapsed);
                // switching off protection at zero lets normal tower damage resume
                if (shieldSecondsRemaining == 0d)
                    playerTower.IsShielded = false;
            }

            // checking for a fresh r press prevents repeated spending while held
            // buying before refreshing means the player buys the offer they actually saw
            bool purchased = keyboard.IsKeyDown(Keys.R) && previousKeyboard.IsKeyUp(Keys.R)
                && TryPurchaseCurrentOffer();
            if (!purchased)
            {
                offerElapsedSeconds += elapsed;
                if (offerElapsedSeconds >= Math.Max(0.01f, UpgradeRefreshSeconds))
                    RefreshOffer();
            }

            // also checking newly spawned player units before they move or fire
            ApplyUnitUpgrades();
        }

        // checking funds before recording or applying anything makes a failed purchase leave the match unchanged
        public bool TryPurchaseCurrentOffer()
        {
            int cost = CurrentOfferCost;
            if (playerTower.IsDead || cost < 0 || !moneyManager.Spend(cost))
                return false;

            UpgradeType purchased = CurrentOffer;
            purchasedCounts[purchased] = GetPurchaseCount(purchased) + 1;
            switch (purchased)
            {
                case UpgradeType.TowerHealth:
                    towerHealthBonus += TowerHealthPercent;
                    // calculating the new maximum from the original keeps repeated health bonuses additive
                    int newMaxHealth = (int)Math.Round(originalTowerMaxHealth * (1d + towerHealthBonus), MidpointRounding.AwayFromZero);
                    playerTower.IncreaseMaxHealth(newMaxHealth - playerTower.MaxTowerHealth);
                    break;
                case UpgradeType.TowerShield:
                    // buying another shield renews its duration, while the purchase count is kept after it expires
                    shieldSecondsRemaining = Math.Max(0f, TowerShieldSeconds);
                    playerTower.IsShielded = shieldSecondsRemaining > 0d;
                    break;
                case UpgradeType.UnitDamage:
                    unitDamageBonus += UnitDamagePercent;
                    unitUpgradeRevision++;
                    ApplyUnitUpgrades();
                    break;
                case UpgradeType.UnitSpeed:
                    unitSpeedBonus += UnitSpeedPercent;
                    unitUpgradeRevision++;
                    ApplyUnitUpgrades();
                    break;
            }

            RefreshOffer();
            return true;
        }

        // selecting from every index except the last one avoids repeated offers without a retry loop
        private void RefreshOffer()
        {
            int next = random.Next(availableUpgrades.Length - (currentOfferIndex >= 0 && availableUpgrades.Length > 1 ? 1 : 0));
            // skipping over the previous index maps the random choice onto the remaining offers
            if (currentOfferIndex >= 0 && availableUpgrades.Length > 1 && next >= currentOfferIndex)
                next++;
            currentOfferIndex = next;
            offerElapsedSeconds = 0d;
        }

        // applying bought bonuses to existing and new player units while keeping enemy units unchanged
        private void ApplyUnitUpgrades()
        {
            if (unitUpgradeRevision == 0)
                // no unit upgrade purchases means no unit stats are changed here
                return;

            // a hash set makes it quick to check which saved units are still alive and present
            var aliveUnits = new HashSet<BaseUnit>(unitManager.GetAlivePlayerUnits());
            foreach (var unit in new List<BaseUnit>(originalUnitStats.Keys))
                if (!aliveUnits.Contains(unit))
                    originalUnitStats.Remove(unit);

            foreach (var unit in aliveUnits)
            {
                if (!unit.IsPlayer)
                    continue;
                if (!originalUnitStats.TryGetValue(unit, out var stats))
                    stats = (unit.Damage, unit.Speed, -1);
                // a matching revision means these bonuses are already applied, so the stats are left alone this frame
                if (stats.Revision == unitUpgradeRevision)
                    continue;

                // recalculating from saved stats only when needed avoids repeated multiplication
                // damage is an integer, so rounding keeps the existing combat system's whole-number damage
                if (unitDamageBonus > 0f)
                    unit.Damage = (int)Math.Round(stats.Damage * (1d + unitDamageBonus), MidpointRounding.AwayFromZero);
                if (unitSpeedBonus > 0f)
                    unit.Speed = stats.Speed * (1f + unitSpeedBonus);
                stats.Revision = unitUpgradeRevision;
                originalUnitStats[unit] = stats;
            }
        }

        // treating an upgrade missing from the dictionary as unbought keeps the initial counts at zero
        public int GetPurchaseCount(UpgradeType upgrade) => purchasedCounts.TryGetValue(upgrade, out int count) ? count : 0;

        // reading the cost for the current type keeps the displayed price and purchase charge consistent
        private int GetCost(UpgradeType upgrade) => upgrade switch
        {
            UpgradeType.TowerHealth => TowerHealthCost,
            UpgradeType.TowerShield => TowerShieldCost,
            UpgradeType.UnitDamage => UnitDamageCost,
            UpgradeType.UnitSpeed => UnitSpeedCost,
            _ => 0
        };

        // building offer names from the settings keeps the displayed percentage or duration up to date
        private string GetUpgradeName(UpgradeType upgrade) => upgrade switch
        {
            UpgradeType.TowerHealth => $"Tower Health +{TowerHealthPercent * 100f:0.#}%",
            UpgradeType.TowerShield => $"Tower Shield {TowerShieldSeconds:0.#}s",
            UpgradeType.UnitDamage => $"Unit Damage +{UnitDamagePercent * 100f:0.#}%",
            UpgradeType.UnitSpeed => $"Unit Speed +{UnitSpeedPercent * 100f:0.#}%",
            _ => string.Empty
        };

        // only listing bought upgrades keeps purchase history separate from the current random offer
        public IEnumerable<string> GetPurchasedLabels()
        {
            foreach (var upgrade in availableUpgrades)
            {
                int count = GetPurchaseCount(upgrade);
                if (count == 0)
                    continue;
                yield return upgrade switch
                {
                    UpgradeType.TowerHealth => $"Tower Health +{towerHealthBonus * 100f:0.#}% (Lv {count})",
                    UpgradeType.TowerShield => $"Tower Shield x{count} ({(shieldSecondsRemaining > 0d ? $"{shieldSecondsRemaining:0.0}s left" : "expired")})",
                    UpgradeType.UnitDamage => $"Unit Damage +{unitDamageBonus * 100f:0.#}% (Lv {count})",
                    UpgradeType.UnitSpeed => $"Unit Speed +{unitSpeedBonus * 100f:0.#}% (Lv {count})",
                    _ => string.Empty
                };
            }
        }

        // drawing the current offer in the top hud and purchase history above the player's tower
        public void Draw(SpriteBatch spriteBatch, Viewport viewport)
        {
            float hudScale = UIManager.GetHudScale(viewport);
            Rectangle button = UIManager.GetUpgradeButtonBounds(viewport);
            spriteBatch.Draw(buttonTexture, button, Color.White);
            int iconSize = (int)(48 * hudScale);
            var icon = new Rectangle(button.X - (int)(12 * hudScale) - iconSize, button.Center.Y - iconSize / 2, iconSize, iconSize);
            spriteBatch.Draw(upgradeIcon, icon, Color.White);
            float textLeft = button.Left + 40 * hudScale;
            float textRight = button.Right - 40 * hudScale;
            float textCenter = (textLeft + textRight) / 2f;
            float textWidth = textRight - textLeft;
            DrawCenteredText(spriteBatch, CurrentOfferName, new Vector2(textCenter, button.Y + 25 * hudScale), 0.8f * hudScale, Color.White, textWidth);
            // the countdown shown on the button is only a display of the existing refresh timer
            double refreshIn = Math.Max(0d, UpgradeRefreshSeconds - offerElapsedSeconds);
            string details = $"R: Buy ${CurrentOfferCost}   |   Refresh {refreshIn:0.0}s";
            DrawCenteredText(spriteBatch, details, new Vector2(textCenter, button.Y + 61 * hudScale), 0.62f * hudScale,
                moneyManager.Money >= CurrentOfferCost ? Color.LightGreen : Color.LightGray, textWidth);

            // spacing the purchased labels above the health bar keeps repeated purchases readable without drawing over the tower
            var labels = new List<string>(GetPurchasedLabels());
            float lineHeight = font.LineSpacing * 0.7f + 3f;
            float y = playerTower.TowerY - 50f - labels.Count * lineHeight;
            foreach (string label in labels)
            {
                float width = font.MeasureString(label).X * 0.7f;
                float x = MathHelper.Clamp(playerTower.Center.X - width / 2f, 10f, Math.Max(10f, viewport.Width - width - 10f));
                DrawShadowedText(spriteBatch, label, new Vector2(x, y), 0.7f, Color.White);
                y += lineHeight;
            }
        }

        // measuring and shrinking wide text keeps it centred inside the button
        private void DrawCenteredText(SpriteBatch spriteBatch, string text, Vector2 position, float scale, Color color, float maxWidth)
        {
            Vector2 size = font.MeasureString(text);
            scale = Math.Min(scale, maxWidth / Math.Max(1f, size.X));
            position.X -= size.X * scale / 2f;
            DrawShadowedText(spriteBatch, text, position, scale, color);
        }

        // drawing a small black offset makes the text readable over the textured background
        private void DrawShadowedText(SpriteBatch spriteBatch, string text, Vector2 position, float scale, Color color)
        {
            spriteBatch.DrawString(font, text, position + new Vector2(1, 1), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
