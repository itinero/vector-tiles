using System;
using System.IO;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Serilog;

namespace Itinero.VectorTiles.Test.Functional.Staging
{
    public static class BuildRouterDb
    {
        private static string RouterDbFile = "belgium.routerdb";
        
        public static RouterDb Build()
        {
            // download data.
            Download.DownloadAll();
            
            try
            {
                if (File.Exists(RouterDbFile))
                {
                    using (var stream = File.OpenRead(RouterDbFile))
                    {
                        return RouterDb.Deserialize(stream);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("Could not load existing router db, rebuilding...");
            }
            
            var routerDb = new RouterDb();
            using (var sourceData = File.OpenRead(Download.Local))
            using (var targetData = File.Open(RouterDbFile, FileMode.Create))
            {
                routerDb.LoadOsmData(sourceData, Vehicle.Car, Vehicle.Bicycle);

                routerDb.Serialize(targetData);
            }

            return routerDb;
        }
    }
}