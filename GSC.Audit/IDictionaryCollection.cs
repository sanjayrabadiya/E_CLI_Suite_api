using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Audit
{
    public interface IDictionaryCollection
    {
        IList<Dictionary> Dictionaries { get; }
    }
}
