using Foundation;
using System;
using System.Threading;
using UIKit;

namespace BasicPlayer
{
    public partial class ViewController : UIViewController
    {
        public ViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Thread thread = new Thread(new ThreadStart(Go));
            thread.Start();
        }

        void Go()
        { 
            Player player = new Player();

            int samplingFrequency = 48000;
            player.Init(samplingFrequency, 1);

            int N = 4096;
            double phi = 0;
            byte[] data = null;

            while (true)
            {
                data = new byte[2 * N];
                for ( int i = 0; i< N;i++)
                {
                    short sine = (short)(10000 * Math.Sin(phi));
                    data[2 * i] = (byte)(sine & 0xff);
                    data[2 * i + 1] = (byte)((sine >> 8) & 0xff);
                    phi += 2 * Math.PI / samplingFrequency * 1000;
                }

                player.Play(data);
            }

        }

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
        }
    }
}