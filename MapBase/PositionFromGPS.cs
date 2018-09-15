using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GMap.NET;

namespace MapBase
{
    class PositionFromGPS : IDisposable
    {   
        private static SerialPort port = new SerialPort(ConfigurationManager.AppSettings["GPSCOM"], 9600, Parity.None, 8, StopBits.One);
        private static PointLatLng p = new PointLatLng(0, 0);
        public static PointLatLng theCoords()
        {
            Console.WriteLine("Starting");
            using (port)
            {
                if (InitiateConnection() != true)
                {
                    InitiateConnection();
                }

                SetCoords();
            }
            return p;
        }

        private static bool InitiateConnection()
        {
            try
            {
                port.Open();
                return true;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("No Connection: " + ex.Message);
                return false;
            }
        }
        
        private static void SetCoords()
        {
            PointLatLng point = new PointLatLng(5, 10);
            var Lat = "";
            var Lon = "";
            
            Console.WriteLine("About to open port");
            if (port.IsOpen)
            {
                Console.WriteLine("Port Opened");
                string data = "";
                byte[] buffer = new byte[1000];
                int rcveLength = 0;
                while (rcveLength < 800)
                {
                    rcveLength += port.Read(buffer, rcveLength, 800 - rcveLength);
                }
                data = Encoding.ASCII.GetString(buffer);
                rcveLength = 0;
                Console.WriteLine(data);
                Console.WriteLine("?");
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
                            Double predLat = Convert.ToDouble(lineArr[2].Substring(0, 2));
                            int a = lineArr[2].Length - 2;
                            Double premLat = Convert.ToDouble(lineArr[2].Substring(2, a));
                            Double mlat = premLat / 60;
                            Double dLat = predLat + mlat;
                            
                            

                            //Longitude
                            Double predLon = Convert.ToDouble(lineArr[4].Substring(1, 2));
                            a = lineArr[4].Length - 3;
                            Console.WriteLine(lineArr[4]);
                            Double premLon = Convert.ToDouble(lineArr[4].Substring(3, a));
                            Double mlon = premLon / 60;
                            Double dLon = predLon + mlon;
                            //Display

                            Console.WriteLine("" + dLat + " " + dLon);
                            point = new PointLatLng(dLat, dLon);
                            p = point;
                            return;
                        }
                        catch
                        {
                            //Cannot Read GPS values
                            Console.WriteLine("NOGPS");
                            MessageBox.Show("No GPS");
                            Lat = "GPS Unavailable";
                            Lon = "GPS Unavailable";

                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("COM Port Closed");
                Lat = "COM Port Closed";
                Lon = "COM Port Closed";

            }
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
