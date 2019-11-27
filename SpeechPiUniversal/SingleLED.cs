using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RaspberryModules.App.Modules
{
    public class SingleLED
    {
        private GpioPin _ledpin;

        public bool IsEnabled;

        public void Init(int pinNumber = 26)
        {
            try
            {
                var gpio = GpioController.GetDefault();
                _ledpin = gpio.OpenPin(pinNumber);
                if (_ledpin != null)
                {
                    _ledpin.Write(GpioPinValue.Low);
                    _ledpin.SetDriveMode(GpioPinDriveMode.Output);
                    IsEnabled = true;
                }

            }
            catch (Exception e)
            {
                IsEnabled = false;
                Debug.WriteLine("LED: Unable to init");
            }

        }

        public void TurnOn()
        {
            if (IsEnabled)
            {
                _ledpin.Write(GpioPinValue.High);
            }
            else
            {
                Debug.WriteLine("LED disabled: Turn On");
            }
        }

        public void TurnOff()
        {
            if (IsEnabled)
            {
                _ledpin.Write(GpioPinValue.Low);
            }
            else
            {
                Debug.WriteLine("LED disabled: Turn Off");
            }
        }

    }
}
