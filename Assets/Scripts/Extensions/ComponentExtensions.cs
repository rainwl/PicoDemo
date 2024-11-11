using System;
using UnityEngine;

namespace Dock
{
    
    public static class ComponentExtensions
    {
        
        public static T EnsureComponent<T>(this Component component) where T : Component
        {
            return EnsureComponent<T>(component.gameObject);
        }

     
        public static T FindAncestorComponent<T>(this Component component, bool includeSelf = true) where T : Component
        {
            return component.transform.FindAncestorComponent<T>(includeSelf);
        }

        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            T foundComponent = gameObject.GetComponent<T>();
            return foundComponent == null ? gameObject.AddComponent<T>() : foundComponent;
        }

        public static Component EnsureComponent(this GameObject gameObject, Type component)
        {
            var foundComponent = gameObject.GetComponent(component);
            return foundComponent == null ? gameObject.AddComponent(component) : foundComponent;
        }
    }
}