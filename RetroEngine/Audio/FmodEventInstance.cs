using FmodForFoxes;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework.Input;
using Nvidia.TextureTools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Audio
{
    public class FmodEventInstance : AudioClip
    {

        EventInstance EventInstance;

        Dictionary<string, Sound> programmerSounds = new Dictionary<string, Sound>();

        private FMOD.Studio.EVENT_CALLBACK ProgrammerSoundCallbackDelegate;


        public string SoundTableKey = string.Empty;


        [RequiresDynamicCode("")]
        public FmodEventInstance(EventInstance eventInstance) 
        {
            EventInstance = eventInstance;

            ProgrammerSoundCallbackDelegate = new FMOD.Studio.EVENT_CALLBACK(ProgrammerSoundCallback);


            EventInstance.SetCallback(ProgrammerSoundCallbackDelegate, FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND | FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND);
        }

        public override void Play(bool fromStart = false)
        {
            base.Play(fromStart);

            Update();
            EventInstance.Start();

            

        }

        public override void Pause()
        {
            base.Pause();

            EventInstance.Paused = true;

        }

        public void SetParameter(string name,float value)
        {
            EventInstance.SetParameterValue(name, value);
        }

        public override void Update()
        {
            base.Update();

            foreach (var sound in programmerSounds.Values)
            {
                sound.MaxDistance3D = MaxDistance;
                sound.MinDistance3D = MinDistance;
                sound.Is3D = Is3D;
            }

            EventInstance.Pitch = Pitch;
            EventInstance.Volume = Volume;


            Apply3D();

        }

        public override void Apply3D()
        {
            base.Apply3D();

            EventInstance.Position3D = Position;

        }

        protected override void Destroy()
        {
            base.Destroy();

            EventInstance?.Dispose();

        }

        public void SetProgrammerSound(string name, Sound sound)
        {
            if(programmerSounds.ContainsKey(name))
            {
                programmerSounds.Remove(name);
            }

            programmerSounds.Add(name, sound);
        }

        Sound GetProgrammerSound(string name)
        {
            if(programmerSounds.ContainsKey(name))
                return programmerSounds[name];




            return null;
        }

        
        private FMOD.RESULT ProgrammerSoundCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instance, IntPtr parameters)
        {
            if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND)
            {
                var soundInfo = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                string soundName = (string)soundInfo.name;

                Sound sound = GetProgrammerSound(soundName);

                if(sound!= null)
                {
                    soundInfo.sound = sound.Native.handle;
                    Marshal.StructureToPtr(soundInfo, parameters, false);
                }

                if(sound == null)
                {

                    var dialogueSound = GetSoundByName(soundName, out var dialogueSoundInfo);

                    if (dialogueSound.hasHandle())
                    {
                        soundInfo.sound = dialogueSound.handle;
                        soundInfo.subsoundIndex = dialogueSoundInfo.subsoundindex;
                        Marshal.StructureToPtr(soundInfo, parameters, false);
                    }
                }

                if (sound == null)
                {

                    Logger.Log($"FMOD programmer sound '{soundName}' not found");

                    return FMOD.RESULT.OK;
                }

                
                
            }
            else if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND)
            {
                return FMOD.RESULT.OK;
                var soundInfo = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                FMOD.Sound sound = new FMOD.Sound(soundInfo.sound);
                sound.release();
            }

            return FMOD.RESULT.OK;
        }

        List<FMOD.Sound> loadedSounds = new List<FMOD.Sound>();

        FMOD.Sound GetSoundByName(string name, out FMOD.Studio.SOUND_INFO soundInfo)
        {
            FMOD.Studio.SOUND_INFO dialogueSoundInfo;
            var keyResult = StudioSystem.Native.getSoundInfo(SoundTableKey, out dialogueSoundInfo);
            soundInfo = dialogueSoundInfo;
            if (keyResult != FMOD.RESULT.OK)
            {
                return new FMOD.Sound();
            }
            FMOD.Sound dialogueSound;
            var soundResult = CoreSystem.Native.createSound(dialogueSoundInfo.name_or_data, dialogueSoundInfo.mode, ref dialogueSoundInfo.exinfo, out dialogueSound);
            lock (loadedSounds)
            {
                loadedSounds.Add(dialogueSound);
            }
            return dialogueSound;
        }

        public static FmodEventInstance Create(string name)
        {
            var e = StudioSystem.GetEvent(name);

            e.LoadSampleData();

            var instance = e.CreateInstance();

            return new FmodEventInstance(instance);

        }

        public static FmodEventInstance CreateFromId(string guid)
        {

            FMOD.Studio.Util.parseID(guid, out var Guid);

            var e = StudioSystem.GetEvent(Guid);

            var instance = e.CreateInstance();

            return new FmodEventInstance(instance);

        }

    }
}
