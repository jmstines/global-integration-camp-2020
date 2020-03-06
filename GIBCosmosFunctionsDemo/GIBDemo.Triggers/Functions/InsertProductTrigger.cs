using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GIBDemo.Core.Helpers;
using GIBDemo.Core.Models;

namespace GIBDemo.Triggers.Functions
{
    public static class InsertProductTrigger
    {
        [FunctionName(nameof(InsertProductTrigger))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "InsertProductTrigger")] HttpRequest req,
            [CosmosDB(
                databaseName: "Products",
                collectionName: "Product",
            ConnectionStringSetting = "cosmosConnection",
            CreateIfNotExists = false,
            PartitionKey = "/ProductType",
            PreferredLocations = "Australia East"
            )] IAsyncCollector<object> products,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var input = JsonConvert.DeserializeObject<Product>(requestBody);

                var product = new Product
                {
                    ProductName = input.ProductName,
                    ProductType = input.ProductType,
                    Price = input.Price,
                    Manufacturer = input.Manufacturer
                };

                await products.AddAsync(product);
                await products.FlushAsync();

                return new OkObjectResult(product);
            }
            catch (Exception ex)
            {
                log.LogError($"Couldn't insert item. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
