using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoneywellScanner
{
    public class SymbolCodeProperty
    {
        public enum PropertyType
        {
            EnableProperty,
            MinMaxProperty
        }

        public UInt32 ID;
        public String Description; // readable text on UI.
        public PropertyType Type;

        public SymbolCodeProperty(UInt32 id, String desc, PropertyType type)
        {
            ID = id;
            Description = desc;
            Type = type;
        }

        public bool isEnableProperty()
        {
            return Type == PropertyType.EnableProperty;
        }

        public bool isMinMaxProperty()
        {
            return Type == PropertyType.MinMaxProperty;
        }
    };

    public class SymbolCode
    {
        public String Description; // readable text on UI.
        public List<SymbolCodeProperty> Properties;

        public SymbolCode(String desc)
        {
            Description = desc;
            Properties = new List<SymbolCodeProperty>();
        }
    };
}
