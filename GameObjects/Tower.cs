using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Tower
    {
        public int TowerX, TowerY, TowerWidth, TowerHeight;
        public int TowerHealth;
        public int MaxTowerHealth { get; private set; }
        public Texture2D texture;

        public Tower(Texture2D newTexture, Rectangle rectangle, int intX, int intY, int width, int height)
        {
            texture = newTexture;
            TowerX = intX;
            TowerY = intY;
            TowerWidth = width;
            TowerHeight = height;
            MaxTowerHealth = 1000;
            TowerHealth = MaxTowerHealth;
        }

        public bool IsDead { get; private set; }

        public Rectangle Rectangle => new Rectangle(TowerX, TowerY, TowerWidth, TowerHeight);

        public Vector2 Center => new Vector2(TowerX + TowerWidth / 2f, TowerY + TowerHeight / 2f);

        // Handy for a HealthBar UI element (0f..1f).
        public float HealthPercentage => MaxTowerHealth <= 0 ? 0f : (float)TowerHealth / MaxTowerHealth;

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle(TowerX, TowerY, TowerWidth, TowerHeight), Color.White);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead)
                return;

            TowerHealth -= amount;

            if (TowerHealth <= 0)
            {
                TowerHealth = 0;
                IsDead = true;
            }
        }

        public void Repair(int amount)
        {
            if (amount >= 1 && amount <= 100)
                TowerHealth = System.Math.Min(MaxTowerHealth, TowerHealth + amount);
        }
    }
}