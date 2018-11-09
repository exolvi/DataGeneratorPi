using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneratorPi
{
    public class SearchResult
    {
        public string ElementName { get; set; }
        public string AttributeName { get; set; }
        public string DataType { get; set; }
        public AFAttribute _self { get; set; }
    }
}
