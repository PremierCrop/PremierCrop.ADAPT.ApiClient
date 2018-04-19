using Microsoft.Extensions.Configuration;

namespace SampleApp
{
    public static class ExampleConfig
    {
        public static void Configure(IConfigurationRoot configuration)
        {
            ApiBaseAddress = configuration["ApiBaseAddress"];
            AppId = configuration["AppId"];
            ApiKey = configuration["ApiKey"];
            CustomerUid = configuration["CustomerUid"];
            BranchUid = configuration["BranchUid"];
            GrowerUid = configuration["GrowerUid"];
            FarmUid = configuration["FarmUid"];
            FieldUid = configuration["FieldUid"];
            CropZoneId = configuration["CropZoneId"];
            CropYear = int.Parse(configuration["CropYear"]);
        }

        public static string ApiBaseAddress { get; private set; }
        public static string AppId { get; private set; }
        public static string ApiKey { get; private set; }

        public static string CustomerUid { get; private set; }
        public static string BranchUid { get; private set; }
        public static string GrowerUid { get; private set; }
        public static string FarmUid { get; private set; }
        public static string FieldUid { get; private set; }
        public static int CropYear { get; private set; }
        public static string CropZoneId { get; private set; }
    }
}
