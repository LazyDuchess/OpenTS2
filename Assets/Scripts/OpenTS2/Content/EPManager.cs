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
            return s_instance;
        }
        static EPManager s_instance;

        public int InstalledProducts
        {
            get
            {
                return _installedProducts;
            }
            set
            {
                _installedProducts = value;
            }
        }

        //mask of all products, minus store.
        int _installedProducts = 0x3EFFF;
        public EPManager()
        {
            s_instance = this;
        }
        public EPManager(int installedProducts) : this()
        {
            InstalledProducts = installedProducts;
        }
        public static List<ProductFlags> GetProductsInMask(int productMask)
        {
            var finalList = new List<ProductFlags>();
            var valueList = Enum.GetValues(typeof(ProductFlags)).Cast<ProductFlags>().ToList();
            foreach (var element in valueList)
            {
                if (BitUtils.AllBitsSet(productMask, (int)element))
                    finalList.Add(element);
            }
            return finalList;
        }
        public bool IsEPInstalled(ProductFlags product)
        {
            if (BitUtils.AllBitsSet(_installedProducts, (int)product))
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
