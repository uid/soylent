using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soylent.Model
{
    public static class WordVersion
    {
        public static double OFFICE_2010 = 14;
        // 13 was skipped due to triskaidekaphobia. Go figure.
        public static double OFFICE_2007 = 12;
        public static double currentVersion
        {
            get
            {
                return double.Parse(Globals.Soylent.Application.Version);
            }
        }
    }
}
