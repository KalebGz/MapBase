using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Configuration;
using System.Diagnostics;
using System.ComponentModel;

namespace MapBase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Int16 RouterID = Convert.ToInt16(ConfigurationManager.AppSettings["RID"]);
        static string IPEndpoint = ConfigurationManager.AppSettings["BaseServer"];
        private PositionGPS Position;
        private PointLatLng[] FriendlyPositions;
        private System.Timers.Timer messageTimer = new System.Timers.Timer();
        public GMapMarker CurrentP = new GMapMarker(GetCPos());
        private UdpClient udpClient;

        public MainWindow()
        {
            Position = new PositionGPS(0, 0, 0);
            DataContext = Position;
            messageTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            messageTimer.Interval = 10000;
            messageTimer.Enabled = true;              
            Thread thdUDPServer = new Thread(new ThreadStart(ServerThread));
            thdUDPServer.IsBackground = true;
            thdUDPServer.Start();
            SelectCoords(CurrentP);
            InitializeComponent();
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            //GMaps.Instance.Mode = AccessMode.CacheOnly;
            // choose your provider here
            //mapView.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapView.MapProvider = BingHybridMapProvider.Instance;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 25;
            // whole world zoom
            mapView.Zoom = 15;
            // lets the map use the mousewheel to zoom
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            mapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            mapView.DragButton = MouseButton.Left;
            mapView.Position = Position.Point;

        }

        private void mapView_TextInput(object sender, TextCompositionEventArgs e)
        {
            mapView.Position = new PointLatLng(200, 100);
        }

        private void mapView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mapView.Position = Position.Point;
            mapView.Zoom = 17;
        }

        public void SelectCoords(GMapMarker CP)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to initialize at current coords", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Position.Point = new PointLatLng(CP.Position.Lat, CP.Position.Lng);
            }
            if (result == MessageBoxResult.No)
            {
                Position.Point = new PointLatLng(38.88, -77.009);
            }
        }

        private void AimVector(int heading)
        {
            GMapMarker marker = CurrentP;
            {
                marker.Shape = new Polygon
                {

                };
            }

        }
        private void HighLightTeams(PointLatLng[] points)
        {
            if (points == null) {return;}
            //if (points[1] == null) {return;}
            GMapMarker TeamA = new GMap.NET.WindowsPresentation.GMapMarker(points[0]);
            GMapMarker TeamB = new GMap.NET.WindowsPresentation.GMapMarker(points[0]);
            GMapMarker TeamC = new GMap.NET.WindowsPresentation.GMapMarker(points[0]);
            TeamA.Shape = new Rectangle
            {
                Width = 20,
                Height = 20,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            TeamB.Shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Red,
                StrokeThickness = 1.5
            };
            TeamC.Shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.AliceBlue,
                StrokeThickness = 1.5
            };

            mapView.Markers.Add(TeamA);
            mapView.Markers.Add(TeamB);
            mapView.Markers.Add(TeamC);
        }

        private void PosBtn_Click(object sender, RoutedEventArgs e)
        {
            mapView.Position = Position.Point;
            mapView.Zoom = 17;
        }

        private void CacheBtn_Click(object sender, RoutedEventArgs e)
        {
            RectLatLng area = mapView.SelectedArea;

            if (!area.IsEmpty)
            {
                for (int i = (int)mapView.Zoom; i <= mapView.MaxZoom; i++)
                {
                    TilePrefetcher obj = new TilePrefetcher();
                    obj.Title = "Prefetching Tiles";
                    obj.Icon = this.Icon;
                    obj.Owner = this;
                    obj.ShowCompleteMessage = false;
                    obj.Start(area, i, mapView.MapProvider, 100);
                }


                Close();
            }
            else
            {
                MessageBox.Show("No Area Chosen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GPS_Click(object sender, RoutedEventArgs e)
        {
            GMapMarker CPos = new GMap.NET.WindowsPresentation.GMapMarker(GetCPos());
            CPos.Shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Red,
                StrokeThickness = 1.5
            };

            mapView.Markers.Add(CPos);
        }

        private void SCOPE_Click(object sender, RoutedEventArgs e)
        {

        }

        private void COMS_Click(object sender, RoutedEventArgs e)
        {
            HighLightTeams(FriendlyPositions);
        }

        private static PointLatLng GetCPos()
        {
            PointLatLng a = PositionFromGPS.theCoords();
            return a;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
                udpClient.Connect(IPEndpoint, 8080);
                PointLatLng CPos = GetCPos();
                Byte[] senddata = Encoding.ASCII.GetBytes(RouterID + "~" + CPos.Lat + "|" + CPos.Lng);
                udpClient.Send(senddata, senddata.Length);
                System.Diagnostics.Debug.WriteLine("Tick");
                Thread thdUDPServer = new Thread(new ThreadStart(ServerThread));
                if (thdUDPServer.IsAlive == false)
                {
                    thdUDPServer.IsBackground = true;
                    thdUDPServer.Start();
                }
            
        }

        public void ServerThread()
        {
           
                while (true)
                {
                    if (udpClient == null)
                    {
                        udpClient = new UdpClient(8081);
                    }
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    try
                    {
                        Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                        FriendlyPositions = ReadLocations(Encoding.ASCII.GetString(receiveBytes));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("No Base Station Detected");
                    }
                }
           
        }

        public PointLatLng[] ReadLocations(String returnData)
        {
            String[] sub = returnData.Split('|');
            List<PointLatLng> coords = new List<PointLatLng>();
            foreach (string s in sub)
            {
                if (s.Length < 1) { break; }
                coords.Add(new PointLatLng(Convert.ToDouble(s.Split(',')[0]), Convert.ToDouble(s.Split(',')[1])));
            }
            Debug.WriteLine(coords[0]);
            return coords.ToArray();
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            udpClient.Close();
            this.Close();
        }
    }
}
