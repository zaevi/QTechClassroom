using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace QTechClassroom
{
    public static class Captcha
    {
        static Rectangle[] CropRectangles = {
            new Rectangle(6, 3, 13, 17),
            new Rectangle(19, 3, 13, 17),
            new Rectangle(32, 3, 13, 17),
            new Rectangle(45, 3, 13, 17)
        };

        public static string Read(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);
                return Read(new Bitmap(bitmap));
            }
        }

        public static string Read(Bitmap image)
        {
            var set = new HashSet<int>();
            for (int y = 0; y < 20; y++)
                for (int x = 0; x < 60; x++)
                    set.Add((int)(image.GetPixel(x, y).GetBrightness() * 255));
            var level = set.Sum() / (float)set.Count / 255;
            var chars = CropRectangles.Select(c => CaptchaModel.Read(image.CropAndToBitArray(c, level))).ToArray();
            return new string(chars);
        }

        private static BitArray CropAndToBitArray(this Bitmap image, Rectangle rect, float level)
        {
            var bitArray = new BitArray(rect.Height * rect.Width + 7);
            var i = 0;
            foreach (var y in Enumerable.Range(rect.Y, rect.Height))
                foreach (var x in Enumerable.Range(rect.X, rect.Width))
                    bitArray.Set(i++, image.GetPixel(x, y).GetBrightness() < level);

            return bitArray;
        }
    }

    public class CaptchaModel
    {
        #region [Fields] ...
        static readonly string labels = "0123456789ABCDEFHJKLMNRPQSTUVWXYZabdefghjmnqrtwxy";
        static readonly string[] modelsBase64 = {
            "cIA/MAaDYTAMhsEwGAbDYDAG/gAHAAAAAAAAAA==", 
            "cIAP8AEwAAbAABgAA2AADIAB/sE/AAAAAAAAAA==",
            "fMAfCAbAABiAARiAARiAARgA/+AfAAAAAAAAAA==",
            "fMA/CAbAAAz4AD8ADoABMAgHf8AHAAAAAAAAAA==",
            "gAAcgAN4gA2wATNgBv7DfwADYAAMAAAAAAAAAA==",
            "/sE/GAADYAB8gD8ADoABNAgHf8AHAAAAAAAAAA==",
            "8AA/MAQGYADsgX9wHAbDYDAO/gAPAAAAAAAAAA==",
            "/IN/AAyAABiAARAAAyAABsAADIABAAAAAAAAAA==",
            "+IA/MAbGwAnwAT5gDgbDYDgO/oAPAAAAAAAAAA==",
            "eIA/OAaDYTAcB/+AGwADMBAGfoAHAAAAAAAAAA==",
            "cAAOYANsgA04A2NgDP7DfxiMATNgAAAAAAAAAA==",
            "fsAfGANjYAZ8gB8wBobBMBgHf+AHAAAAAAAAAA==",
            "8AF/cAgHYAAMgAEwAAbAAXAI+AEfAAAAAAAAAA==",
            "/sA/GA6DY2AMjIExMAbGYBgO/+APAAAAAAAAAA==",
            "/sE/GAADYAD8gT8wAAbAABgA/+AfAAAAAAAAAA==",
            "/sAfGAADYAAMgB/wAwbAABgAA2AAAAAAAAAAAA==",
            "BsbAGBgDY2D8j/8xMAbGwBgYA2NgAAAAAAAAAA==",
            "DIABMAAGwAAYAANgAAyAATAABsAAHMADGAAAAA==",
            "BsMwGAMzYAZsgAewAWbAGBgGg2FgAAAAAAAAAA==",
            "BsAAGAADYAAMgAEwAAbAABgA/+AfAAAAAAAAAA==",
            "DtiDe3APbmdtrI01m2bTbBpH42gAAQAAAAAAAA==",
            "BsbBeBgPY2NsjJkxNsbG8Bgeg2NgAAAAAAAAAA==",
            "fsAfGAfDYBiMgz/wA2bAGBgGg2FgAAAAAAAAAA==", // R
            "fsAfGAfDYBiMgz/wAQbAABgAA2AAAAAAAAAAAA==", // P
            "8AF/cBwGZ8AMmAEzYAaMwXEc/AEfAA6AB0AAAA==", // Q
            "PMAPGAEDYAA4AB4AB8AAGAgDP8ADAAAAAAAAAA==",
            "/+N/wAAYAANgAAyAATAABsAAGAADAAAAAAAAAA==",
            "BsNgGAyDYTAMhsEwGAbDYDgO/oAPAAAAAAAAAA==",
            "A0ZAGAyDwRgYA2PABtgAG8ABOAAAAAAAAAAAAA==",
            "w2E4GEejaDbNJttm22ybZeM4HIfjAAAAAAAAAA==",
            "A8MwGAZmgAfwAAzAA3iAGRgGwzAwAAAAAAAAAA==",
            "A8MwGAZmwAzwAAyAATAABsAAGAAAAAAAAAAAAA==",
            "/sE/AAZgAAbAAAzAABiAARgA/+AfAAAAAAAAAA==",
            "AAAAAAA8wA+IATDAB/zAGBgD/8AZAAAAAAAAAA==",
            "BsAAGAA74A+cg2EwDIbBMDgHf2AHAAAAAAAAAA==",
            "gAEwAAbcwB+cg2EwDIbBMDgH/oAbAAAAAAAAAA==",
            "AAAAAAAcwAecgTnwB/7AALgCfoAHAAAAAAAAAA==",
            "eIAPMAAf4AMYAANgAAyAATAABsAAAAAAAAAAAA==",
            "AAAAAADcwB+cg2EwDIbBMDgH/oAbAANx4Ad4AA==",
            "BsAAGABzYB8cg2EwDIbBMBgGw2AYAAAAAAAAAA==",
            "BsAAAAADYAAMgAEwAAbAABgAA2AADIABOAADAA==",
            "AAAAAABzbN8dh2EwDIbBMBgGw2AYAAAAAAAAAA==",
            "AAAAAABzYB8cg2EwDIbBMBgGw2AYAAAAAAAAAA==",
            "AAAAAADcwB+cg2EwDIbBMDgH/oAbAANgAAyAAQ==",
            "AAAAAAAf4AMcgAEwAAbAABgAA2AAAAAAAAAAAA==",
            "AMAAGIAP8AEMgAEwAAbAABgAD8ABAAAAAAAAAA==",
            "AAAAAIBxPI5HsW2zbbbNtlk0jsNxAAAAAAAAAA==",
            "AAAAAADDwAyYAR6AAzgADzADZmAYAAAAAAAAAA==",
            "AAAAAIDBMBiMgTEwBGyADaAAHIADMAACYAAMAA==",
        };
        static BitArray[] models = null;
        #endregion

        public static string Labels => labels;
        public static BitArray[] Models
        {
            get
            {
                models = models ?? modelsBase64.Select(b => Base64ToBitArray(b)).ToArray();
                return models;
            }
        }

        private static BitArray Base64ToBitArray(string base64)
            => new BitArray(Convert.FromBase64String(base64));

        private static double GetDistanceRatio(BitArray model, BitArray instance)
        {

            var matched = 0;
            var length = 0;
            for (var i = 0; i < model.Length; i++)
                if (model[i])
                {
                    length++;
                    if (instance[i]) matched++;
                }
            return (double)matched / length;
        }

        public static char Read(BitArray instance)
        {
            var ratios = Models.Select(m => GetDistanceRatio(m, instance)).ToList();
            return Labels[ratios.IndexOf(ratios.Max())];
        }
    }
}
