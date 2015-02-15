using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tread
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const int ONE = 1;
        private string _counterText;
        private string _distance;
        private string _speed;
        private string _calories;
        public string CounterText
        {
            get
            {
                return _counterText;
            }
            set
            {
                if (value == ONE.ToString()) _counterText = value + " step / 6000 steps";
                else _counterText = value + " steps / 6000 steps";
                RaisePropertyChanged("CounterText");
            }
        }
        public string Distance
        {
            get
            {
                return _distance;
            }
            set
            {
                _distance = value + "m";
                RaisePropertyChanged("Distance");
            }
        }
        public string Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value + "m/s";
                RaisePropertyChanged("Speed");
            }
        }
        public string Calories
        {
            get
            {
                return _calories;
            }
            set
            {
                _calories = value + "cal";
                RaisePropertyChanged("Calories");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
