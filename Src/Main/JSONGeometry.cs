using System;
using Microsoft.SqlServer.Types;
using USC.GISResearchLab.Common.Geometries;

namespace USC.GISResearchLab.Common.Core.Utils.JSON
{
    public class JSONGeometry
    {
        public string JsonShape;

        public static string GetAsJsonString(SqlGeography geog)
        {
            string ret = string.Empty;
            string type = geog.STGeometryType().Value.ToLower();

            if (geog.STNumGeometries().Value == 1)
            {
                switch (type)
                {
                    case "linestring":
                        ret = "{\"paths\":[[";
                        break;
                    case "polygon":
                        ret = "{\"rings\":[[";
                        break;
                    case "point":
                        ret = "{\"rings\":[[";
                        break;
                    default:
                        throw new NotSupportedException("Geography type '" + type + "' is not supported at this time.");
                }
            }
            else
            {
                throw new NotSupportedException("shapes with more than one geometry or ring are not supported at this time.");
            }

            for (int i = 1; i <= geog.STNumPoints().Value; i++)
            {
                if (i == 1) ret += "[" + geog.STPointN(i).Long.Value + "," + geog.STPointN(i).Lat.Value + "]";
                else ret += ",[" + geog.STPointN(i).Long.Value + "," + geog.STPointN(i).Lat.Value + "]";
            }

            ret += "]],\"spatialReference\":{\"wkid\":" + geog.STSrid.Value + "}}";
            return ret;
        }

        public SqlGeography GetAsSqlGeography()
        {
            SqlGeography ret = null;
            string ringsString;
            int i = -1, wkid = -1;

            if (!string.IsNullOrEmpty(JsonShape))
            {
                if (JsonShape.StartsWith("{\"rings\":")) // The input is polygon
                {
                    i = JsonShape.LastIndexOf(",\"spatialReference\":{\"wkid\":");
                    ringsString = JsonShape.Substring(9, i - 9);
                    wkid = int.Parse(JsonShape.Substring(i + 28, JsonShape.Length - (i + 30)));

                    ringsString = ringsString.Replace("[[[", "((");
                    ringsString = ringsString.Replace("]]]", "))");
                    ringsString = ringsString.Replace(',', ' ');
                    ringsString = "POLYGON " + ringsString.Replace("] [", ",");

                    ret = Geometry.WKT2SqlGeography(wkid, ringsString);
                }
                else
                {
                    throw new NotSupportedException("Shapes other than polygons are not supported at this time.");
                }
            }
            return ret;
        }
    }
}
