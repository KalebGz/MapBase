using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace MapBase
{
   public class PositionGPS : INotifyPropertyChanged
    {
        private double latitude;
        public double Latitude
        {
            get { return latitude; }
            set
            {
                this.Point = new GMap.NET.PointLatLng(value, Longitude);
                latitude = value;
                OnPropertyChanged();
            }
        }
        private Double longitude;
        public double Longitude
        {
            get { return longitude; }
            set
            {
                this.Point = new GMap.NET.PointLatLng(Latitude, value);
                longitude = value;
                OnPropertyChanged();
            }
        }
        private int zoom;
        public int Zoom
        {
            get { return zoom; }
            set
            {
                zoom  = value;
                OnPropertyChanged();
            }
        }
        private GMap.NET.PointLatLng point;
        public GMap.NET.PointLatLng Point
        {
            get { return point; }
            set
            {
                point = value;
                OnPropertyChanged();
            }
        }
        public PositionGPS(double lat, double lon, int zoom)
        {
            Latitude = lat;
            Longitude = lon;
            Zoom = zoom;
            Point = new GMap.NET.PointLatLng(lat, lon);

        }

        public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(
                [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if( PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        
    }
}
