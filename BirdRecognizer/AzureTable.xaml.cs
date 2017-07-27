using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace BirdRecognizer
{
    public partial class AzureTable : ContentPage
    {
		Geocoder geoCoder;
       
        public AzureTable()
        {
            InitializeComponent();
            geoCoder = new Geocoder();
		}

		async void Get_ClickedAsync(object sender, System.EventArgs e)
		{
            List<BirdLocationModel> birdLocation = await AzureManager.AzureManagerInstance.GetBirdLocation();

			foreach (BirdLocationModel model in birdLocation)
			{
				var position = new Position(model.Latitude, model.Longitude);
				var possibleAddresses = await geoCoder.GetAddressesForPositionAsync(position);
                foreach (var address in possibleAddresses)
                {
                    model.City = address;
                }
			}

			BirdList.ItemsSource = birdLocation;
        }
    }
}
