using BackgroundGps.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.UI.Notifications;

namespace BackgroundGps.Engine
{
    public sealed class BackgroundGpsTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        Accelerometer _accelerometer = null;
        Geolocator _locator = new Geolocator();

        private DateTime startTime, endTime;
        private List<string> coordonates;

        private BasicGeoposition lastPosition;
        double tmpDist = 0, dist = 0;
        bool firstRun = true;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //// GET START TIME
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("startTime") == true)
            {
                object tmp = null;
                localSettings.Values.TryGetValue("startTime", out tmp);

                startTime = Convert.ToDateTime(tmp.ToString());

                System.Diagnostics.Debug.WriteLine("Start Time  " + startTime.ToString());
            }


            coordonates = new List<string>();


            _deferral = taskInstance.GetDeferral();
            try
            {
                // force gps quality readings
                _locator.DesiredAccuracy = PositionAccuracy.High;

                taskInstance.Canceled += taskInstance_Canceled;

                _accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();
                _accelerometer.ReportInterval = _accelerometer.MinimumReportInterval > 5000 ? _accelerometer.MinimumReportInterval : 5000;
                _accelerometer.ReadingChanged += accelerometer_ReadingChanged;

            }
            catch (Exception ex)
            {
                // Add your chosen analytics here
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _deferral.Complete();
        }

        async void accelerometer_ReadingChanged(Windows.Devices.Sensors.Accelerometer sender, Windows.Devices.Sensors.AccelerometerReadingChangedEventArgs args)
        {
            try
            {
                if (_locator.LocationStatus != PositionStatus.Disabled)
                {
                    try
                    {
                        Geoposition pos = await _locator.GetGeopositionAsync();

                        System.Diagnostics.Debug.WriteLine("LAT " + pos.Coordinate.Latitude + " " + pos.Coordinate.Longitude);


                        updateCoordList(pos.Coordinate.Latitude, pos.Coordinate.Longitude);

                        //update EndTime
                        this.setEndTime();

                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult != unchecked((int)0x800705b4))
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            if (_accelerometer != null)
            {
                _accelerometer.ReadingChanged -= accelerometer_ReadingChanged;
                _accelerometer.ReportInterval = 0;
            }
        }

        private void setEndTime()
        {
            /// SET END TIME
            endTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("Setting END time");


            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("endTime"))
            {
                localSettings.Values.Remove("endTime");
            }
            try
            {

                localSettings.Values.Add("endTime", endTime.ToString());
                System.Diagnostics.Debug.WriteLine("END time set");

            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("END time Couldn't be set");
            }


        }


        private double calculateTotalDistance(BasicGeoposition lastPos, BasicGeoposition newPos)
        {
            if (newPos.Equals(lastPos))
            {
                return this.dist;
            }

            DistanceUtil util = new DistanceUtil();
            double tmpDist = util.distance(lastPosition.Latitude, lastPosition.Longitude, newPos.Latitude, newPos.Longitude, 'K');

            return this.dist + tmpDist;
        }

        private void updateCoordList(double lat, double lon)
        {
            BasicGeoposition pos = new BasicGeoposition();
            pos.Latitude = lat;
            pos.Longitude = lon;

            if (firstRun)
            {
                lastPosition = pos;
                firstRun = false;
            }

            this.dist = this.calculateTotalDistance(lastPosition, pos);

            lastPosition = pos;

            System.Diagnostics.Debug.WriteLine("NEW DIST :" + dist);


            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("dist"))
            {
                localSettings.Values.Remove("dist");
            }

            try
            {
                localSettings.Values.Add("dist", this.dist);

            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("NEW DISTANCE couldn't be set");
            }
        }
    }
}
