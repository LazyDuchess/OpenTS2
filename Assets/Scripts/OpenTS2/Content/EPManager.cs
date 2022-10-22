using OpenTS2.Common.Utils;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class EPManager
    {
        public static EPManager Get()
        {
            return INSTANCE;
        }
        static EPManager INSTANCE;
        //mask of all products, minus store.
        int InstalledProducts = 0x3EFFF;
        public EPManager()
        {
            INSTANCE = this;
        }
        public bool IsEPInstalled(ProductFlags product)
        {
            if (BitUtils.AllBitsSet(InstalledProducts, (int)product))
                return true;
            return false;
        }
        public ProductFlags GetLatestProduct()
        {
            var installedProductList = GetInstalledProducts();
            return installedProductList[installedProductList.Count - 1];
        }
        public List<ProductFlags> GetInstalledProducts()
        {
            var finalList = new List<ProductFlags>();
            var valueList = Enum.GetValues(typeof(ProductFlags)).Cast<ProductFlags>().ToList();
            foreach(var element in valueList)
            {
                if (IsEPInstalled(element))
                    finalList.Add(element);
            }
            return finalList;
        }
    }
}
