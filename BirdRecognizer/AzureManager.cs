using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace BirdRecognizer
{
	public class AzureManager
	{

		private static AzureManager instance;
		private MobileServiceClient client;
		private IMobileServiceTable<BirdLocationModel> birdLocationTable;

		private AzureManager()
		{
			this.client = new MobileServiceClient("https://onopce01-app-1.azurewebsites.net");
            this.birdLocationTable = this.client.GetTable<BirdLocationModel>();
		}

		public MobileServiceClient AzureClient
		{
			get { return client; }
		}

		public static AzureManager AzureManagerInstance
		{
			get
			{
				if (instance == null)
				{
					instance = new AzureManager();
				}

				return instance;
			}
		}

		public async Task<List<BirdLocationModel>> GetBirdLocation()
		{
			return await this.birdLocationTable.ToListAsync();
		}

        public async Task PostBirdLocation(BirdLocationModel birdLocationTable)
        {
            await this.birdLocationTable.InsertAsync(birdLocationTable);
        }
    }
}
