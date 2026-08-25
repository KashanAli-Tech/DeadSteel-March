using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Coursework.Managers
{
    internal class ExplosionManager
    {
        public List<Explosion> Explosions = new List<Explosion>();
        private Texture2D explosionTexture;

        public void LoadContent(ContentManager content)
        {
            explosionTexture = content.Load<Texture2D>("ExplosionFinal");
        }

        public void AddExplosion(Vector2 pos, int damage = 30, float radius = 40f)
        {
            Explosions.Add(new Explosion(explosionTexture, pos, 0.5f, damage, radius));
        }

        public void Update(GameTime gameTime)
        {
            for (int i = Explosions.Count - 1; i >= 0; i--)
            {
                Explosions[i].Update(gameTime);
                if (Explosions[i].IsFinished)
                    Explosions.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var e in Explosions) e.Draw(spriteBatch);
        }
    }
}

        /*internal class ShellExplosion
        {

            public ShellExplosion(Texture2D newTexture, Vector2 ShellExplosionLocation, bool ExpFinished)
            {
                texture = newTexture;
                ShellExploc = ShellExplosionLocation;
                ExpDone = ExpFinished;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
            }

            public void Update(GameTime gameTime)
            {
                elapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                destRect = new Rectangle((int)ShellExploc.X - 64, (int)ShellExploc.Y - 57, 128, 115);

                if (elapsed < delay)
                {
                    if (frames >= 500)
                    {
                        frames = 0;
                        ExpDone = true;
                    }
                    else
                    {
                        frames++;
                    }
                    elapsed = 0;
                    sourceRect = new Rectangle(127 * frames, 0, 128, 115);
                }
            }


        }*/


