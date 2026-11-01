using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects.Units
{
    using Coursework.GameObjects;

    // a slower unit with more health and damage, using the same base movement as the other units
    internal class Tank : BaseUnit
    {
        // giving the tank its own stats here avoids copying the movement and damage methods
        public Tank(Texture2D texture, Vector2 position)
            : base(texture, 300, 1.2f, 40, position, 1.2f, 220f, 1.2f)
        {
        }

        // the tank's damage is carried by its shell and applied when the shell reaches a tower
        public override bool FiresShells => true;

        public override void Update(Vector2 targetPosition, GameTime gameTime)
        {
            base.Update(targetPosition, gameTime);
        }
    }
}
