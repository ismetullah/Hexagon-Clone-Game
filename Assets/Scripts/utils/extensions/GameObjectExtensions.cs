using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    /**
     * Returns Bounds object for a specific GameObject
     * Calculates children's bounds too
     */
    public static Bounds GetBounds(this GameObject source)
    {
        Bounds bounds;
        Renderer childRender;
        bounds = source.GetRenderBounds();
        if (bounds.extents.x == 0)
        {
            bounds = new Bounds(source.transform.position, Vector3.zero);
            foreach (Transform child in source.transform)
            {
                childRender = child.GetComponent<Renderer>();
                if (childRender)
                {
                    bounds.Encapsulate(childRender.bounds);
                }
                else
                {
                    bounds.Encapsulate(child.gameObject.GetBounds());
                }
            }
        }
        return bounds;
    }

    /**
     * Returns Bounds object for a specific GameObject
     */
    public static Bounds GetRenderBounds(this GameObject source)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer render = source.GetComponent<Renderer>();
        if (render != null)
        {
            return render.bounds;
        }
        return bounds;
    }
}