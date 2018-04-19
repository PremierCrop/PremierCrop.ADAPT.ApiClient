using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.user.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            ExampleConfig.Configure(configuration);
            
            Console.WriteLine("Press enter to begin.");
            Console.ReadLine();

            var referenceLinkExample = new ReferenceLinkExample();
            await referenceLinkExample.CallUsingReferenceLinks();


            Console.WriteLine("Press enter to exit.");
            Console.Read();
        }


    }
}
