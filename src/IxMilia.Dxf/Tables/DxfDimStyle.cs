﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dxf
{
    public partial class DxfDimStyle
    {
        public const string XDataStyleName = "DSTYLE";

        public bool TryGetStyleFromXDataDifference(DxfXData xdata, out DxfDimStyle style)
        {
            style = default(DxfDimStyle);
            var dimStyleList = xdata?.Items.OfType<DxfXDataNamedList>().FirstOrDefault(l => l.Name == XDataStyleName);
            if (dimStyleList == null)
            {
                // no dim style override
                return false;
            }

            if (dimStyleList.Items.Count % 2 != 0)
            {
                // must be an even number
                return false;
            }

            var codePairs = new List<DxfCodePair>();
            for (int i = 0; i < dimStyleList.Items.Count; i += 2)
            {
                if (!(dimStyleList.Items[i] is DxfXDataInteger codeItem))
                {
                    // must alternate between int/<data>
                    return false;
                }

                DxfCodePair pair;
                var valueItem = dimStyleList.Items[i + 1];
                var code = codeItem.Value;
                switch (DxfCodePair.ExpectedType(code).Name)
                {
                    case nameof(Boolean):
                        pair = new DxfCodePair(code, true);
                        break;
                    case nameof(Double) when valueItem is DxfXDataDistance dist:
                        pair = new DxfCodePair(code, dist.Value);
                        break;
                    case nameof(Double) when valueItem is DxfXDataReal real:
                        pair = new DxfCodePair(code, real.Value);
                        break;
                    case nameof(Double) when valueItem is DxfXDataScaleFactor scale:
                        pair = new DxfCodePair(code, scale.Value);
                        break;
                    case nameof(Int16) when valueItem is DxfXDataInteger i32:
                        pair = new DxfCodePair(code, i32.Value);
                        break;
                    case nameof(Int32) when valueItem is DxfXDataLong i32:
                        pair = new DxfCodePair(code, i32.Value);
                        break;
                    case nameof(Int64) when valueItem is DxfXDataLong i32:
                        pair = new DxfCodePair(code, i32.Value);
                        break;
                    case nameof(String) when valueItem is DxfXDataString str:
                        pair = new DxfCodePair(code, str.Value);
                        break;
                    default:
                        // unexpected code pair type
                        return false;
                }

                codePairs.Add(pair);
            }

            if (codePairs.Count == 0)
            {
                // no difference to apply
                return false;
            }

            // if we made it this far, there is a differnce that should be applied
            style = Clone();
            foreach (var pair in codePairs)
            {
                style.ApplyCodePair(pair);
            }

            return true;
        }
    }
}
