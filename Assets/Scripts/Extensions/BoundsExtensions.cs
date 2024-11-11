using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dock
{
    public static class BoundsExtensions
    {
        public static float GetScaleToFitInside(this Bounds bounds, Bounds containerBounds)
        {
            var objectSize = bounds.size;
            var containerSize = containerBounds.size;
            Assert.IsTrue(objectSize.x != 0 && objectSize.y != 0 && objectSize.z != 0, "The bounds of the container must not be zero.");
            return Mathf.Min(containerSize.x / objectSize.x, containerSize.y / objectSize.y, containerSize.z / objectSize.z);
        }
    }
}