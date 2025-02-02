﻿using AnonymousWhiteLabel.UWP.Services;
using System;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(AudioRecorder))]
namespace AnonymousWhiteLabel.UWP.Services
{
    public class AudioRecorder : IAudioRecorder
    {
        private MediaCapture _mediaCapture;
        private InMemoryRandomAccessStream _memoryBuffer;

        public bool IsRecording { get; set; }

        public void DeleteOutput()
        {
            if (IsRecording)
                StopRecording();
            _memoryBuffer.Dispose();
        }

        public byte[] GetOutput()
        {
            if (IsRecording)
                StopRecording();
            var s = _memoryBuffer.CloneStream();
            var dr = new DataReader(s.GetInputStreamAt(0));
            var bytes = new byte[s.Size];
            _ = dr.LoadAsync((uint)s.Size);
            dr.ReadBytes(bytes);
            return bytes;
        }
        public void PlayRecording()
        {
            var playbackMediaElement = new MediaElement();
            playbackMediaElement.SetSource(_memoryBuffer, "MP3");
            playbackMediaElement.Play();
        }
        public async void StartRecording()
        {
            if (IsRecording)
            {
                throw new InvalidOperationException("Recording already in progress!");
            }
            _mediaCapture = new MediaCapture();

            try
            {
                await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No capture device available!");

            }

            var profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
            //profile.Audio = AudioEncodingProperties.CreatePcm(sampleRate, channels, bitsPerSample);

            _memoryBuffer = new InMemoryRandomAccessStream();
            await _mediaCapture.StartRecordToStreamAsync(profile, _memoryBuffer);


            //MediaCaptureInitializationSettings settings =
            //  new MediaCaptureInitializationSettings
            //  {
            //      StreamingCaptureMode = StreamingCaptureMode.Audio
            //  };

            //await _mediaCapture.InitializeAsync(settings);
            //await _mediaCapture.StartRecordToStreamAsync(
            //  MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), _memoryBuffer);
            IsRecording = true;
        }
        public void StopRecording()
        {
            var size = _memoryBuffer.Size;
            _ = _mediaCapture.StopRecordAsync();
            IsRecording = false;
            _mediaCapture.Dispose();
        }
    }
}
