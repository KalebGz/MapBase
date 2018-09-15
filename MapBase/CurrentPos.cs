using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Timers;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using GMap.NET;

namespace MapBase
{
    public class CurrentPos : INotifyPropertyChanged
    {
        public string Lat { get { return lat; } set { lat = value; OnPropertyChanged(); } }
        private string lat;
        public string Lon { get { return lon; } set { lon = value; OnPropertyChanged(); } }
        private string lon;
        public Double NLat;
        public Double Nlon;
        private SerialPort port;
        private System.Timers.Timer timer;

        public CurrentPos()
        {
            port = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
            GetConnection();
            Timer_Tick(null, null);
            Lat = lat;
            Lon = lon;
            NLat = ProcessCoords(Lat);
            Nlon = ProcessCoords(Lon);
        }
        private void GetConnection()
        {
            InitiateConnection();
            Timer_Tick(null, null);
        }
        private void SetConnection()
        {
            Thread.CurrentThread.Name = "Main";
            Task connect = new Task( () => InitiateConnection());
            connect.Start();
            connect.Wait();
            Task tick = new Task(() => Timer_Tick(null, null));
            tick.Start();
            tick.Wait();

        }
        public PointLatLng getCoords()
        {
            
            var l = ProcessCoords(lat);
            var l2 = ProcessCoords(lon);
            return new PointLatLng(l, l2);
        }
        private double ProcessCoords(String c)
        {
            if(c == null) { Timer_Tick(null, null); }
            var t = c.Substring(1, c.Length - 1);
            var neg = 1;
            if (this.lat[0] == '-')
            {
                neg = -1;
                t = t.Substring(1, c.Length - 1);
            }
          
            return Double.Parse(t) * neg;
        }
        private Task<bool> IsConnected()
        {
            var t = InitiateConnection();
            return Task.FromResult(t);
        }

        private bool InitiateConnection()
        {
            try
            {
                port.Open();
                timer = new System.Timers.Timer
                {
                    Interval = 3000,
                    AutoReset = true,
                    Enabled = true
                };
                timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
                //timer.Start();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
               
                MessageBox.Show("No Connection");
                return false;
            }
        }
        private Task IsTicked()
        {
            Timer_Tick(null, null);
            return Task.Run(() => { Timer_Tick(null, null); });
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (port.IsOpen)
            {
                string data = port.ReadExisting();
                string[] strArr = data.Split('$');
                for (int i = 0; i < strArr.Length; i++)
                {
                    string strTemp = strArr[i];
                    Console.WriteLine(strTemp);
                    string[] lineArr = strTemp.Split(',');
                    if (lineArr[0] == "GPGGA")
                    {
                        try
                        {
                            //Latitude
                            Double dLat = Convert.ToDouble(lineArr[2]);
                            dLat = dLat / 100;
                            string[] plat = dLat.ToString().Split('.');
                            Lat = lineArr[3].ToString() +
                            plat[0].ToString() + "." +
                            ((Convert.ToDouble(plat[1]) /
                            60)).ToString("#####");

                            //Longitude
                            Double dLon = Convert.ToDouble(lineArr[4]);
                            dLon = dLon / 100;
                            string[] plon = dLon.ToString().Split('.');
                            lon = lineArr[5].ToString() +
                            plon[0].ToString() + "." +
                            ((Convert.ToDouble(plon[1]) /
                            60)).ToString("#####");


                            //Display



                            return;
                        }
                        catch
                        {
                            //Cannot Read GPS values
                            lat = "GPS Unavailable";
                            lon = "GPS Unavailable";

                        }
                    }
                }
            }
            else
            {
                lat = "COM Port Closed";
                lon = "COM Port Closed";

            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(
            [CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
        public void OnClick()
        {
            if (timer.Enabled == true)
            {
                timer.Enabled = false;
            }
            else
            {
                timer.Enabled = true;
            }
        }
    }
}