using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects.Units
{
    using Coursework.GameObjects;

    internal class Soldier : BaseUnit
    {
        public Soldier(Texture2D texture, Vector2 position)
            : base(texture, 100, 2f, 10, position, 1f, 70f, 1f)
        {
        }

        public override bool FiresShells => true;

        public override void Update(Vector2 targetPosition, GameTime gameTime)
        {
            base.Update(targetPosition, gameTime);
        }
    }
}
