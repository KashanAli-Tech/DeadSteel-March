using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Coursework.Managers
{
    // manages a changing list of explosions so several effects can exist at once without separate fields
    internal class ExplosionManager
    {
        public List<Explosion> Explosions = new List<Explosion>();
        private Texture2D explosionTexture;

        // sharing one sprite sheet avoids loading the animation again for every explosion
        public void LoadContent(ContentManager content)
        {
            explosionTexture = content.Load<Texture2D>("ExplosionFinal");
        }

        // passing damage and radius into each effect allows both damaging blasts and visual-only explosions
        public void AddExplosion(Vector2 pos, int damage = 30, float radius = 20f)
        {
            Explosions.Add(new Explosion(explosionTexture, pos, 0.5f, damage, radius));
        }

        // removing finished animations backwards keeps the remaining list entries from being skipped
        public void Update(GameTime gameTime)
        {
            for (int i = Explosions.Count - 1; i >= 0; i--)
            {
                Explosions[i].Update(gameTime);
                if (Explosions[i].IsFinished)
                    Explosions.RemoveAt(i);
            }
        }

        // letting each explosion choose its own animation frame keeps frame calculations out of the manager
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var e in Explosions) e.Draw(spriteBatch);
        }
    }
}


