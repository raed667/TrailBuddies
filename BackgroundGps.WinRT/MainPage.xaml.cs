using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Parse;
using BackgroundGps.WinRT.Model;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace BackgroundGps.WinRT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private BackgroundTaskRegistration deviceUseTask;
        private DateTime startTime, endTime;
        private TimeSpan duration;
        private string username;

        private List<string> coordonates;

        public MainPage()
        {
            this.InitializeComponent();

            coordonates = new List<string>();

            TrackLocationButton.Click += MainPage_Loaded;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            //Buton
            StoptrackingButton.IsEnabled = false;

            try
            {
                ParseClient.Initialize("tFQtC1M0IhpZCWBBRRmqXCCE3SdUHO76f1RSNDOD", "wX0h5aUBInXq1NzNZIVx5b04kdidb4iGHKPLKidf");
            }
            catch (Exception)
            {
                MessageDialog warningDialog = new MessageDialog("We couldn't get Parse to work, please try to connect to a Wifi", "Parse BaaS");
                warningDialog.ShowAsync();
            }

        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var promise = await BackgroundExecutionManager.RequestAccessAsync();

            if (promise == BackgroundAccessStatus.Denied)
            {
                MessageDialog warningDialog = new MessageDialog("Background execution is disabled. Please re-enable in the Battery Saver app to allow this app to function", "Background GPS");
                await warningDialog.ShowAsync();
            }
            else
            {
                var defaultSensor = Windows.Devices.Sensors.Accelerometer.GetDefault();
                if (defaultSensor != null)
                {
                    var deviceUseTrigger = new DeviceUseTrigger();

                    deviceUseTask = RegisterBackgroundTask("BackgroundGps.Engine.BackgroundGpsTask", "GpsTask", deviceUseTrigger, null);

                    try
                    {
                        DeviceTriggerResult r = await deviceUseTrigger.RequestAsync(defaultSensor.DeviceId);

                        System.Diagnostics.Debug.WriteLine(r); //Allowed 
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }
        }

        public static BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint,
                                                                        string taskName,
                                                                        IBackgroundTrigger trigger,
                                                                        IBackgroundCondition condition)
        {
            //
            // Check for existing registrations of this background task.
            //

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                System.Diagnostics.Debug.WriteLine(cur.Value.Name);

                if (cur.Value.Name == taskName)
                {
                    System.Diagnostics.Debug.WriteLine("Task already registered " + taskName);
                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }

            System.Diagnostics.Debug.WriteLine("Registering new task " + taskName);

            //
            // Register the background task.
            //

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = null;

            try
            {
                task = builder.Register();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return task;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            username = e.Parameter.ToString();
        }

        private void TrackLocationButton_Click(object sender, RoutedEventArgs e)
        {
            TrackLocationButton.IsEnabled = false;
            StoptrackingButton.IsEnabled = true;
            startTime = DateTime.Now;


            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("startTime"))
            {
                localSettings.Values.Remove("startTime");
            }
            localSettings.Values.Add("startTime", startTime.ToString());

            progressRing.IsActive = true;

        }

        private async void StoptrackingButton_Click(object sender, RoutedEventArgs e)
        {
            TrackLocationButton.IsEnabled = true;
            StoptrackingButton.IsEnabled = false;
            deviceUseTask.Unregister(true);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("endTime") == true)
            {
                object tmp = null;
                localSettings.Values.TryGetValue("endTime", out tmp);

                endTime = Convert.ToDateTime(tmp.ToString());

                System.Diagnostics.Debug.WriteLine("endTime Time  " + endTime.ToString());

                duration = endTime - startTime;

                System.Diagnostics.Debug.WriteLine("Duration " + duration.TotalMinutes);
            }

            /////
            if (localSettings.Values.ContainsKey("dist") == true)
            {
                object tmp = null;
                localSettings.Values.TryGetValue("dist", out tmp);

                double dist = (double)tmp;


                System.Diagnostics.Debug.WriteLine("FINAL Dist : " + dist);

                /// PARSE
                /// 
                var trailObject = new ParseObject("Trail");
                trailObject["distance"] = dist;
                trailObject["duration"] = duration.TotalMinutes;
                trailObject["userId"] = username;

                await trailObject.SaveAsync();

                progressRing.IsActive = false;
            }
        }
    }
}
