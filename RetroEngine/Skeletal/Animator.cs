using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Skeletal
{
    public class Animator
    {

        protected List<Animation> AnimationsToUpdate = new List<Animation>();

        public virtual void Update()
        {
            foreach (var animation in AnimationsToUpdate)
            {
                animation.Update(Time.deltaTime);
            }
        }

        public virtual Dictionary<string, Matrix> GetResultPose()
        {
            return new Dictionary<string, Matrix>();
        }

        protected Animation AddAnimation(string path, bool loop = true, int index = 0)
        {

            Animation animation = new Animation();

            animation.LoadFromFile(path);
            animation.PlayAnimation(index, loop);
            AnimationsToUpdate.Add(animation);

            return animation;
        }

        public virtual void Load()
        {

        }

    }
}
