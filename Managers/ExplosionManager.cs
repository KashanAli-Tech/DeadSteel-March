using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Coursework.Managers
{
    internal class ExplosionManager
    {

        // THERE ARE SOME ERRORS IN THIS
        // I NEED TO FIX THEM AFTER FINISHING COURSEWORK DOCUMENTATION - MILESTONES


        public List<Explosion> Explosions = new List<Explosion>();
        private Texture2D explosionTexture;

        public void LoadContent(ContentManager content)
        {
            explosionTexture = content.Load<Texture2D>("ExplosionFinal");
        }

        public void AddExplosion(Vector2 pos, int damage = 30, float radius = 40f)
        {
            Explosions.Add(new Explosion(explosionTexture, pos, 0.6f, damage, radius));
        }

        public void Update(GameTime gameTime)
        {
            for (int i = Explosions.Count - 1; i >= 0; i--)
            {
                Explosions[i].Update(gameTime);
                if (Explosions[i].IsFinished) Explosions.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var e in Explosions) e.Draw(spriteBatch);
        }
    }
}
