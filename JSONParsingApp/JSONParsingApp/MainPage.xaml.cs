using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace JSONParsingApp
{
    public partial class MainPage : ContentPage
    {
        Color ColorBusEireannGreen = Color.FromHex("00875D");
        int NumberOfRows = 0;
        int NumPassages;
        long CurrentTime;

        public MainPage()
        {
            
            InitializeComponent();
            CurrentTime = GetCurrentTimeUTC();
            picker.SelectedIndex = 0;
            SetupGridHeaders();
            GetPassageInfo();
            //LBLtext.Text = "" + CurrentTime;

        }

        private long GetCurrentTimeUTC()
        {
            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        private void AddInfoToGrid(PassageData pd)
        {
            Label l;
            Grid GridMain;
            GridMain = GrdInfo;
            
            if (pd.StatusCode == 1)
            {
                l = new Label();
                l.SetValue(Grid.RowProperty, NumberOfRows);
                l.SetValue(Grid.ColumnProperty, 0);
                l.Text = pd.ActualArrival;
                l.HorizontalTextAlignment = TextAlignment.Center;
                l.FontSize = 14;
                GridMain.Children.Add(l);

                l = new Label();
                l.SetValue(Grid.RowProperty, NumberOfRows);
                l.SetValue(Grid.ColumnProperty, 1);
                l.Text = pd.CongestionLevel + "";
                l.HorizontalTextAlignment = TextAlignment.Center;
                l.FontSize = 14;
                GridMain.Children.Add(l);

                l = new Label();
                l.SetValue(Grid.RowProperty, NumberOfRows);
                l.SetValue(Grid.ColumnProperty, 2);
                //l.Text = pd.AccuracyLevel + "";
                l.Text = TimeDifferenceText(pd);
                l.HorizontalTextAlignment = TextAlignment.Center;
                l.FontSize = 14;
                GridMain.Children.Add(l);
            }
        }

        private String TimeDifferenceText(PassageData pd)
        {
            String Text = "";
            int MinuteDifference = (int)((pd.ActualArrivalUTC - CurrentTime) / 60);
            int HourDifference = MinuteDifference / 60;
            if (MinuteDifference < 60)
            {
                if(MinuteDifference == 1)
                {
                    Text = MinuteDifference + " minute";
                }
                else
                {
                    Text = MinuteDifference + " minutes";
                }

            }
            else
            {
                if(MinuteDifference % 60 == 0 && HourDifference == 1)
                {
                    Text = HourDifference + " hour";
                }
                else if(MinuteDifference % 60 == 0 && HourDifference != 1)
                {
                    Text = HourDifference + " hours";
                }
                else
                {
                    if(MinuteDifference % 60 == 1 && HourDifference == 1)
                    {
                        Text = HourDifference + " hour " + MinuteDifference % 60 + " minute";
                    }
                    else if (MinuteDifference % 60 == 1 && HourDifference > 1)
                    {
                        Text = HourDifference + " hours " + MinuteDifference % 60 + " minute";
                    }
                    else if (MinuteDifference % 60 != 1 && HourDifference == 1)
                    {
                        Text = HourDifference + " hour " + MinuteDifference % 60 + " minutes";
                    }
                    else if (MinuteDifference % 60 != 1 && HourDifference > 1)
                    {
                        Text = HourDifference + " hours " + MinuteDifference % 60 + " minutes";
                    }
                }
                {

                }
            }
            
            return Text;
        }

        private void SetupGridHeaders()
        {
            Grid GridMain;
            
            Label label;

            GridMain = GrdInfo;

            label = new Label();
            label.SetValue(Grid.RowProperty, 0);
            label.SetValue(Grid.ColumnProperty, 0);
            label.HorizontalTextAlignment = TextAlignment.Center;
            label.Text = "Arrival Time";
            label.TextColor = Color.White;
            label.BackgroundColor = ColorBusEireannGreen;
            label.FontAttributes = FontAttributes.Bold;
            label.FontSize = 16;
            GridMain.Children.Add(label);

            label = new Label();
            label.SetValue(Grid.RowProperty, 0);
            label.SetValue(Grid.ColumnProperty, 1);
            label.HorizontalTextAlignment = TextAlignment.Center;
            label.Text = "Congestion";
            label.TextColor = Color.White;
            label.BackgroundColor = ColorBusEireannGreen;
            label.FontAttributes = FontAttributes.Bold;
            label.FontSize = 16;
            GridMain.Children.Add(label);

            label = new Label();
            label.SetValue(Grid.RowProperty, 0);
            label.SetValue(Grid.ColumnProperty, 2);
            label.HorizontalTextAlignment = TextAlignment.Center;
            label.Text = "Expected In";
            label.TextColor = Color.White;
            label.BackgroundColor = ColorBusEireannGreen;
            label.FontAttributes = FontAttributes.Bold;
            label.FontSize = 16;
            GridMain.Children.Add(label);
        }

        public class PassageData{
            public string ActualArrival;
            public int CongestionLevel;
            public int AccuracyLevel;
            public int StatusCode = 4; //4 is departed// Must be 1 to be valid
            public long ActualArrivalUTC;
        }

        private int CountNumberOfPassages()
        {
            string albumurl = Uri.EscapeUriString(GetSelectedRouteURL());
            string doc = "";

            using (System.Net.WebClient client = new System.Net.WebClient()) // WebClient class inherits IDisposable
            {
                doc = client.DownloadString(albumurl);
            }

            JObject jObj = (JObject)JsonConvert.DeserializeObject(doc);
            int count = JObject.Parse(doc)["stopPassageTdi"].Count() - 1; //Return number of elements within stopPassageTdi, passage number - 1;
            return count;
        }

        private void BtnUpdate_Clicked(object sender, EventArgs e)
        {
            RemoveInfoFromGrid();
            CurrentTime = GetCurrentTimeUTC();
            GetPassageInfo();
        }

        private void GetPassageInfo()
        {

            PassageData[] Passages = new PassageData[12];
            for (int i = 0; i < 11; i++)
            {
                Passages[i] = new PassageData();
            }

            GetSelectedRouteURL();
            string albumurl = Uri.EscapeUriString(GetSelectedRouteURL());
            string doc = "";
            using (System.Net.WebClient client = new System.Net.WebClient()) // WebClient class inherits IDisposable
            {
                doc = client.DownloadString(albumurl);
            }

            NumPassages = CountNumberOfPassages();

            for (int i = 0; i < NumPassages; i++)
            {
                
                try
                {

                
                JObject jObj = (JObject)JsonConvert.DeserializeObject(doc);
                Passages[i].StatusCode = (int)JObject.Parse(doc)["stopPassageTdi"]["passage_" + i.ToString()]["status"];
                if (Passages[i].StatusCode == 1)
                {
                    Passages[i].ActualArrival = (string)JObject.Parse(doc)["stopPassageTdi"]["passage_" + i + ""]["arrival_data"]["actual_passage_time"];
                    Passages[i].ActualArrivalUTC = (long)JObject.Parse(doc)["stopPassageTdi"]["passage_" + i + ""]["arrival_data"]["actual_passage_time_utc"];
                    Passages[i].AccuracyLevel = (int)JObject.Parse(doc)["stopPassageTdi"]["passage_" + i + ""]["accuracy_level"];
                    Passages[i].CongestionLevel = (int)JObject.Parse(doc)["stopPassageTdi"]["passage_" + i + ""]["congestion_level"];
                    NumberOfRows++;
                    AddInfoToGrid(Passages[i]);
                }
                }
                catch (Exception)
                {

                }
            }
        }

        private string GetSelectedRouteURL()
        {
            String url;
            switch (picker.SelectedIndex)
            {
                case 0:
                    //St James - OPP Bayview Heights
                    url = "https://www.buseireann.ie/inc/proto/stopPassageTdi.php?stop_point=7338653551721711131&_=1572293278787";
                    break;
                case 1:
                    //Rahoon - OPP Droim Chaoin
                    url = "https://www.buseireann.ie/inc/proto/stopPassageTdi.php?stop_point=7338653551721710991&_=1572214323066";
                    break;
                default:
                    // St James By Default
                    url = "https://www.buseireann.ie/inc/proto/stopPassageTdi.php?stop_point=7338653551721711131&_=1572293278787";
                    break;
            }
            return url;
        }

        private void RemoveInfoFromGrid()
        {
            foreach (var item in GrdInfo.Children)
            {
                if (item.GetType() == typeof(Label))
                {
                    if (((Label)item).TextColor != Color.White)
                    {
                        ((Label)item).Text = "";
                    }
                }
            }

            NumberOfRows = 0;
        }
    }
    
}
