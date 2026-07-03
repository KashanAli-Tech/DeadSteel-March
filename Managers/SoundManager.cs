using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Coursework.Managers
{
    internal class SoundManager
    {
        // I NEED TO FIND A DIFFERENT SHOOTING SOUND FOR TANK AND MACHINE GUNNER WHENEVER FREE




        private SoundEffect shoot;
        private SoundEffect explosion;
        private SoundEffect click;

        public void LoadContent(ContentManager content)
        {
            // Provide these assets in Content
            shoot = content.Load<SoundEffect>("shoot"); 
            explosion = content.Load<SoundEffect>("explosion");
            click = content.Load<SoundEffect>("click");
        }

        public void PlayShoot() { shoot?.Play(); }
        public void PlayExplosion() { explosion?.Play(); }
        public void PlayClick() { click?.Play(); }
    }
}
