using System.Collections.Generic;
using System.IO;
using Itinero.VectorTiles.Layers;
using Itinero.VectorTiles.Mapbox;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Itinero.VectorTiles.MapboxGL.Controllers
{
    public class VectorTileController : Controller
    {
        [HttpGet("/")]
        public IActionResult GetIndex()
        {
            return View("Index");
        }

        [HttpGet("/tiles/{z}/{x}/{y}.mvt")]
        public IActionResult GetTile(int z, int x, int y)
        {
            // x,y,z.
            var tile = new Itinero.VectorTiles.Tiles.Tile(x, y, z);
            var vectorTile = this.GetVectorTile(tile.Id);

            var stream = new MemoryStream();
            lock(Program.RouterDb)
            {
                vectorTile.Value.Write(stream, (a, l) =>
                {
                    if (l.Name != "transportation")
                    {
                        return a;
                    }

                    var result = new Attributes.AttributeCollection();
                    if (a.TryGetValue("highway", out var highway))
                    {
                        var className = string.Empty;
                        switch (highway)
                        {
                            case "motorway":
                            case "motorway_link":
                                className = "motorway";
                                break;
                            case "trunk":
                            case "trunk_link":
                                className = "trunk";
                                break;
                            case "primary":
                            case "primary_link":
                                className = "primary";
                                break;
                            case "secondary":
                            case "secondary_link":
                                className = "secondary";
                                break;
                            case "tertiary":
                            case "tertiary_link":
                                className = "tertiary";
                                break;
                            case "unclassified":
                            case "residential":
                            case "living_street":
                            case "road":
                                className = "minor";
                                break;
                            case "service":
                            case "track":
                                className = highway;
                                break;
                            case "pedestrian":
                            case "path":
                            case "footway":
                            case "cycleway":
                            case "steps":
                            case "bridleway":
                            case "corridor":
                                className = "path";
                                break;
                        }
                        if (!string.IsNullOrEmpty(className))
                        {
                            result.AddOrReplace("class", className);
                        }
                    }

                    foreach (var tag in a)
                    {
                        if (tag.Key == "highway")
                        {
                            continue;
                        }

                        result.AddOrReplace(tag.Key, tag.Value);
                    }
                    return result;
                });
            }
            stream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(stream, new MediaTypeHeaderValue("application/x-protobuf"));
        }

        /// <summary>
        /// Gets a vector tile.
        /// </summary>
        public Result<VectorTile> GetVectorTile(ulong tileId)
        {
            try
            {
                var tile = new Itinero.VectorTiles.Tiles.Tile(tileId);
                var z = tile.Zoom;

                var config = new VectorTileConfig()
                {
                    EdgeLayerConfig = new EdgeLayerConfig()
                    {
                        Name = "transportation",
                        IncludeProfileFunc = (p, m) =>
                        {
                            if (z > Program.ProfilesPerZoom.Length)
                            {
                                return true;
                            }

                            var profileSet = Program.ProfilesPerZoom[z];
                            if (profileSet == null)
                            {
                                return false;
                            }

                            return profileSet.Contains(p);
                        }
                    }
                };

                return new Result<VectorTile>(Program.RouterDb.ExtractTile(tileId, config));
            }
            catch (System.Exception ex)
            {
                return new Result<VectorTile>(ex.Message);
            }
        }
    }
}