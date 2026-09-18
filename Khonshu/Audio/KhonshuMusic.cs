using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.IO;

namespace Khonshu.Audio
{
    public sealed class KhonshuMusic : IDisposable
    {
        private  MemoryStream ms;
        //private  Mp3FileReader mp3Reader;
        private  WaveOutEvent output;
        private  VolumeSampleProvider volume;
        private  WaveStream inputStream;

        public Boolean IsLoop { get; set; } = false;
        public void Load(Byte[] AudioBytes)
        {
            Dispose();
            ms = new MemoryStream(AudioBytes, writable: false);
            inputStream = new StreamMediaFoundationReader(ms);
            var provider = inputStream.ToSampleProvider();
            if(provider.WaveFormat.Channels == 1)
            {
                provider = new MonoToStereoSampleProvider(provider);
            }



            volume = new VolumeSampleProvider(provider) { Volume = 1f };
            output = new WaveOutEvent();
            output.PlaybackStopped += (s,e) => BackPlay();
            output.Init(volume);
        }
        private void BackPlay()
        {
             inputStream.CurrentTime = TimeSpan.Zero;

            if (IsLoop)
            {
                output.Play();
            }
        }

        public String GetTimeStr()
        {
            return ForatTime(inputStream?.TotalTime ?? TimeSpan.Zero);
        }

        private String ForatTime(TimeSpan time)
        {
            return$"{(Int32)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }


        public void Play() => output?.Play();
        public void Pause() => output?.Pause();
        public void Stop()
        {
            output?.Stop();
            if (inputStream != null) inputStream.CurrentTime = TimeSpan.Zero;
        }

        public Boolean IsPlaying()
        {
            

            return output?.PlaybackState == PlaybackState.Playing;
        }
        public Boolean IsStoped()
        {
            return output?.PlaybackState == PlaybackState.Stopped;
        }
        public Boolean IsPause()
        {
            return output?.PlaybackState == PlaybackState.Paused;
        }

        public Single Volume
        {
            get => volume?.Volume ?? 1f;
            set { if (volume != null) volume.Volume = Math.Clamp(value, 0f, 1f); }
        }

        public Double CurrentSeconds
        {
            get => inputStream?.CurrentTime.TotalSeconds ?? 0;
            set { if (inputStream != null) inputStream.CurrentTime = TimeSpan.FromSeconds(value); }
        }

        public Double TotalSeconds => inputStream?.TotalTime.TotalSeconds ?? 0;

        public void Dispose()
        {
            output?.Stop();
            output?.Dispose();
            inputStream?.Dispose();
            ms?.Dispose();
        }
    }
}
