using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using Newtonsoft.Json;
using PremierCrop.ADAPT.Rest;

namespace SampleApp
{
    public class ReferenceLinkExample
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiBaseAddress;
        
        public ReferenceLinkExample()
        {
            _apiBaseAddress = ExampleConfig.ApiBaseAddress;
            CustomDelegatingHandler customDelegatingHandler = new CustomDelegatingHandler(ExampleConfig.AppId, ExampleConfig.ApiKey);
            _httpClient = HttpClientFactory.Create(customDelegatingHandler);
        }
        public async Task CallUsingReferenceLinks()
        {
            var idFactory = new UniqueIdFactory()
            {
                UniqueIdSource = "premiercrop.com",
                UniqueIdSourceType = IdSourceTypeEnum.URI
            };

            // Get Grower by Customer
            var growers = await GetGrowersByCustomerAsync(ExampleConfig.CustomerUid);

            // Get Grower by Branch
            var growers2 = await GetGrowersByBranchAsync(ExampleConfig.BranchUid);

            //foreach (var g in growers)
            //    Console.WriteLine(g.Object.Name);

            var grower = growers.First(g => idFactory.ContainsId(g.Object.Id, ExampleConfig.GrowerUid));
            var growerSelf = await GetObjectByRel<Grower>(grower.Links, Relationships.Self);
            
            var farms = await GetListByRel<Farm>(growerSelf.Links);
            var farm = farms.First(f => idFactory.ContainsId(f.Object.Id, ExampleConfig.FarmUid));
            // Get owning grower
            var parentGrower = await GetObjectByRel<Grower>(farm.Links);

            var fields = await GetListByRel<Field>(farm.Links);
            var field = fields.First(f => idFactory.ContainsId(f.Object.Id, ExampleConfig.FieldUid));
            // self
            var fieldSelf = await GetObjectByRel<Field>(field.Links, Relationships.Self);
            // OR by Grower.
            var fields2 = await GetListByRel<Field>(grower.Links);
            var fieldBoundary = await GetObjectByRel<FieldBoundary>(field.Links, queryParams: ExampleConfig.CropYear);

            var cropZones = await GetListByRel<CropZone>(field.Links);
            var cropZone = cropZones.First(cz => idFactory.ContainsId(cz.Object.Id, ExampleConfig.CropZoneId));

            // Get WorkItemOperations
            var operations = await GetListByRel<WorkItemOperation>(field.Links, 2017, OperationTypeEnum.Fertilizing);
            var op = operations.First();

            // Get Prescription.
            var prescription = await GetObjectByRel<Prescription>(op.Links);

            // Get Products for it.
            var products = await GetObjectsByMultipleRels<CropNutritionProduct>(prescription.Links);
            var product = products.First();

            // Total pounds
            var lookup = prescription.Object.RxProductLookups.First();
            var pounds = lookup.Representation.MaxValue.Value;  // Min/Max set to same values.
            var lbsUnit = lookup.UnitOfMeasure.Code; // lbs

            // Get all CropNutrition products.
            var url = $"/CropNutritionProducts";
            var allProducts = await Get<IReadOnlyCollection<ModelEnvelope<CropNutritionProduct>>>(url);
        }



        private async Task<ModelEnvelope<T>> GetObjectByRel<T>(IEnumerable<ReferenceLink> links, string rel = null, params object[] queryParams) where T : class
        {
            rel = rel ?? typeof(T).ObjectRel();
            var link = links.Single(l => l.Rel == rel);
            return await Get<ModelEnvelope<T>>(link.Link, queryParams);
        }

        private async Task<IReadOnlyCollection<ModelEnvelope<T>>> GetObjectsByMultipleRels<T>(IEnumerable<ReferenceLink> links, params object[] queryParams) where T : class
        {
            var rel = typeof(T).ObjectRel();
            var relLinks = links.Where(l => l.Rel == rel);
            var list = new List<ModelEnvelope<T>>();
            foreach (var link in relLinks)
            {
                var x = await Get<ModelEnvelope<T>>(link.Link, queryParams);
                list.Add(x);
            }

            return list.AsReadOnly();
        }

        private async Task<IReadOnlyCollection<ModelEnvelope<T>>> GetListByRel<T>(IEnumerable<ReferenceLink> links, params object[] queryParams) where T : class
        {
            var link = links.Single(l => l.Rel == typeof(T).ListRel());
            
            return await Get<IReadOnlyCollection<ModelEnvelope<T>>>(link.Link, queryParams);
        }

        private async Task<T> Get<T>(string url, params object[] queryParams) where T: class 
        {
            foreach (var p in queryParams)
            {
                url += $"/{p}";
            }
            var response = await _httpClient.GetStringAsync($"{_apiBaseAddress}{url}");
            return JsonConvert.DeserializeObject<T>(response);
        }

        public async Task<IReadOnlyCollection<ModelEnvelope<Grower>>> GetGrowersByCustomerAsync(string customerUid)
        {
            var source = "premiercrop.com";
            var url = $"/Customers/{source}/{customerUid}/Growers";
            return await Get<IReadOnlyCollection<ModelEnvelope<Grower>>>(url);
        }

        public async Task<IReadOnlyCollection<ModelEnvelope<Grower>>> GetGrowersByBranchAsync(string branchUid)
        {
            var source = "premiercrop.com";
            var id = "00537B58-A9C5-43B3-9BEC-C49D6A2B70C8";
            DateTime lastUtc = new DateTime(2015, 1, 1);
            var url = $"/Branches/{source}/{id}/Growers/{lastUtc:yyyy-MM-dd}";
            return await Get<IReadOnlyCollection<ModelEnvelope<Grower>>>(url);
        }

    }
}