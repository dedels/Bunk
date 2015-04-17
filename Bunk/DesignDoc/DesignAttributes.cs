using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.Design
{

    public abstract class BunkAttribute : System.Attribute
    {
        public string Name { get; protected set; }
    }
    public class ViewAttribute :BunkAttribute
    {
        public ViewAttribute(string viewName=null)
        { this.Name = viewName; }
    }
}
