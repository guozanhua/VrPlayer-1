﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

using Leap;
using VrPlayer.Helpers;
using Vector = Leap.Vector;

namespace VrPlayer.Models.Trackers
{
    public class LeapTracker : TrackerBase, ITracker
    {
        CustomListener _listener;
        Controller _leap;

        public static readonly DependencyProperty RotationFactorProperty =
            DependencyProperty.Register("RotationFactorProperty", typeof(double),
            typeof(LeapTracker), new FrameworkPropertyMetadata(5D));

        public double RotationFactor
        {
            get { return (double)GetValue(RotationFactorProperty); }
            set { SetValue(RotationFactorProperty, value); }
        }

        public LeapTracker()
        {
            try
            {
                _listener = new CustomListener();
                _listener.Init += new InitEventHandler(_listener_Init);
                _listener.Frame += new FrameEventHandler(_listener_Frame);
                _listener.Exit += new ExitEventHandler(_listener_Exit);
                _listener.Connect += new ConnectEventHandler(_listener_Connect);
                _listener.Disconnect += new DisconnectEventHandler(_listener_Disconnect);

                _leap = new Controller();
                _leap.AddListener(_listener);

                this.IsEnabled = true;
                this.PositionScaleFactor = 0.01;
            }
            catch (Exception exc)
            {
                IsEnabled = false;
            }
        }

        void _listener_Exit(object sender, EventArgs e)
        {
        }

        void _listener_Connect(object sender, EventArgs e)
        {
        }

        void _listener_Disconnect(object sender, EventArgs e)
        {
            Dispose();
        }

        void _listener_Frame(object sender, ControllerEventArgs e)
        {
            //Todo: Get rid of the dispatcher if possible
            this.Dispatcher.Invoke((Action)(() =>
            {
                Frame frame = _leap.Frame();

                if (frame.Fingers.Count > 0)
                {
                    Finger finger = frame.Fingers.First();
                    if (finger != null)
                    {
                        Vector vec = finger.Direction;
                        this.RawRotation = QuaternionHelper.QuaternionFromEulerAngles(
                            RotationFactor * RadianToDegree(vec.Pitch),
                            RotationFactor * RadianToDegree(vec.Yaw),
                            0);
                        this.RawPosition = PositionScaleFactor * new Vector3D(
                            finger.TipPosition.x,
                            -finger.TipPosition.y,
                            finger.TipPosition.z
                            );
                    }

                    if (frame.Fingers.Count >= 5)
                    {
                        base.CalibratePosition();
                    }

                    base.UpdatePositionAndRotation();
                }
            }));

            
         }

        void _listener_Init(object sender, EventArgs e)
        {
            this.IsEnabled = true;
        }

        public override void Dispose()
        {
            _leap.RemoveListener(_listener);
            _listener.Dispose();
            _leap.Dispose();
        }

        private double RadianToDegree(double angle)
        {
            return 180D * angle / Math.PI;
        }
    }

    #region Custom Leap Listener

    public delegate void InitEventHandler(object sender, ControllerEventArgs e);
    public delegate void ConnectEventHandler(object sender, ControllerEventArgs e);
    public delegate void DisconnectEventHandler(object sender, ControllerEventArgs e);
    public delegate void ExitEventHandler(object sender, ControllerEventArgs e);
    public delegate void FrameEventHandler(object sender, ControllerEventArgs e);

    public class CustomListener : Listener
    {
        public event InitEventHandler Init;//dispatched once, when the controller to which the listener is registered is initialized.
        public event ConnectEventHandler Connect;//dispatched when the controller connects to the Leap and is ready to begin sending frames of motion tracking data.
        public event DisconnectEventHandler Disconnect;//dispatched if the controller disconnects from the Leap (for example, if you unplug the Leap device or shut down the Leap software).
        public event ExitEventHandler Exit;//dispatched to a listener when it is removed from a controller.
        public event FrameEventHandler Frame;//dispatched when a new frame of motion tracking data is available.

        public override void OnInit(Controller arg0)
        {
            base.OnInit(arg0);
            Init(this, null);
        }

        public override void OnConnect(Controller arg0)
        {
            base.OnConnect(arg0);
            Connect(this, null);
        }

        public override void OnDisconnect(Controller arg0)
        {
            base.OnDisconnect(arg0);
            Disconnect(this, null);
        }

        public override void OnExit(Controller arg0)
        {
            base.OnExit(arg0);
            Exit(this, null);
        }

        public override void OnFrame(Controller arg0)
        {
            base.OnFrame(arg0);
            Frame(this, new ControllerEventArgs(arg0));
        }
    }

    public class ControllerEventArgs : EventArgs
    {
        private Controller _controller;
        public Controller Controller
        {
            get { return _controller; }
        }

        public ControllerEventArgs(Controller controller)
        {
            _controller = controller;
        }
    }

    #endregion
}
