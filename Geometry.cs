using Xenonauts.Strategy;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Geoshape
{
    public static class Geometry
    {
        public const int Radius = 6371;  //  Radius of Earth in km

        private static Vector2 _originCorrection = new Vector2(897, 464);  // Move the origin to Null Island

        #region Some things UnityEngine.Mathf should have but doesn't
        private static float DegToRad(float degrees) => degrees * PI / 180f;
        private static float RadToDeg(float radians) => radians / PI * 180f;
        public static float Square(this float value) => value * value;
        public static float Norm(this Vector2 vector) => vector.magnitude;
        public static float Norm(this Vector3 vector) => vector.magnitude;
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

        /// <summary>
        /// Convert a geoscape position to the position it would be at on Earth
        /// </summary>
        /// <returns></returns>
        public static Vector3 GeoscapeToCartesianPosition(Vector2 position_geoscape)
            => Radius * GeoscapeToNormal(position_geoscape);
        #endregion

        /// <summary>
        /// Return the central angle between two points
        /// </summary>
        public static float AngleBetweenPoints(Vector3 normalA, Vector3 normalB)
        {
            float crossterm = Norm(Vector3.Cross(normalA, normalB));
            float dotterm = Vector3.Dot(normalA, normalB);
            return Atan2(crossterm, dotterm);
        }

        /// <summary>
        /// Return the central angle from a circle segment length
        /// </summary>
        public static float AngleFromDistance(float distance_km)
            => distance_km / Radius;

        /// <summary>
        /// Calculate the distance between <paramref name="normalA"/>
        /// and <paramref name="normalB"/> in km
        /// </summary>
        public static float DistanceBetweenPoints(Vector3 normalA, Vector3 normalB)
            => Radius * AngleBetweenPoints(normalA, normalB);

        /// <summary>
        /// Calculate the time it takes in hours to travel from <paramref name="normalA"/> 
        /// to <paramref name="normalB"/> at <paramref name="speed"/> km/h
        /// </summary>
        public static float TimeBetweenPoints(Vector3 normalA, Vector3 normalB, float speed)
            => DistanceBetweenPoints(normalA, normalB) / speed;
    }
}
