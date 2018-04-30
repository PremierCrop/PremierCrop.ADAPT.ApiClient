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
        private readonly ReferenceLinkClient _client;
        
        public ReferenceLinkExample()
        {
            CustomDelegatingHandler customDelegatingHandler = new CustomDelegatingHandler(ExampleConfig.AppId, ExampleConfig.ApiKey);
            var httpClient = HttpClientFactory.Create(customDelegatingHandler);
            _client = new ReferenceLinkClient(httpClient, ExampleConfig.ApiBaseAddress);
        }
        public async Task CallUsingReferenceLinks()
        {
            var idFactory = new UniqueIdFactory()
            {
                UniqueIdSource = "premiercrop.com",
                UniqueIdSourceType = IdSourceTypeEnum.URI
            };

            // Get Grower by Customer
            var growersCustomer = await GetGrowersByCustomerAsync(idFactory.UniqueIdSource, ExampleConfig.CustomerUid);
            Console.WriteLine($"Growers By Customer count: {growersCustomer.Count}");

            // Get Grower by Branch
            var growersBranch = await GetGrowersByBranchAsync(idFactory.UniqueIdSource, ExampleConfig.BranchUid);
            Console.WriteLine($"Growers By Branch count: {growersBranch.Count}");

            // Growers
            Console.WriteLine("Growers");
            var growers = await _client.Get<IReadOnlyCollection<ModelEnvelope<Grower>>>("/Growers");
            Console.WriteLine($"All growers count: {growers.Count}");

            var grower = growers.First(g => idFactory.ContainsId(g.Object.Id, ExampleConfig.GrowerUid));
            Console.WriteLine($"Grower Name: {grower.Object.Name}.");
            var growerSelf = await _client.GetObjectByRel<Grower>(grower.Links, Relationships.Self);
            Console.WriteLine($"Grower Self Name: {growerSelf.Object.Name}.");

            var farmsByGrower = await _client.GetListByRel<Farm>(growerSelf.Links);
            Console.WriteLine($"Farms count for Grower Self: {farmsByGrower.Count}");

            var fieldsByGrower = await _client.GetListByRel<Field>(growerSelf.Links);
            Console.WriteLine($"Fields count for Grower Self: {fieldsByGrower.Count}");

            // Farms
            Console.WriteLine();
            Console.WriteLine("Farms");
            var farm = farmsByGrower.First(f => idFactory.ContainsId(f.Object.Id, ExampleConfig.FarmUid));
            Console.WriteLine($"Farm Description: {farm.Object.Description}.");
            var farmSelf = await _client.GetObjectByRel<Farm>(farm.Links, Relationships.Self);
            Console.WriteLine($"Self Farm Description: {farmSelf.Object.Description}.");
            // Get owning grower
            var farmGrower = await _client.GetObjectByRel<Grower>(farmSelf.Links);
            Console.WriteLine($"Self Farm Grower Name: {farmGrower.Object.Name}.");
            // Get fields
            var fields = await _client.GetListByRel<Field>(farmSelf.Links);
            Console.WriteLine($"Self Farm Fields count: {fields.Count}.");

            // Fields
            Console.WriteLine();
            Console.WriteLine("Fields");
            var field = fields.First(f => idFactory.ContainsId(f.Object.Id, ExampleConfig.FieldUid));
            Console.WriteLine($"First Field Description: {field.Object.Description}.");
            var fieldSelf = await _client.GetObjectByRel<Field>(field.Links, Relationships.Self);
            Console.WriteLine($"Self Field Description: {fieldSelf.Object.Description}.");
            // Get owning grower
            var fieldGrower = await _client.GetObjectByRel<Grower>(fieldSelf.Links);
            Console.WriteLine($"Self Field Grower Name: {fieldGrower.Object.Name}.");
            // Get owning farm
            var fieldFarm = await _client.GetObjectByRel<Farm>(fieldSelf.Links);
            Console.WriteLine($"Self Field Farm Description: {fieldFarm.Object.Description}.");
            // Get all Crop Zones
            var cropZones = await _client.GetListByRel<CropZone>(fieldSelf.Links);
            Console.WriteLine($"Self Field CropZones count: {cropZones.Count}.");
            // Get CropZones for crop year by adding param.
            var cropYearCropZones = await _client.GetListByRel<CropZone>(fieldSelf.Links, ExampleConfig.CropYear);
            Console.WriteLine($"Self Field CropZones for  Crop Year count: {cropYearCropZones.Count}.");
            // Get FieldBoundary for current crop year by adding param. (rel = null is 2nd param that isn't required, so specify queryParams).
            var boundary = await _client.GetObjectByRel<FieldBoundary>(fieldSelf.Links, queryParams: ExampleConfig.CropYear);
            Console.WriteLine(boundary != null
                ? $"Self Field FieldBoundary Crop Year: {boundary.Object.TimeScopes[0]?.TimeStamp1?.Year}."
                : "No boundary found for CropZone.");

            // CropZones
            Console.WriteLine();
            Console.WriteLine("CropZones");
            var firstCropZone = cropYearCropZones.First();
            Console.WriteLine($"First CropZone Description: {firstCropZone.Object.Description}.");
            var cropZoneSelf = await _client.GetObjectByRel<CropZone>(firstCropZone.Links, Relationships.Self);
            Console.WriteLine($"Self CropZone Description: {cropZoneSelf.Object.Description}.");
            // Get owning field
            var cropZoneField = await _client.GetObjectByRel<Field>(cropZoneSelf.Links);
            Console.WriteLine($"Self CropZone Field Description: {cropZoneField.Object.Description}.");

            // FieldBoundaries
            if (boundary == null)
            {
                Console.WriteLine("No boundaries to process.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("FieldBoundaries");
                var boundarySelf = await _client.GetObjectByRel<FieldBoundary>(boundary.Links, Relationships.Self);
                Console.WriteLine(
                    $"Self FieldBoundary Crop Year: {boundarySelf.Object.TimeScopes[0]?.TimeStamp1?.Year}.");
                // Get owning field
                var boundaryField = await _client.GetObjectByRel<Field>(boundarySelf.Links);
                Console.WriteLine($"Self FieldBoundary Field Description: {boundaryField.Object.Description}.");
            }
            
            // WorkItemOperations
            Console.WriteLine();
            Console.WriteLine("WorkItemOperations");
            var operations = await _client.GetListByRel<WorkItemOperation>(field.Links, ExampleConfig.CropYear, OperationTypeEnum.Fertilizing);
            Console.WriteLine($"WorkItemOperations count: {operations.Count}.");

            foreach (var op in operations)
            {
                Console.WriteLine();
                Console.WriteLine($"WorkItemOperation Id: {op.Object.Id.UniqueIds.FirstOrDefault()?.Id}");
                Console.WriteLine($"WorkItemOperation Description: {op.Object.Description}.");

                // Get Prescription.
                var prescription = await _client.GetObjectByRel<Prescription>(op.Links);
                Console.WriteLine($"Prescription Description: {prescription.Object.Description}.");

                // Get Products for it.
                var products = await _client.GetObjectsByMultipleRels<CropNutritionProduct>(prescription.Links);
                Console.WriteLine($"Prescription Products Count: {products.Count}.");
                var product = products.First();
                Console.WriteLine($"First Product Description: {product.Object.Description}.");

                // Total pounds
                var lookup = prescription.Object.RxProductLookups.First();
                var pounds = lookup.Representation.MaxValue.Value; // Min/Max set to same values.
                var lbsUnit = lookup.UnitOfMeasure.Code; // lbs
                Console.WriteLine($"First Product used: {pounds} {lbsUnit}.");
            }


            // Products
            Console.WriteLine();
            Console.WriteLine("Products");
            var url = $"/CropNutritionProducts";
            var allProducts = await _client.Get<IReadOnlyCollection<ModelEnvelope<CropNutritionProduct>>>(url);
            Console.WriteLine($"All CropNutritionProducts count: {allProducts.Count}.");
        }



        public async Task<IReadOnlyCollection<ModelEnvelope<Grower>>> GetGrowersByCustomerAsync(string source, string customerUid)
        {
            var url = $"/Customers/{source}/{customerUid}/Growers";
            return await _client.Get<IReadOnlyCollection<ModelEnvelope<Grower>>>(url);
        }

        public async Task<IReadOnlyCollection<ModelEnvelope<Grower>>> GetGrowersByBranchAsync(string source, string branchUid)
        {
            DateTime lastUtc = new DateTime(2015, 1, 1);
            var url = $"/Branches/{source}/{branchUid}/Growers/{lastUtc:yyyy-MM-dd}";
            return await _client.Get<IReadOnlyCollection<ModelEnvelope<Grower>>>(url);
        }

    }
}