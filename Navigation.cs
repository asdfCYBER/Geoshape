using System;
using Artitas;
using UnityEngine;
using Xenonauts.Strategy.Factories;
using Xenonauts.Strategy.Systems;

namespace Geoshape
{
    public static class Navigation
    {
        public static void MoveEntity(Entity entity, TimeSpan timeElapsed)
        {
            float hours = (float)timeElapsed.TotalHours;
            float distance_km = AircraftSystem.ToKPH(entity.Speed()) * hours;
            MoveEntity(entity, distance_km);
        }

        /// <summary>
        /// Move <paramref name="entity"/> by <paramref name="distance"/> km
        /// towards its goal and update its orientation
        /// </summary>
        public static void MoveEntity(Entity entity, float distance)
        {
            Vector2 currentPos = entity.Position();
            Vector3 currentPosNormal = Geometry.GeoscapeToNormal(currentPos);
            Vector3 targetNormal = GetTargetPosition(entity);

            // Calculate the position the entity is at after travelling distance_km to its goal
            Vector3 newPosNormal = TowardsTargetDistance(currentPosNormal, targetNormal, distance);
            Vector2 newPos = Geometry.NormalToGeoscape(newPosNormal);
            Vector2 direction = (newPos - currentPos).normalized;

            entity.AddPosition(newPos);
            entity.AddRotation(Quaternion.LookRotation(Vector3.forward, direction));
            GreatCircleArc.Update(entity, currentPosNormal, targetNormal);
        }

        /// <summary>
        /// Return the current position if there is no target, the expected interception
        /// point if the target can move, and the target's position if it is static
        /// </summary>
        public static Vector3 GetTargetPosition(Entity entity)
        {
            Entity target = entity.Goal();

            if (target is null || !target.HasPosition())
                return Geometry.GeoscapeToNormal(entity.Position());
            else if (StrategyArchetypes.CanMove.Accepts(target) && !target.IsLinkedToGoal(entity))
                return GetInterceptionPoint(entity, target);
            else
                return Geometry.GeoscapeToNormal(target.Position());
        }

        /// <summary>
        /// Return the position <paramref name="distance"/> km away from the location with
        /// normal vector <paramref name="origin"/> in the direction of <paramref name="target"/>
        /// </summary>
        public static Vector3 TowardsTargetDistance(Vector3 origin, Vector3 target, float distance)
        {
            Vector3 greatCircle = Vector3.Cross(origin, target);
            Vector3 direction = Vector3.Cross(greatCircle, origin);

            float angle = Geometry.AngleFromDistance(distance);
            return origin.normalized * Mathf.Cos(angle) + direction.normalized * Mathf.Sin(angle);
        }

        /// <summary>
        /// Return the position when travelling <paramref name="distance"/> from 
        /// <paramref name="origin"/> at heading <paramref name="heading"/>
        /// </summary>
        public static Vector3 TowardsHeadingDistance(Vector3 origin, float heading, float distance)
        {
            Vector3 north = Vector3.forward;
            Vector3 directionEast = Vector3.Cross(north, origin);
            Vector3 directionNorth = Vector3.Cross(origin, directionEast);
            Vector3 direction = directionNorth * Mathf.Cos(heading) + directionNorth * Mathf.Sin(heading);

            float angle = Geometry.AngleFromDistance(distance);
            return origin.normalized * Mathf.Cos(angle) + direction.normalized * Mathf.Sin(angle);
        }

        /// <summary>
        /// Find the point at which <paramref name="interceptor"/> can meet
        /// <paramref name="target"/> given that the target does not change course
        /// </summary>
        private static Vector3 GetInterceptionPoint(Entity interceptor, Entity target)
        {
            // Values that can be considered constants. tar = target, int = interceptor
            Vector3 tarPos = Geometry.GeoscapeToNormal(target.Position());
            Vector3 tarGoalPos = Geometry.GeoscapeToNormal(target.Goal().Value.Position());
            Vector3 intPos = Geometry.GeoscapeToNormal(interceptor.Position());
            float intSpeed = AircraftSystem.ToKPH(interceptor.Speed());
            float tarSpeed = AircraftSystem.ToKPH(target.Speed());

            // The interception position lies a distance interceptorSpeed * t away from
            // interceptorPosition, and a distance targetSpeed * t away from targetPosition.
            // The target moves along a known great circle arc. Therefore the equation
            // t - distance(interceptor at t=0, target at t)/interceptor speed = 0
            // has to be solved for t, from which we can calculate the interception position.
            // The bisection method is used to approximate this implicit equation. The function
            // is definitely negative for t = half the Earth's circumference / interceptorSpeed
            // and definitely positive for t = 0, so those are used as bounds.
            float interceptTime = BisectionMethod(delegate (float t) {
                    Vector3 tarPosAtT = TowardsTargetDistance(tarPos, tarGoalPos, tarSpeed * t);
                    return t - Geometry.DistanceBetweenPoints(tarPosAtT, intPos) / intSpeed;
                }, lowerBound: 0f, upperBound: Mathf.PI * Geometry.Radius / intSpeed);

            // No solution is found
            if (float.IsNaN(interceptTime) || interceptTime <= 0)
            {
                Debug.Log($"[Geoshape] No solution is found for {target.Name()}" +
                    $" trying to intercept {interceptor.Name()}!");
                return target.Position();
            }

            // A solution is found, the position is calculated and converted to normal vector
            float distance = tarSpeed * interceptTime;
            Vector3 interceptionPoint = TowardsTargetDistance(tarPos, tarGoalPos, distance);

            Debug.Log($"[Geoshape] {interceptor.Name()} is intercepting {target.Name()}. " +
                $"Expected interception time: {interceptTime}, distance: {distance}, interception" +
                $" point (geoscape coordinates): {Geometry.NormalToGeoscape(interceptionPoint)}");

            return interceptionPoint;
        }

        /// <summary>
        /// Finds the solution to function(x) = 0. <paramref name="function"/> must be a
        /// continuous function, and a <paramref name="lowerBound"/> and
        /// <paramref name="upperBound"/> must be given. Continues iterating until the
        /// result has a maximum possible error of <paramref name="tolerance"/>, or until
        /// <paramref name="maxIterations"/> is reached.
        /// </summary>
        /// <returns>The solution, or NaN if no solution has been found</returns>
        private static float BisectionMethod(Func<float, float> function, float lowerBound,
            float upperBound, float tolerance = 0.1f, ushort maxIterations = 100)
        {
            // Input validation
            if (lowerBound > upperBound)
                throw new ArgumentException("lowerBound needs to be lower than upperBound");
            if (Mathf.Sign(function(lowerBound)) == Mathf.Sign(function(upperBound)))
                throw new ArgumentException("The sign of function(lowerBound) can not be " +
                    "the same as the sign of function(upperBound)");

            // Bisection method: halve the search domain each step until every point
            // in the domain is at most 'tolerance' away from the exact solution
            int i = 0;
            while (i < maxIterations)
            {
                float midpoint = (lowerBound + upperBound) / 2f;
                float value = function(midpoint);
                
                if (value == 0 || (upperBound - lowerBound)/2f < tolerance)
                    return midpoint;

                if (Mathf.Sign(value) == Mathf.Sign(function(lowerBound)))
                    lowerBound = midpoint;
                else
                    upperBound = midpoint;

                i++;
            }
            
            return float.NaN;
        }
    }
}
