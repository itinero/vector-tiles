﻿// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents configuration data for the segment layer.
    /// </summary>
    public class SegmentLayerConfig : LayerConfig
    {
        /// <summary>
        /// Gets or sets the include profile func.
        /// </summary>
        public Func<ushort, uint, bool> IncludeProfileFunc { get; set; }

        /// <summary>
        /// Creates a new layer based on this configuration.
        /// </summary>
        /// <returns></returns>
        public SegmentLayer NewLayer(RouterDb routerDb)
        {
            return new SegmentLayer()
            {
                Meta = routerDb.EdgeMeta,
                Profiles =  routerDb.EdgeProfiles,
                EdgeMeta =  routerDb.EdgeData,
                Name = this.Name,
                Segments = new List<Segment>()
            };
        }
    }
}