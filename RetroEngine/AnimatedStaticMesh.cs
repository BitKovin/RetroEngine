using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class AnimatedStaticMesh : StaticMesh
    {

        public List<Model> frames = new List<Model>();

        public float frameTime = 0.06666666666f;

        public float animationTime = 0;

        public bool loop = false;
        public bool playing = false;

        public void Update()
        {
            if (loop)
            {
                while (frames.Count * frameTime <= animationTime)
                {
                    animationTime -= frames.Count * frameTime;
                }
            }
            else if(animationTime> frames.Count * frameTime)
            {
                animationTime = frames.Count * frameTime;
            }

            int currentFrame = (int)Math.Floor((double)(animationTime / frameTime));

            if(currentFrame > frames.Count - 1)
                currentFrame -= frames.Count;

            model = frames[currentFrame];
        }

        public void AddTime(float time)
        {
            if(playing)
                animationTime += time;
        }

        public void AddFrame(string name)
        {
            frames.Add(GetModelFromPath(name));
        }

        public void Play(float time = 0)
        {
            playing = true;

            animationTime = time;
        }

        public override void PreloadTextures()
        {
            foreach(Model m in frames)
            {
                model = m;
                LoadCurrentTextures();
            }
            
        }

    }
}
