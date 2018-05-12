using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Itinero.VectorTiles.MapboxGL
{
    public class Program
    {
        internal static RouterDb RouterDb;

        internal static HashSet<ushort>[] ProfilesPerZoom;

        public static void Main(string[] args)
        {
            using(var stream = File.OpenRead(
                Path.Combine("data", "data.routerdb")))
            {
                RouterDb = Itinero.RouterDb.Deserialize(stream);
            }

            // TODO: this is now hardcoded, should be configurable, perhaps have a look at lua again.
            ProfilesPerZoom = new HashSet<ushort>[20];
            for (ushort p = 0; p < RouterDb.EdgeProfiles.Count; p++)
            {
                var profile = RouterDb.EdgeProfiles.Get(p);
                if (profile == null)
                {
                    continue;
                }
                var highway = string.Empty;
                profile.TryGetValue("highway", out highway);
                for (var z = 0; z < ProfilesPerZoom.Length; z++)
                {
                    var profiles = ProfilesPerZoom[z];
                    if (profiles == null)
                    {
                        ProfilesPerZoom[z] = new HashSet<ushort>();
                        profiles = ProfilesPerZoom[z];
                    }
                    if (z == 7 || z == 8)
                    { // osm_highway_linestring_gen4
                        if (highway == "motorway" || highway == "motorway_link" ||
                            highway == "trunk" || highway == "trunk_trunk")
                        {
                            profiles.Add(p);
                        }
                    }
                    else if (z == 9)
                    { // osm_highway_linestring_gen3
                        if (highway == "motorway" || highway == "motorway_link" ||
                            highway == "trunk" || highway == "trunk_trunk" ||
                            highway == "primary" || highway == "primary_trunk")
                        {
                            profiles.Add(p);
                        }
                    }
                    else if (z == 10)
                    { // osm_highway_linestring_gen2
                        if (highway == "motorway" || highway == "motorway_link" ||
                            highway == "trunk" || highway == "trunk_trunk" ||
                            highway == "primary" || highway == "primary_trunk" ||
                            highway == "secondary" || highway == "secondary_trunk")
                        {
                            profiles.Add(p);
                        }
                    }
                    else if (z == 11)
                    { // osm_highway_linestring_gen1
                        if (highway == "motorway" || highway == "motorway_link" ||
                            highway == "trunk" || highway == "trunk_trunk" ||
                            highway == "primary" || highway == "primary_trunk" ||
                            highway == "secondary" || highway == "secondary_trunk" ||
                            highway == "tertiary" || highway == "tertiary_trunk")
                        {
                            profiles.Add(p);
                        }
                    }
                    else if (z >= 12)
                    {
                        profiles.Add(p);
                    }
                }
            }

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build();
    }
}