using ManagedBass;
using ManagedBass.Fx;
using System;
using System.Diagnostics;

namespace Taipu
{
    public class JukeboxSynced
    {
        public int musicStream;
        public int tempoStream;
        public double streamPosition => Bass.ChannelBytes2Seconds(tempoStream, Bass.ChannelGetPosition(tempoStream));
        public long streamPositionBytes => Bass.ChannelGetPosition(tempoStream);
        public long streamLengthBytes => Bass.ChannelGetLength(tempoStream);
        public double streamLength => Bass.ChannelBytes2Seconds(tempoStream,Bass.ChannelGetLength(tempoStream));
        public ManagedBass.PlaybackState state => Bass.ChannelIsActive(tempoStream);
        public void LoadStream(String path)
        {
            Free();
            musicStream = Bass.CreateStream(path,0,0,BassFlags.Prescan | BassFlags.Decode);
            if (musicStream == 0)
            {
                Debug.WriteLine("jukebox failed to load stream "+path);
            }
            tempoStream = BassFx.TempoCreate(musicStream, BassFlags.Default);
            Bass.ChannelSetAttribute(tempoStream, ChannelAttribute.TempoUseAAFilter, 1f);
            if (tempoStream == 0)
            {
                Debug.WriteLine("tempo stream creation failure " + Bass.LastError);
                tempoStream = musicStream;
            }


        }
        public void Start(bool restart) {
            if (tempoStream != 0) {
            Bass.ChannelPlay(tempoStream, restart);
            }
        }

        public void Stop()
        {
            if (tempoStream != 0)
            {
                Bass.ChannelStop(tempoStream);
            }
        }
        public void Seek(double secondsPosition)
        {
            if (tempoStream != 0) { 
            Bass.ChannelSetPosition(tempoStream, Bass.ChannelSeconds2Bytes(tempoStream,secondsPosition));
                }
        }
        public void Free()
        {
            if (tempoStream != 0)
            {
                Bass.StreamFree(tempoStream);
            }
            if (musicStream != 0 && musicStream!=tempoStream)
            {
                Bass.StreamFree(musicStream);
            }
            tempoStream=0;
            musicStream=0;
        }
    }
}
