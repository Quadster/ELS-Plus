﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace ELS.Siren
{
    public enum ToneType
    {
        Horn,
        SrnTon1,
        SrnTon2,
        SrnTon3,
        SrnTon4,
        SrnPnic
    }
    class Tone
    {
        
        private readonly string _file;
        private int soundId = Function.Call<int>(Hash.GET_SOUND_ID);
        private Entity _entity;
        private readonly ToneType _type;
        internal string Type;
        internal bool _state { private set;  get; }
        internal bool AllowUse { get; set; }
        internal Tone(string file, Entity entity,ToneType type,bool allow, bool state =false)
        {
            _entity = entity;
            _file = file;
            _type = type;
            SetState(state);
            AllowUse = allow;
            switch(type)
            {
                case ToneType.SrnTon1:
                    Type = "WL";
                    break;
                case ToneType.SrnTon2:
                    Type = "YP";
                    break;
                case ToneType.SrnTon3:
                    Type = "A1";
                    break;
                case ToneType.SrnTon4:
                    Type = "A2";
                    break;
            }
        }

        internal void SetState(bool state)
        {
            
                _state = state;
                if (_state && AllowUse)
                {
                    if (!Audio.HasSoundFinished(soundId)) return;
                    Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, soundId, (InputArgument)_file, (InputArgument)_entity.Handle, (InputArgument)0, (InputArgument)0, (InputArgument)0);
                }
                else
                {
                    Audio.StopSound(soundId);
                }
        }
        
        internal void CleanUp()
        {
#if DEBUG
            CitizenFX.Core.Debug.WriteLine("Tone deconstructor ran");
#endif
            Audio.StopSound(soundId);
            Audio.ReleaseSound(soundId);
        }
    }
}
