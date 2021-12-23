using System;
using Foundation;
using AVFoundation;
using CoreMedia;
using System.Threading;

namespace BasicPlayer
{
    public class Player
    {
        CMSampleBufferError sampleBufferError;
        CMBlockBufferError blockBufferError0;
        CMBlockBufferError blockBufferError1;
        CMBlockBufferError blockBufferError2;
        CMBlockBufferError blockBufferError3;
        AVQueuedSampleBufferRenderingStatus status;
        CMBlockBufferFlags flags = CMBlockBufferFlags.AssureMemoryNow;
        int timescale;
        CMTime startTime;
        int totalFrameLength;
        int nChannels;
        CMAudioFormatDescription formatDescriptiom;
        AVSampleBufferRenderSynchronizer synchronizer;
        AVSampleBufferAudioRenderer audioRenderer;
    
        public Player()
        {
            synchronizer = new AVSampleBufferRenderSynchronizer();
            audioRenderer = new AVSampleBufferAudioRenderer();
            synchronizer.Add(audioRenderer);
        }

        public bool Init(int samplingFrequency, int nChannels)
        {
            this.nChannels = nChannels;
            timescale = samplingFrequency; ;
            audioRenderer.Muted = false;
            audioRenderer.Volume = 1;
            synchronizer.Rate = 1;
            startTime = synchronizer.CurrentTime;
            AVAudioFormat format = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, samplingFrequency, (uint)nChannels, true);
            IntPtr fmt = format.FormatDescription.Handle;
            formatDescriptiom = (CMAudioFormatDescription)CMAudioFormatDescription.Create(fmt);
            totalFrameLength = 0;

            AVAudioSession audioSession = AVAudioSession.SharedInstance();
            try
            {
                audioSession.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.AllowAirPlay);
            }
            catch
            {
            }
            NSError error;
            return !audioSession.SetActive(true, out error);

        }

        public bool Play(byte[]  data)
        {
                try
                {

                int blockLength = data.Length;
                int frameLength = blockLength / (2 * nChannels);
                    CMCustomBlockAllocator cba = new CMCustomBlockAllocator();

                    CMBlockBuffer blockListBuffer = CMBlockBuffer.CreateEmpty(0, 0, out blockBufferError0);
                    if (blockBufferError0 != CMBlockBufferError.None)
                        Thread.Sleep(10);

                    CMBlockBuffer blockBuffer = CMBlockBuffer.FromMemoryBlock(IntPtr.Zero, (uint)blockLength, cba, 0, (nuint)blockLength, flags, out blockBufferError1);
                    if (blockBufferError1 != CMBlockBufferError.None)
                        Thread.Sleep(10);

                    blockBufferError2 = blockBuffer.ReplaceDataBytes(data, 0);
                    if (blockBufferError2 != CMBlockBufferError.None)
                        Thread.Sleep(10);

                    blockBufferError3 = blockListBuffer.AppendBuffer(blockBuffer, 0, (uint)blockLength, 0);
                    if (blockBufferError3 != CMBlockBufferError.None)
                        Thread.Sleep(10);

                    CMTime sampleTime = new CMTime(totalFrameLength, timescale) + startTime;

                    CMSampleBuffer sampleBuffer = CMSampleBuffer.CreateReadyWithPacketDescriptions(blockListBuffer, formatDescriptiom, frameLength, sampleTime, null, out sampleBufferError);
                    if (sampleBufferError != CMSampleBufferError.None)
                        Thread.Sleep(10);
                        
                    totalFrameLength += frameLength;

                    if (!audioRenderer.HasSufficientMediaDataForReliablePlaybackStart)
                    {
                        audioRenderer.Enqueue(sampleBuffer);
                        status = audioRenderer.Status;
                        if (status != AVQueuedSampleBufferRenderingStatus.Rendering)
                            Thread.Sleep(10);
                    }
                    else if (audioRenderer.ReadyForMoreMediaData)
                    {
                        audioRenderer.Enqueue(sampleBuffer);
                        status = audioRenderer.Status;
                        if (status != AVQueuedSampleBufferRenderingStatus.Rendering)
                            Thread.Sleep(10);
                    }
                    else
                    {
                    int count = 0;
                    int countMax = 500;
                        while (!audioRenderer.ReadyForMoreMediaData && count < countMax)
                        {
                            Thread.Sleep(10);
                        count++;
                        }
                    if (count >= countMax)
                        return false;
                        audioRenderer.Enqueue(sampleBuffer);
                    }
                }
                catch (Exception e)
                {
                    string s1 = e.Message;
                    string s2 = e.StackTrace;
                }
            return true;
            }

        public void Flush()
        {
            audioRenderer.Flush();
        }
    }
}