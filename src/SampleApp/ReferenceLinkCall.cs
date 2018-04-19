using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using Newtonsoft.Json;
using PremierCrop.ADAPT.Rest;

namespace SampleApp
{
    public class ReferenceLinkCall
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiBaseAddress;

        public ReferenceLinkCall(string apiBaseAddress, string appId, string apiKey)
        {
            _apiBaseAddress = apiBaseAddress;
            CustomDelegatingHandler customDelegatingHandler = new CustomDelegatingHandler(appId, apiKey);
            _httpClient = HttpClientFactory.Create(customDelegatingHandler);
        }

        public void CallUsingReferenceLinks()
        {

            // Get Grower by Customer
            var growers = GetGrowersByCustomerAsync().Result;

            // Get Grower by Branch
            //var growers2 = GetGrowersByBranchAsync().Result;
            
            var grower = growers.First();
            var growerSelf = GetObjectByRel<ModelEnvelope<Grower>>(grower.Links, Relationships.Self).Result;
            
            var farms = GetListByRel<Farm>(growerSelf.Links).Result;
            var farm = farms.First();
            // Get owning grower
            var parentGrower = GetObjectByRel<Grower>(farm.Links);

            var fields = GetListByRel<Field>(farm.Links).Result;
            var field = fields.First();
            // OR by Grower.
            var fields2 = GetListByRel<Field>(grower.Links).Result;
            
            var cropZones = GetListByRel<CropZone>(field.Links).Result;
            // Other options.
            var cropZones2 = GetListByRel<CropZone>(grower.Links).Result;
            var cropZones3 = GetListByRel<CropZone>(field.Links).Result;
            // TODO: How to allow cropYear param in the rel???  Or just get all and filter.  

            var cropZone = cropZones.First();
            var fieldBoundary = GetObjectByRel<FieldBoundary>(cropZone.Links).Result;

            // Other Options
            //// Get all crop zones where boundary has been updated since X date.
            //var cropZonesBoundaries = ListCropZonesWithBoundaryChangesByLastUpdatedAsync().Result;
            //var cropZoneId2 = cropZonesBoundaries.First().Object.Id.UniqueIds.First();
            //var fieldBoundary2 = GetFieldBoundaryAsync(cropZoneId2).Result;

            Console.WriteLine("Press enter to exit.");
            Console.Read();

        }



        private async Task<T> GetObjectByRel<T>(IEnumerable<ReferenceLink> links, string rel = null) where T : class
        {
            rel = rel ?? typeof(T).ObjectRel();
            var link = links.Single(l => l.Rel == rel);
            return await Get<T>(link.Link);
        }

        private async Task<IReadOnlyCollection<ModelEnvelope<T>>> GetListByRel<T>(IEnumerable<ReferenceLink> links) where T : class
        {
            var link = links.Single(l => l.Rel == typeof(T).ListRel());
            return await Get<IReadOnlyCollection<ModelEnvelope<T>>>(link.Link);
        }

        private async Task<T> Get<T>(string url) where T: class 
        {
            var response = await _httpClient.GetStringAsync($"{_apiBaseAddress}{url}");
            return JsonConvert.DeserializeObject<T>(response);
        }

        public async Task<IReadOnlyCollection<ModelEnvelope<Grower>>> GetGrowersByCustomerAsync()
        {
            var source = "premiercrop.com";
            var id = "E38E3612-1033-48DF-B951-C29F237BC96A";
            var url = $"/Customers/{source}/{id}/Growers";
            return await Get<IReadOnlyCollection<ModelEnvelope<Grower>>>(url);
        }

        public async Task<IReadOnlyCollection<ModelEnvelope<Grower>>> GetGrowersByBranchAsync()
        {
            var source = "premiercrop.com";
            var id = "00537B58-A9C5-43B3-9BEC-C49D6A2B70C8";
            DateTime lastUtc = new DateTime(2015, 1, 1);
            var url = $"/Branches/{source}/{id}/Growers/{lastUtc:yyyy-MM-dd}";
            return await Get<IReadOnlyCollection<ModelEnvelope<Grower>>>(url);
        }

    }
}