using System.Collections.Generic;

namespace SimpleParser
{
    public class HierarchicalItem
    {
        public string Name { get; set; }
        public TypeEnum Type { get; set; }
        public object Value { get; set; }
        public List<HierarchicalItem> Children { get; set; } = new List<HierarchicalItem>();
    }
}