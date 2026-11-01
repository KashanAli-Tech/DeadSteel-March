using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Coursework.Managers
{
    // stores active projectiles in a list so they can be updated and removed independently of their firing units
    internal class ShellManager
    {
        public List<Shell> Shells = new List<Shell>();
        public Texture2D ShellTexture;

        // loading one shell texture lets every active projectile share the same image
        public void LoadContent(ContentManager content)
        {
            ShellTexture = content.Load<Texture2D>("ShellPic1");
        }

        // ignoring missing shells keeps invalid entries out of the active projectile list
        public void AddShell(Shell shell)
        {
            if (shell == null)
                return;
            Shells.Add(shell);
        }

        // updating from the end allows expired shells to be removed without skipping another projectile
        public void Update(GameTime gameTime)
        {
            for (int i = Shells.Count - 1; i >= 0; i--)
            {
                if (Shells[i] == null)
                {
                    Shells.RemoveAt(i);
                    continue;
                }

                Shells[i].Update(gameTime);
                if (!Shells[i].IsAlive)
                    Shells.RemoveAt(i);
            }
        }

        // drawing each active shell here keeps the game state independent of the projectile list
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var s in Shells) s.Draw(spriteBatch);
        }
    }
}