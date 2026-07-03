using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Coursework.GameObjects;
using System.Collections.Generic;

namespace Coursework.Managers
{
    internal class UIManager
    {

        // NOT FINISHED - NEED TO ADD MORE THINGS


        private SpriteFont font;
        private Texture2D white;

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            font = content.Load<SpriteFont>("DefaultFont");
            white = new Texture2D(graphicsDevice, 1, 1);
            white.SetData(new[] { Color.White });
        }

        public void DrawMoney(SpriteBatch spriteBatch, int money)
        {
            spriteBatch.DrawString(font, $"Money: {money}", new Vector2(10, 10), Color.Yellow);
        }

        public void DrawTowerHealth(SpriteBatch spriteBatch, Tower tower, Vector2 pos)
        {
            string s = $"HP: {tower.TowerHealth}";
            spriteBatch.DrawString(font, s, pos, Color.White);
        }

        public void DrawHealthBar(SpriteBatch spriteBatch, Rectangle rect, int current, int max)
        {
            float pct = MathHelper.Clamp((float)current / max, 0f, 1f);
            spriteBatch.Draw(white, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), Color.DarkGray);
            spriteBatch.Draw(white, new Rectangle(rect.X, rect.Y, (int)(rect.Width * pct), rect.Height), Color.LimeGreen);
        }
    }
}
