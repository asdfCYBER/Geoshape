using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenonauts;
using Xenonauts.Strategy;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Geoshape
{
    public static class Geometry
    {
        public const int Radius = 6_371_000;  //  Radius of Earth in meters

        private static Vector2 _originCorrection = new Vector2(897, 464);  // Move the origin to Null Island

        #region Some things UnityEngine.Mathf should have but doesn't
        private static float DegToRad(float degrees) => degrees * PI / 180f;
        private static float RadToDeg(float radians) => radians / PI * 180f;
        private static float Square(float value) => Pow(value, 2);   // Why does csharp still not have an operator for this?
        private static float Norm(Vector2 vector) => Norm(vector.x, vector.y);
        private static float Norm(float x, float y) => Sqrt(Square(x) + Square(y));  // Euclidean norm
        #endregion

        #region Coordinate transformations
        /// <summary>
        /// Convert a geoscape position to a latitude/longitude pair. Warning: this method uses estimates!
        /// </summary>
        public static Vector2 GeoscapeToGCS(Vector2 position_geoscape)
        {
            // latitude correction (GEOSCAPE_DIMENSIONS.y / 2 is not at zero latitude)
            position_geoscape -= _originCorrection;

            // all longitudes are represented on the map, but not all latitudes
            float longitude = position_geoscape.x / StrategyConstants.GEOSCAPE_PLAYABLE_BOUNDS.width * 360;
            float latitude = position_geoscape.y / 5.327f;  // estimate, roughly equal to height*140 degrees
            return new Vector2(latitude, longitude);
        }

        /// <summary>
        /// Convert a latitude/longitude pair to a geoscape position
        /// </summary>
        public static Vector2 GCSToGeoscape(Vector2 position_gcs)
        {
            float x = position_gcs.y * StrategyConstants.GEOSCAPE_PLAYABLE_BOUNDS.width / 360;
            float y = position_gcs.x * 5.327f;
            return new Vector2(x, y) + _originCorrection;
        }

        /// <summary>
        /// Convert a latitude/longitude pair to a normal vector
        /// </summary>
        public static Vector3 GCSToNormal(Vector2 position_gcs)
        {
            float latitude = DegToRad(position_gcs.x);
            float longitude = DegToRad(position_gcs.y);

            return new Vector3(
                Cos(latitude) * Cos(longitude),
                Cos(latitude) * Sin(longitude),
                Sin(latitude)
            );
        }

        /// <summary>
        /// Convert a normal vector to a latitude/longitude pair
        /// </summary>
        public static Vector2 NormalToGCS(Vector3 normal)
        {
            float latitude = Atan2(normal.z, Norm(normal.x, normal.y));
            float longitude = Atan2(normal.y, normal.x);

            return new Vector2(
                RadToDeg(latitude),
                RadToDeg(longitude)
            );
        }

        /// <summary>
        /// Convert a normal vector to a geoscape position
        /// </summary>
        public static Vector2 NormalToGeoscape(Vector3 normal)
            => GCSToGeoscape(NormalToGCS(normal));

        /// <summary>
        /// Convert a geoscape position to a normal vector
        /// </summary>
        public static Vector3 GeoscapeToNormal(Vector2 position_geoscape)
            => GCSToNormal(GeoscapeToGCS(position_geoscape));
        #endregion

        public static float AngleBetweenPoints(Vector3 normalA, Vector3 normalB)
        {
            float crossterm = Norm(Vector3.Cross(normalA, normalB));
            float dotterm = Vector3.Dot(normalA, normalB);
            return Atan2(crossterm, dotterm);
        }

        public static float AngleFromDistance(float distance)
            => RadToDeg(distance / Radius);

        public static float DistanceBetweenPoints(Vector3 normalA, Vector3 normalB)
            => Radius * AngleBetweenPoints(normalA, normalB);
    }
}
