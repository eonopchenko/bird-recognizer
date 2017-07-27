using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using BirdRecognizer.Model;
using Xamarin.Forms;
using Plugin.Geolocator;

namespace BirdRecognizer
{
    public partial class PhotoAnalysis : ContentPage
    {
        private float Lat;
        private float Lon;

        public PhotoAnalysis()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera found!", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
            {
                return;
            }

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            TagLabel.Text = "";

            await getLocationAsync();
            await MakePredictionRequest(file);
            await postLocationAsync();
        }

        async Task getLocationAsync()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;

            var position = await locator.GetPositionAsync(10000);

            Lat = (float)position.Latitude;
            Lon = (float)position.Longitude;
        }

        async Task postLocationAsync()
        {
            BirdLocationModel model = new BirdLocationModel()
            {
                Longitude = Lon,
                Latitude = Lat,
                Bird = TagLabel.Text
            };

            await AzureManager.AzureManagerInstance.PostBirdLocation(model);
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "97989a77a61f41909808f50b9ab66ba9");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/31c74fea-3d49-4eca-a5d7-725627d96d84/image?iterationId=48f18307-d678-4796-8c8b-80c9051f8061";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);

                    double max = responseModel.Predictions.Max(m => m.Probability);

                    if (max >= 0.5)
                    {
                        TagLabel.Text = responseModel.Predictions[0].Tag;
                    }
                    else
                    {
                        TagLabel.Text = "Not recognized";
                    }
                }

                //Get rid of file once we have finished using it
                file.Dispose();
            }
        }
    }
}
