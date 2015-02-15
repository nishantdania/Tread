using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tread
{
    public class Utils
    {
        private double distance;
        private double speed;
        private double calories;
        public int steps { get; set; }
        private const double STEP_LENGTH = 78;
        private static double METRIC_RUNNING_FACTOR = 1.02784823;
        private double timeElapsed;
        public double weight { get; set; }

        public Utils()
        {
            distance = 0;
            speed = 0;
            calories = 0;
            steps = 0;
            if(weight < 10) weight = 60;  
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
            speed = distance / timeElapsed;
            speed *= 10000;
            string _speed = speed.ToString();
            if (_speed == null) return "";
            //return string.Format("{0:0.00}", speed);
            return "";
        }

        public string GetCalories()
        {
            calories = calories + weight * METRIC_RUNNING_FACTOR * STEP_LENGTH / 1000000;
            string _calories = calories.ToString();
            if (_calories == null) return "";
            return string.Format("{0:0.00}", calories); ;
        }
    }
}
