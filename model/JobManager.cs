using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Soylent.Model
{
    public class JobManager
    {
        private int lastJob = 0;

        public JobManager()
        {
            // Search the active-hits directory to find the most recent job number
            DirectoryInfo activeHits = new DirectoryInfo(TurKit.getRootDirectory() + @"\turkit\active-hits\");
            FileInfo[] hits = activeHits.GetFiles(@"*.data.js");

            foreach (FileInfo file in hits)
            {
                Regex jobnumber = new Regex(@"(shortn|crowdproof|macro).(?<jobNumber>\d{1,}).data.js");
                Match regexResult = jobnumber.Match(file.Name);
                int thisJob = int.Parse(regexResult.Groups["jobNumber"].Value);

                lastJob = Math.Max(thisJob, lastJob);
            }
        }

        /*
        private void setLastJob(int i)
        {
            lastJob = Math.Max(lastJob, i);
        }
         */

        public int generateJobNumber()
        {
            return ++lastJob;
        }
    }
}
