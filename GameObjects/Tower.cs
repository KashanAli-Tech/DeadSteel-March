using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    // keeps a tower's position, health and damage checks together so both sides use the same rules
    internal class Tower
    {
        public int TowerX, TowerY, TowerWidth, TowerHeight;
        public int TowerHealth;
        // other classes can read the maximum, but increases go through the tower's health method
        public int MaxTowerHealth { get; private set; }
        // the upgrade manager controls the shield flag, while the tower checks it before taking damage
        public bool IsShielded { get; internal set; }
        public Texture2D texture;

        // starting at maximum health gives each new tower a full health bar
        public Tower(Texture2D newTexture, Rectangle rectangle, int intX, int intY, int width, int height, int maxHealth = 1000)
        {
            texture = newTexture;
            TowerX = intX;
            TowerY = intY;
            TowerWidth = width;
            TowerHeight = height;
            MaxTowerHealth = System.Math.Max(1, maxHealth);
            TowerHealth = MaxTowerHealth;
        }

        public bool IsDead { get; private set; }

        public Rectangle Rectangle => new Rectangle(TowerX, TowerY, TowerWidth, TowerHeight);

        public Vector2 Center => new Vector2(TowerX + TowerWidth / 2f, TowerY + TowerHeight / 2f);

        // dividing by the current maximum keeps health comparisons correct after a health upgrade
        public float HealthPercentage => MaxTowerHealth <= 0 ? 0f : (float)TowerHealth / MaxTowerHealth;

        // drawing inside the tower's rectangle keeps its displayed size tied to its stored dimensions
        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle(TowerX, TowerY, TowerWidth, TowerHeight), Color.White);
        }

        // checking death and shielding here means every tower hit goes through the same protection rules
        public void TakeDamage(int amount)
        {
            if (IsDead || IsShielded || amount <= 0)
                return;

            TowerHealth -= amount;

            if (TowerHealth <= 0)
            {
                TowerHealth = 0;
                IsDead = true;
            }
        }

        // adding the same amount to current and maximum health keeps existing damage after an upgrade
        public void IncreaseMaxHealth(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            MaxTowerHealth += amount;
            TowerHealth = System.Math.Min(MaxTowerHealth, TowerHealth + amount);
        }

    }
}
