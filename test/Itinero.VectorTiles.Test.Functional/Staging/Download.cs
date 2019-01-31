using System.IO;
using System.Net;

namespace Itinero.VectorTiles.Test.Functional.Staging
{
    /// <summary>
    /// Downloads all data needed for testing.
    /// </summary>
    public static class Download
    {
        public static string PBF = "http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf";
        public static string Local = "belgium-latest.osm.pbf";
        
        /// <summary>
        /// Downloads the luxembourg data.
        /// </summary>
        public static void DownloadAll()
        {
            if (File.Exists(Download.Local)) return;
            var client = new WebClient();
            client.DownloadFile(Download.PBF,
                Download.Local);
        }
    }
}