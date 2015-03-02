﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManageRecommendationModel
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string email = ConfigurationManager.AppSettings["azureDatamarket.email"];
                string key = ConfigurationManager.AppSettings["azureDatamarket.key"];
                string modelId = ConfigurationManager.AppSettings["recommendationModel.id"];
                string catalogFilePath = ConfigurationManager.AppSettings["recommendationModel.catalog.path"];
                string usageFilePath = ConfigurationManager.AppSettings["recommendationModel.usage.path"];
                bool buildModel = bool.Parse(ConfigurationManager.AppSettings["recommendationModel.build"]);

                if (email == null || key == null)
                    throw new ApplicationException("Please fill azureDatamarket.email and azureDatamarket.key in the configuration file");

                RecommendationModel model = null;

                if (modelId == null)
                {
                    model = new RecommendationModel(email, key);
                    modelId = model.CreateModel();
                    Console.WriteLine("You may want to update configuration with the following in appSettings:");
                    Console.WriteLine("<add key=\"recommendationModel.id\" value=\"{0}\" />", modelId);
                }
                else
                {
                    model = new RecommendationModel(email, key, modelId);
                }
                Console.WriteLine("using model {0}", model.ModelId);

                if (catalogFilePath == null)
                {
                    Console.WriteLine("no catalog to import");
                }
                else
                {
                    Console.WriteLine("Importing catalog '{0}'", catalogFilePath);
                    var report = model.ImportCatalog(catalogFilePath);
                    Console.WriteLine("catalog import report: {0}", report);
                }

                if (usageFilePath == null)
                {
                    Console.WriteLine("no usage to import");
                }
                else
                {
                    Console.WriteLine("Importing usage '{0}'", usageFilePath);
                    var report = model.ImportUsage(usageFilePath);
                    Console.WriteLine("catalog usage report: {0}", report);
                }

                if (buildModel)
                {
                    Console.WriteLine("\nTrigger build for model '{0}'", model.ModelId);
                    var buildId = model.BuildModel();
                    Console.WriteLine("\ttriggered build id '{0}'", buildId);

                    Console.WriteLine("\nMonitoring build");
                    //monitor the current triggered build
                    var status = BuildStatus.Create;
                    bool monitor = true;
                    while (monitor)
                    {
                        status = model.GetLatestBuildStatus();

                        Console.Write("\tbuild '{0}' (model '{1}'): status {2}", buildId, modelId, status);
                        if (status != BuildStatus.Error && status != BuildStatus.Cancelled && status != BuildStatus.Success)
                        {
                            Console.WriteLine(" --> will check in 5 sec...");
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            monitor = false;
                        }
                    }
                }

                //@@@
                //Console.WriteLine("\nGetting some recommendations...");
                //// get recommendations
                //var seedItems = new List<CatalogItem>
                //{
                //    // These item data were extracted from the catalog file in the resource folder.
                //    new CatalogItem() {Id = "1", Name = "Chébon"},
                //    new CatalogItem() {Id = "2", Name = "Fishtre"},
                //    new CatalogItem() {Id = "8", Name = "Eau du robinet"},
                //    new CatalogItem() {Id = "10", Name = "Lait de vache"}
                //};
                //Console.WriteLine("\t for single seed item");
                //// show usage for single item
                //model.InvokeRecommendations(seedItems);


            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            { 
                Console.WriteLine("---- done ----");
                Console.ReadLine();
            }
        }
    }
}
