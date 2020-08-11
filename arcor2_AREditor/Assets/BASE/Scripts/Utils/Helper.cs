using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    /// <summary>
    /// Static helper class providing useful methods.
    /// </summary>
    public static class Helper {
        /// <summary>
        /// Method which recursively finds all children of the parent with given tag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="tag"></param>
        /// <param name="forceActive"></param>
        /// <returns></returns>
        public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component {
            if (parent == null) {
                throw new System.ArgumentNullException();
            }
            if (string.IsNullOrEmpty(tag) == true) {
                throw new System.ArgumentNullException();
            }
            List<T> list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
            if (list.Count == 0) {
                return null;
            }

            for (int i = list.Count - 1; i >= 0; i--) {
                if (list[i].CompareTag(tag) == false) {
                    list.RemoveAt(i);
                }
            }
            return list.ToArray();
        }

        public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component {
            if (parent == null) {
                throw new System.ArgumentNullException();
            }
            if (string.IsNullOrEmpty(tag) == true) {
                throw new System.ArgumentNullException();
            }

            T[] list = parent.GetComponentsInChildren<T>(forceActive);
            foreach (T t in list) {
                if (t.CompareTag(tag) == true) {
                    return t;
                }
            }
            return null;
        }
    }

}
