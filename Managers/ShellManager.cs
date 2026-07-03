using Coursework.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Coursework.Managers
{
    internal class ShellManager
    {

        // THERE ARE SOME ERRORS IN THIS
        // I NEED TO FIX THEM AFTER FINISHING COURSEWORK DOCUMENTATION - MILESTONES

        public List<Shell> Shells = new List<Shell>();
        public Texture2D ShellTexture;

        public void LoadContent(ContentManager content)
        {
            // Provide a default shell texture name in your Content
            ShellTexture = content.Load<Texture2D>("ShellPic1");
        }

        public void AddShell(Shell shell)
        {
            Shells.Add(shell);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = Shells.Count - 1; i >= 0; i--)
            {
                Shells[i].Update(gameTime);
                if (!Shells[i].IsAlive) Shells.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var s in Shells) s.Draw(spriteBatch);
        }
    }
}
