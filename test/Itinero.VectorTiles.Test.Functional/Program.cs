using Itinero.VectorTiles.Layers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Itinero.VectorTiles.Test.Functional.Staging;

namespace Itinero.VectorTiles.Test.Functional
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                if (level == Logging.TraceEventType.Verbose.ToString().ToLower())
                {
                    Log.Debug($"[{o}] {level} - {message}");
                }
                else if (level == Logging.TraceEventType.Information.ToString().ToLower())
                {
                    Log.Information($"[{o}] {level} - {message}");
                }
                else if (level == Logging.TraceEventType.Warning.ToString().ToLower())
                {
                    Log.Warning($"[{o}] {level} - {message}");
                }
                else if (level == Logging.TraceEventType.Critical.ToString().ToLower())
                {
                    Log.Fatal($"[{o}] {level} - {message}");
                }
                else if (level == Logging.TraceEventType.Error.ToString().ToLower())
                {
                    Log.Error($"[{o}] {level} - {message}");
                }
                else
                {
                    Log.Debug($"[{o}] {level} - {message}");
                }
            };

            // attach logger.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            
            // build router db.
            var routerDb = BuildRouterDb.Build();
            
            // test writing vector tiles.
            var tile = Tiles.Tile.CreateAroundLocation(51.267966846313556f, 4.801913201808929f, 9);
            var tileRange = tile.GetSubTiles(14);

            var func = new Action(() =>
            {
                foreach (var t in tileRange)
                {
                    var config = new VectorTileConfig()
                    {
                        EdgeLayerConfig = new EdgeLayerConfig()
                        {
                            Name = "cyclenetwork",
                            GetAttributesFunc = (edgeId, zoom) =>
                            {
                                var edge = routerDb.Network.GetEdge(edgeId);
                                var attributes = routerDb.GetProfileAndMeta(edge.Data.Profile, edge.Data.MetaId);
                                var ok = attributes.Any(a => a.Key == "cyclenetwork");
                                if (!ok)
                                {
                                    return null;
                                }
                                return attributes;
                            }
                        },
                        VertexLayerConfigs = new List<VertexLayerConfig>()
                        {
                            new VertexLayerConfig()
                            {
                                Name = "cyclenodes",
                                GetAttributesFunc = (vertex) =>
                                {
                                    var attributes = routerDb.GetVertexAttributes(vertex);
                                    var ok = attributes.Any(a => a.Key == "rcn_ref");
                                    if (!ok)
                                    {
                                        return null;
                                    }
                                    return attributes;
                                }
                            }
                        }
                    };

                    Log.Information("Extracting tile: {0}", t.ToInvariantString());
                    var vectorTile = routerDb.ExtractTile(t.Id, config);

                    if (vectorTile.IsEmpty) continue;
                    
                    Log.Information("Writing tile: {0}", t.ToInvariantString());
                    var fileInfo = new FileInfo(Path.Combine("tiles", t.Zoom.ToString(), t.X.ToString(), $"{t.Y}.mvt"));
                    if (!fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    using (var stream = fileInfo.Open(FileMode.Create))
                    {
                        Itinero.VectorTiles.Mapbox.MapboxTileWriter.Write(vectorTile, stream);
                    }
                }
            });
            func.TestPerf($"Extracted and written {tileRange.Count} tiles.");
        }
    }
}