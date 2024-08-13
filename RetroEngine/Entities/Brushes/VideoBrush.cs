using MonoGame.Extended.Framework.Media;
using RetroEngine.Audio;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("videoBrush")]
    public class VideoBrush : Entity
    {

        VideoPlayer player;

        Video video;

        public VideoBrush() 
        {
            mergeBrushes = true;
            Static = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            video = AssetRegistry.LoadVideoFromFile("BadApple.mp4");

            player = new VideoPlayer(GameMain.Instance.GraphicsDevice);

        }

        public override void Destroy()
        {
            base.Destroy();

            player?.Dispose();
            video?.Dispose();

        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            player.Volume = SoundManager.Volume;

            if (player.Video == null)
            if(GameMain.SkipFrames<=0)
            {
                player.Play(video);
            }


            var tex = player.GetTexture();

            foreach(var mesh in meshes)
            {
                mesh.textureSearchPaths.Clear();
                mesh.texture = tex;
            }

        }

    }
}
