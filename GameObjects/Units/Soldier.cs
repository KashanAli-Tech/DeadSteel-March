using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework.GameObjects.Units
{
    using Coursework.GameObjects;

    // the basic unit inherits shared movement and health handling, but supplies its own stats
    internal class Soldier : BaseUnit
    {
        // passing the soldier's health, speed, damage, size, range and attack interval to the base constructor
        public Soldier(Texture2D texture, Vector2 position)
            : base(texture, 100, 2f, 10, position, 1f, 70f, 1f)
        {
        }

        // tells the unit manager to fire a projectile rather than applying an immediate tower hit
        public override bool FiresShells => true;

        public override void Update(Vector2 targetPosition, GameTime gameTime)
        {
            base.Update(targetPosition, gameTime);
        }
    }
}
