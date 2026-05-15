using UnityEngine;
using System.Collections.Generic;

namespace UniftUI
{
    public static class TextureTracker
    {
        private static HashSet<Texture2D> managedTextures = new HashSet<Texture2D>();
        
        public static void RegisterTexture(Texture2D texture)
        {
            if (texture != null && !managedTextures.Contains(texture))
            {
                managedTextures.Add(texture);
            }
        }
        
        public static void UnregisterAndDestroyTexture(Texture2D texture)
        {
            if (texture != null && managedTextures.Contains(texture))
            {
                managedTextures.Remove(texture);
                Object.Destroy(texture);
            }
        }
        
        public static void CleanupAllTextures()
        {
            foreach (Texture2D texture in managedTextures)
            {
                if (texture != null)
                {
                    Object.Destroy(texture);
                }
            }
            managedTextures.Clear();
        }
    }
}
