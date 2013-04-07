using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcHtmlExtensions
{
    public class EnumEntry<tt>
    {
        public tt Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public EnumEntry()
        {
        
        }
        public EnumEntry(tt value)
        {
            Value = value;
            Name = value.ToString();
            Description = Extensions.GetEnumDescription<tt>(value);
        }
        public static List<EnumEntry<tt>> GetList()
        {
            Type enumType = typeof(tt);
            IEnumerable<tt> values = Enum.GetValues(enumType).Cast<tt>();
            return values.Select(i => new EnumEntry<tt>(i)).ToList();
        }
    }
}
