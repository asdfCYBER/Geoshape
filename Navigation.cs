﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artitas;
using UnityEngine;
using Xenonauts.Strategy;
using Xenonauts.Strategy.Scripts;
using Xenonauts.Strategy.Systems;
using Xenonauts.Strategy.Factories;

namespace Geoshape
{
    public static class Navigation
    {
        public static void MoveEntity(Entity entity, TimeSpan timeElapsed)
        {
            Entity target = entity.Goal();
            if (!target.HasPosition())
                return;

            if (StrategyArchetypes.CanMove.Accepts(target) && !target.IsLinkedToGoal(entity))
                ToMovingTarget(entity, timeElapsed);
            else
                ToStaticTarget(entity, timeElapsed);
        }

        private static void ToMovingTarget(Entity entity, TimeSpan timeElapsed)
        {
            Vector3 interceptionNormal = GetInterceptionPoint(entity, entity.Goal()).normalized;
            Vector2 interceptionGeoscape = Geometry.NormalToGeoscape(interceptionNormal);
            GreatCircleArc arc = GreatCircleArc.GetArc(entity, interceptionGeoscape);

            // Get the elapsed time, multiply by the speed and
            // move that much distance along the great circle
            float hours = (float)timeElapsed.TotalHours;
            float distance_km = AircraftSystem.ToKPH(entity.Speed()) * hours;
            Vector2 newPosition = arc.MoveDistanceFrom((Vector2)entity.Position(), distance_km);
            Vector2 direction = arc.DirectionAt((Vector2)entity.Position());

            entity.AddPosition(newPosition);
            entity.AddRotation(Quaternion.LookRotation(Vector3.forward, direction));
        }

        private static void ToStaticTarget(Entity entity, TimeSpan timeElapsed)
        {
            GreatCircleArc arc = GreatCircleArc.GetArc(entity);

            // Get the elapsed time, multiply by the speed and
            // move that much distance along the great circle
            float hours = (float)timeElapsed.TotalHours;
            float distance_km = AircraftSystem.ToKPH(entity.Speed()) * hours;
            Vector2 newPosition = arc.MoveDistanceFrom((Vector2)entity.Position(), distance_km);
            Vector2 direction = arc.DirectionAt((Vector2)entity.Position());

            entity.AddPosition(newPosition);
            entity.AddRotation(Quaternion.LookRotation(Vector3.forward, direction));
        }

        private static Vector3 GetInterceptionPoint(Entity interceptor, Entity target)
        {
            GreatCircleArc targetArc = GreatCircleArc.GetArc(target);
            if (targetArc == null)
            {
                Debug.Log($"[Geoshape] Unable to get the great circle for {target.Name()}");
                return target.Position(); // TODO: fix wrong coordinate system
            }

            // Values that can be considered constants. tar = target, int = interceptor
            Vector3 tarPos = Geometry.GeoscapeToCartesianPosition(target.Position());
            Vector3 intPos = Geometry.GeoscapeToCartesianPosition(interceptor.Position());
            float intSpeed = AircraftSystem.ToKPH(interceptor.Speed());
            float tarSpeed = AircraftSystem.ToKPH(target.Speed());

            // The interception position lies a distance interceptorSpeed * t away from
            // interceptorPosition, and a distance targetSpeed * t away from targetPosition.
            // The target moves along a known great circle arc. Therefore the equation
            // ||targetPosition(t)-interceptorPosition(t=0)||^2 - (interceptorSpeed*t)^2 = 0   // not norm of difference but distance over surface!
            // has to be solved for t, from which we can calculate the interception position.
            // I couldn't find an explicit solution for t, so the bisection method is used.
            // The function is definitely negative for t = radius of Earth / interceptorSpeed
            // and definitely positive for t = 0, so these are used as bounds.
            float interceptTime = BisectionMethod(delegate (float t) {
                    Vector3 tarPosAtT = targetArc.MoveDistanceFrom(tarPos.normalized, tarSpeed * t);
                    return Geometry.DistanceBetweenPoints(tarPosAtT.normalized, intPos.normalized) - (intSpeed * t);
                }, lowerBound: 0f, upperBound: Geometry.Radius / intSpeed, 0.1f, 100);

            // No solution is found
            if (float.IsNaN(interceptTime)) // TODO: or interceptTime <= 0
            {
                Debug.Log($"[Geoshape] No solution is found for {target.Name()}" +
                    $" trying to intercept {interceptor.Name()}!");
                return target.Position();
            }

            // A solution is found, the position is calculated and converted to normal vector
            float distance = tarSpeed * interceptTime;
            Vector3 interceptionPoint = targetArc.MoveDistanceFrom(tarPos.normalized, distance);
            Debug.Log($"[Geoshape] distance: {distance}, current pos: {tarPos.normalized}, interception pos: {interceptionPoint}, distancebetweenpoints: {Geometry.DistanceBetweenPoints(tarPos.normalized, interceptionPoint.normalized)}");

            Debug.Log($"[Geoshape] {interceptor} is intercepting {target}. interception time: {interceptTime}, distance: {distance}, interception point (geoscape coordinates): {Geometry.NormalToGeoscape(interceptionPoint)}");
            Debug.Log($"[Geoshape] intspeed: {intSpeed}, tarspeed: {tarSpeed}. Distance to intercept for int: {Geometry.DistanceBetweenPoints(intPos.normalized, interceptionPoint.normalized)}, for tar: {Geometry.DistanceBetweenPoints(tarPos.normalized, interceptionPoint.normalized)}");

            return targetArc.MoveDistanceFrom(tarPos.normalized, distance);
        }

        /// <summary>
        /// Finds the solution to function(x) = 0. <paramref name="function"/> must be a
        /// continuous function, and a <paramref name="lowerBound"/> and
        /// <paramref name="upperBound"/> must be given. Continues iterating until the
        /// result has a maximum possible error of <paramref name="tolerance"/>, or until
        /// <paramref name="maxIterations"/> is reached.
        /// </summary>
        /// <returns>The solution, or NaN if no solution has been found</returns>
        private static float BisectionMethod(Func<float, float> function,
            float lowerBound, float upperBound, float tolerance, ushort maxIterations)
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
