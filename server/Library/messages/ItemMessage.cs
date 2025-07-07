using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.messages
{
    public interface IItemMessage
    {

    }
    public class ItemJsonData : IItemMessage
    {
        public ItemJsonData Clone()
        {
            return new ItemJsonData
            {
                ColorItemSn = this.ColorItemSn, 
            };
        }
        public int ColorItemSn { get; set; } = 0;
    }
}
