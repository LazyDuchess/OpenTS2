using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using OpenTS2.Engine;
using System;

namespace OpenTS2.UI
{
    public static class UIUtils
    {

        /// <summary>
        /// Gets a list of string elements separated by a character.
        /// </summary>
        /// <param name="str">String to separate</param>
        /// <param name="separator">Character to separate string by</param>
        /// <returns>List of strings</returns>
        public static List<string> GetCharSeparatedList(string str, char separator)
        {
            var split = str.Split(separator);
            var splitList = new List<string>();
            foreach (var splitString in split)
            {
                if (!string.IsNullOrEmpty(splitString))
                    splitList.Add(splitString);
            }
            return splitList;
        }

        /// <summary>
        /// Returns a Key and Value, given a constant in an UI caption. Format is Key=Value.
        /// </summary>
        /// <param name="str">Constant</param>
        /// <returns>Key and Value. Empty key and value if not valid.</returns>
        public static KeyValuePair<string, string> GetConstant(string str)
        {
            if (str.IndexOf('=') < 0)
                return new KeyValuePair<string, string>("", "");
            var split = str.Split('=');
            var key = split[0];
            var value = split[1];
            return new KeyValuePair<string, string>(key, value);
        }

        // TODO: Clean this up, sucks atm.
        public static Texture2D MakeEdgeImage(Texture2D texture, int width, int height)
        {
            var splitWidth = texture.width / 3;
            var splitHeight = texture.height / 3;
            var hSequence = SplitTextureHorizontalSequence(texture, splitWidth);
            var vSequences0 = SplitTextureVerticalSequence(hSequence[0], splitHeight);
            var vSequences1 = SplitTextureVerticalSequence(hSequence[1], splitHeight);
            var vSequences2 = SplitTextureVerticalSequence(hSequence[2], splitHeight);

            // Don't need horizontal cuts no more.
            foreach(var sequence in hSequence)
                sequence.Free();

            var topLeft = vSequences0[0];
            var centerLeft = vSequences0[1];
            var bottomLeft = vSequences0[2];

            var topCenter = vSequences1[0];
            var center = vSequences1[1];
            var bottomCenter = vSequences1[2];

            var topRight = vSequences2[0];
            var centerRight = vSequences2[1];
            var bottomRight = vSequences2[2];

            var finalEdgeImage = new Texture2D(width, height, texture.format, 1, false);
            finalEdgeImage.name = texture.name;
            finalEdgeImage.filterMode = texture.filterMode;

            // Top Left Corner
            finalEdgeImage.SetPixels(0, 0, splitWidth, splitHeight, topLeft.GetPixels());
            // Bottom Left Corner
            finalEdgeImage.SetPixels(0, height - splitHeight, splitWidth, splitHeight, bottomLeft.GetPixels());
            // Top Right Corner
            finalEdgeImage.SetPixels(width - splitWidth, 0, splitWidth, splitHeight, topRight.GetPixels());
            // Bottom Right Corner
            finalEdgeImage.SetPixels(width - splitWidth, height - splitHeight, splitWidth, splitHeight, bottomRight.GetPixels());

            // Center fill
            FillBlockWithTiledImage(finalEdgeImage, center, splitWidth, splitHeight, width - splitWidth * 2, height - splitHeight * 2);
            // Left Edges
            FillBlockWithTiledImage(finalEdgeImage, centerLeft, 0, splitHeight, splitWidth, height - splitHeight * 2);
            // Right Edges
            FillBlockWithTiledImage(finalEdgeImage, centerRight, width - splitWidth, splitHeight, splitWidth, height - splitHeight * 2);
            // Top Edges
            FillBlockWithTiledImage(finalEdgeImage, topCenter, splitWidth, 0, width - splitWidth * 2, splitHeight);
            // Bottom Edges
            FillBlockWithTiledImage(finalEdgeImage, bottomCenter, splitWidth, height - splitHeight, width - splitWidth * 2, splitHeight);
            //Done
            finalEdgeImage.Apply();

            foreach (var sequence in vSequences0)
                sequence.Free();
            foreach (var sequence in vSequences1)
                sequence.Free();
            foreach (var sequence in vSequences2)
                sequence.Free();

            return finalEdgeImage;

            void FillBlockWithTiledImage(Texture2D target, Texture2D tiledImage, int x, int y, int blockWidth, int blockHeight)
            {
                if (blockWidth <= 0 || blockHeight <= 0)
                    return;
                var amountX = Mathf.CeilToInt((float)blockWidth / tiledImage.width);
                var amountY = Mathf.CeilToInt((float)blockHeight / tiledImage.height);
                var tiledPixels = tiledImage.GetPixels();
                for (var i = 0; i < amountX; i++)
                {
                    for (var j = 0; j < amountY; j++)
                    {
                        var currentPixels = tiledPixels;

                        var currentX = x + (i * tiledImage.width);
                        var currentY = y + (j * tiledImage.height);

                        var currentMaxX = currentX + tiledImage.width - x;
                        var currentMaxY = currentY + tiledImage.height - y;

                        var currentWidth = tiledImage.width;
                        var currentHeight = tiledImage.height;

                        var changed = false;
                        if (currentMaxX > blockWidth)
                        {
                            currentWidth -= currentMaxX - blockWidth;
                            changed = true;
                        }

                        if (currentMaxY > blockHeight)
                        {
                            currentHeight -= currentMaxY - blockHeight;
                            changed = true;
                        }

                        if (changed)
                            currentPixels = tiledImage.GetPixels(0, 0, currentWidth, currentHeight);

                        target.SetPixels(currentX, currentY, currentWidth, currentHeight, currentPixels);
                    }
                }
            }
        }
        public static Texture2D[] SplitTextureVerticalSequence(Texture2D texture, int size)
        {
            var amount = texture.height / size;
            var array = new Texture2D[amount];
            for (var i = 0; i < amount; i++)
            {
                var currentY = i * size;
                var pixels = texture.GetPixels(0, currentY, texture.width, size);
                var newTexture = new Texture2D(texture.width, size, texture.format, 1, false);
                newTexture.SetPixels(0, 0, texture.width, size, pixels);
                newTexture.Apply();
                newTexture.name = texture.name;
                newTexture.filterMode = texture.filterMode;
                array[i] = newTexture;
            }
            return array;
        }
        public static Texture2D[] SplitTextureHorizontalSequence(Texture2D texture, int size)
        {
            var amount = texture.width / size;
            var array = new Texture2D[amount];
            for(var i=0;i<amount;i++)
            {
                var currentX = i * size;
                var pixels = texture.GetPixels(currentX, 0, size, texture.height);
                var newTexture = new Texture2D(size, texture.height, texture.format, 1, false);
                newTexture.SetPixels(0,0,size,texture.height, pixels);
                newTexture.Apply();
                newTexture.name = texture.name;
                newTexture.filterMode = texture.filterMode;
                array[i] = newTexture;
            }
            return array;
        }
    }
}
