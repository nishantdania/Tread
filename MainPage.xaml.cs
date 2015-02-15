using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Tread.Resources;
using Windows.Devices.Sensors;
using System.Diagnostics;

namespace Tread
{
    public partial class MainPage : PhoneApplicationPage
    {
        public enum PedometerStatus { Stoped, Walking, Starting }; 

        private const float MAGNETIC_FIELD_EARTH_MAX = 30.0f;
        private const float STANDARD_GRAVITY = 9.80665f;

        private const int WALKING_DELTA = 60000;
        private const int ACTION_EVAL_TIMER = 30000;

        private readonly Accelerometer _sensor;

        private Boolean started = false;

        double[] mScale = new double[2];
        private double mYOffset;
        private double mLimit = 3;
        private double mLastValues;
        private double mLastDirections;
        private double[] mLastExtremes = new double[2];
        private double mLastDiff;
        private int mLastMatch = -1;
        private double[] event_values = new double[3];
        private int stepsInaRow = 0;
        private DateTime lastStepStamp;
        public bool isWalking = false;
        private int deltaTimer = 60000;

        public event EventHandler<AccelerometerReadingChangedEventArgs> ReadingChanged;
        public event EventHandler<PedometerStatus> PedometerStatusChanged;
        public event EventHandler StepDetected;

        private MainPageViewModel viewModel;
        private int stepCount = 0;
        private Utils utils;

        private double rDist = 0;
        private double rSpeed = 0;
        private double rCal = 0;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            utils = new Utils();
            this._sensor = Accelerometer.GetDefault();
            if (_sensor == null)
            {
                Debug.WriteLine("No Accelerometer Found");
            }
            else
            {
                int h = 480;
                mYOffset = h * 0.5f;
                mScale[0] = -(h * 0.5f * (1.0f / (STANDARD_GRAVITY * 2)));
                mScale[1] = -(h * 0.5f * (1.0f / (MAGNETIC_FIELD_EARTH_MAX)));
            }
        }
        
        private void _sensor_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            if (this.ReadingChanged != null) ReadingChanged(_sensor, args);
            event_values[0] = args.Reading.AccelerationX;
            event_values[1] = args.Reading.AccelerationY;
            event_values[2] = args.Reading.AccelerationZ;
            CheckStep();
        }

        private void CheckStep()
        {
            double vSum = 0;
            for (int i = 0; i < 3; i++)
            {
                double vaux = mYOffset + event_values[i] * mScale[1];
                vSum += vaux;
            }
            double v = vSum / 3;
            float direction = (v > mLastValues ? 1 : (v < mLastValues ? -1 : 0));
            if (direction == -mLastDirections)
            {
                // Direction changed
                int extType = (direction > 0 ? 0 : 1);
                mLastExtremes[extType] = mLastValues;
                double diff = Math.Abs(mLastExtremes[extType] - mLastExtremes[1 - extType]);
                if (diff > mLimit)
                {
                    bool isAlmostAsLargeAsPrevious = diff > (mLastDiff * 2 / 3);
                    bool isPreviousLargeEnough = mLastDiff > (diff / 3);
                    bool isNotContra = (mLastMatch != 1 - extType);
                    if (isAlmostAsLargeAsPrevious && isPreviousLargeEnough && isNotContra)
                    {
                        Debug.WriteLine("STEP UP");
                        stepCount++;
                        UpdateUI();
                        TimeSpan ts = DateTime.Now - lastStepStamp;
                        if (ts.TotalMilliseconds > 1500) stepsInaRow = 0;

                        stepsInaRow++;
                        if (StepDetected != null) StepDetected(null, EventArgs.Empty);

                        if (stepsInaRow > 3 && !this.isWalking)
                        {
                            if (deltaTimer > WALKING_DELTA)
                            {
                                deltaTimer = WALKING_DELTA;
                                
                                if (!this.isWalking)
                                {

                                }
                            }
                            this.isWalking = true;
                            if (PedometerStatusChanged != null) PedometerStatusChanged(null, PedometerStatus.Walking);
                        }
                        else
                        {
                            if (stepsInaRow > 3 && this.isWalking)
                            {
                                if (deltaTimer > WALKING_DELTA * 2)
                                {
                                    deltaTimer = WALKING_DELTA * 2;
                                }
                            }
                        }
                        lastStepStamp = DateTime.Now;
                        mLastMatch = extType;
                    }
                    else
                    {
                        mLastMatch = -1;
                    }
                }
                mLastDiff = diff;
            }
            mLastDirections = direction;
            mLastValues = v;
            TimeSpan ts2 = DateTime.Now - lastStepStamp;
            if (ts2.TotalMilliseconds > (ACTION_EVAL_TIMER * 2) && isWalking)
            {
                this.isWalking = false;
                if (PedometerStatusChanged != null) PedometerStatusChanged(null, PedometerStatus.Stoped);
            }
        }

        private void UpdateUI()
        {
            utils.steps = stepCount;
            Dispatcher.BeginInvoke(() =>
            {
                viewModel.CounterText = stepCount.ToString();
                viewModel.Distance = utils.GetDistance() ;
                viewModel.Speed = utils.GetSpeed();
                viewModel.Calories = utils.GetCalories();
                viewModel.Progress = utils.Progress();
            });
        }

        private void InitializeUI()
        {
            Dispatcher.BeginInvoke(() =>
            {
                viewModel.CounterText = "0";
                viewModel.Distance = "0";
                viewModel.Speed = "0";
                viewModel.Calories = "0";
                viewModel.Progress = "0";
                viewModel.RDistance = string.Format("{0:0.00}", rDist);
                viewModel.RSpeed = string.Format("{0:0.00}", rSpeed);
                viewModel.RCal = string.Format("{0:0.00}", rCal);
            });
        }

        internal void Start()
        {
            if (this._sensor != null) this._sensor.ReadingChanged += _sensor_ReadingChanged;
        }
        internal void Stop()
        {
            if (this._sensor != null) this._sensor.ReadingChanged -= _sensor_ReadingChanged;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            viewModel = new MainPageViewModel();
            this.DataContext = viewModel;
            viewModel.CounterText = "0";
            //Start();
            InitializeUI();
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!started)
            {
                utils.InitSW();
                viewModel.CounterText = "0";
                InitializeUI();
                Start();
                myStoryboard.Begin();
                started = true;
            }
            else
            {
                if (rDist < utils.distance)
                {
                    rDist = utils.distance;
                    viewModel.RDistance = string.Format("{0:0.00}", rDist);
                }
                if (rSpeed < utils.speed)
                {
                    rSpeed = utils.speed;
                    viewModel.RSpeed = string.Format("{0:0.00}", rSpeed);
                }
                if (rCal < utils.calories)
                {
                    rCal = utils.calories;
                    viewModel.RCal = string.Format("{0:0.00}", rCal);
                }
                utils.StopSW();
                Stop();
                utils.Reset();
                stepCount = 0;
                //viewModel.CounterText = "0";
                //InitializeUI();
                myStoryboard.Stop();
                started = false;
            }
        }
    }
}