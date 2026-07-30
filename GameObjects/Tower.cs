using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects
{
    internal class Tower
    {
        public int TowerX, TowerY, TowerWidth, TowerHeight;
        public int TowerHealth;
        public Texture2D texture;

        public Tower(Texture2D newTexture, Rectangle rectangle, int intX, int intY, int width, int height)
        {
            texture = newTexture;
            TowerX = intX;
            TowerY = intY;
            TowerWidth = width;
            TowerHeight = height;
            TowerHealth = 1000;
        }

        public bool IsDead { get; private set; }

        public Rectangle Rectangle
        {
            get { return new Rectangle(TowerX, TowerY, TowerWidth, TowerHeight); }
        }

        public Vector2 Center
        {
            get { return new Vector2(TowerX + TowerWidth / 2f, TowerY + TowerHeight / 2f); }
        }

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


        // not in use as of now
        public void Repair(int amount)
        {
            if (amount >= 1 && amount <= 100)
                TowerHealth += amount;
        }
    }
}
