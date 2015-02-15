using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tread
{
    public class Utils
    {
        public double distance{get; private set;}
        public double speed { get; private set; }
        public double calories { get; private set; }
        public int steps { get; set; }
        private const double STEP_LENGTH = 78;
        private static double METRIC_RUNNING_FACTOR = 1.02784823;
        private double timeElapsed;
        public double weight { get; set; }
        private string progress;
        private Stopwatch stopWatch;
        

        public Utils()
        {
            stopWatch = new Stopwatch();
            distance = 0;
            speed = 0;
            calories = 0;
            steps = 0;
            if(weight < 10) weight = 60;  
        }

        public string Progress()
        {
            progress = ((int)(steps / 6)).ToString();
            return progress;
        }

        public string GetDistance()
        {
            distance = (distance + STEP_LENGTH/100);
            string _distance = distance.ToString();
            if (_distance == null) return "";
            return string.Format("{0:0.00}", distance);
        }

        public string GetSpeed()
        {
            timeElapsed = stopWatch.ElapsedMilliseconds/1000;
            speed = distance / timeElapsed;
            string _speed = speed.ToString();
            if (_speed == null) return "";
            return string.Format("{0:0.00}", speed);
            //return "";
        }

        public string GetCalories()
        {
            calories = calories + weight * METRIC_RUNNING_FACTOR * STEP_LENGTH / 1000000;
            string _calories = calories.ToString();
            if (_calories == null) return "";
            return string.Format("{0:0.00}", calories); 
        }


        public void InitSW()
        {
            stopWatch.Start();
        }

        public void StopSW()
        {
            stopWatch.Stop();
        }

        public void Reset()
        {
            distance = 0;
            calories = 0;
            speed = 0;

        }
    }
}
