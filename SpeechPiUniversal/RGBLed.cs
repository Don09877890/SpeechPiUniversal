using System;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace RaspberryModules.App.Modules
{
    public enum LedStatus { Red, Green, Blue, Purple, Yellow };

    public class RGBLed
    {
        private GpioPin _bluepin;

        private GpioPin _redpin;

        private GpioPin _greenpin;

        public bool IsEnabled;

        public void Init()
        {
            try
            {
                // Init the LED
                var gpio = GpioController.GetDefault();

                // Blue
                _bluepin = gpio.OpenPin(13);
                if (_bluepin != null)
                {
                    _bluepin.Write(GpioPinValue.High);
                    _bluepin.SetDriveMode(GpioPinDriveMode.Output);
                }

                // Green
                _greenpin = gpio.OpenPin(6);
                if (_greenpin != null)
                {
                    _greenpin.Write(GpioPinValue.High);
                    _greenpin.SetDriveMode(GpioPinDriveMode.Output);
                }

                // Red
                _redpin = gpio.OpenPin(5);
                if (_redpin != null)
                {
                    _redpin.Write(GpioPinValue.High);
                    _redpin.SetDriveMode(GpioPinDriveMode.Output);
                }

                TurnOffLed();
                IsEnabled = true;
            }
            catch (Exception e)
            {
                IsEnabled = false;
                Debug.WriteLine("LED: Unable to init");
            }
        }

        public void TurnOnLed(LedStatus ledStatus)
        {
            if (IsEnabled)
            {
                switch (ledStatus)
                {
                    case LedStatus.Red:
                        _redpin.Write(GpioPinValue.Low);
                        _bluepin.Write(GpioPinValue.High);
                        _greenpin.Write(GpioPinValue.High);
                        break;

                    case LedStatus.Green:
                        _redpin.Write(GpioPinValue.High);
                        _greenpin.Write(GpioPinValue.Low);
                        _bluepin.Write(GpioPinValue.High);
                        break;

                    case LedStatus.Blue:
                        _redpin.Write(GpioPinValue.High);
                        _greenpin.Write(GpioPinValue.High);
                        _bluepin.Write(GpioPinValue.Low);
                        break;

                    case LedStatus.Purple:
                        _redpin.Write(GpioPinValue.Low);
                        _greenpin.Write(GpioPinValue.High);
                        _bluepin.Write(GpioPinValue.Low);
                        break;
                    case LedStatus.Yellow:
                        _redpin.Write(GpioPinValue.Low);
                        _greenpin.Write(GpioPinValue.Low);
                        _bluepin.Write(GpioPinValue.Low);
                        break;
                }
            }
            else
            {
                Debug.WriteLine($"LED {ledStatus} ON (RGB Led not found)");
            }
        }

        public void TurnOffLed()
        {
            if (IsEnabled)
            {
                _redpin.Write(GpioPinValue.High);
                _greenpin.Write(GpioPinValue.High);
                _bluepin.Write(GpioPinValue.High);

            }
            else
            {
                Debug.WriteLine("LED OFF (RGB Led not found)");
            }
        }
    }
}