/*
Copyright (c) 2010 Wolfgang Bruessler

http://code.google.com/p/sporttracks-csv-import/

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;


namespace WbSportTracksCsvImporter
{
    class ExtendDataImporters : IExtendDataImporters
    {
        string LogfileName()
        {
            return Path.GetTempPath() + "WbCsvImport.log";
        }
        void WriteToLogfile(string logline, bool append)
        {
            try
            {
                string logFileName = LogfileName();
                StreamWriter writer = new StreamWriter(logFileName, append);
                writer.WriteLine(logline);
                writer.Close();
            }
            catch
            {
            }
        }

        #region IExtendDataImporters Members

        public void AfterImport(IList added, IList updated)
        {
            foreach (object item in added)
            {
                //WriteToLogfile("Added item", true);
                if (item is IActivity)
                {
                    //WriteToLogfile("Added activity", true);
                    IActivity activity = (IActivity)item;

                    if ((activity!=null) && (activity.UseEnteredData == true) && (activity.ElevationMetersTrack!=null) )
                    {
                        WriteToLogfile("Found activity with elevation track and enabled using entered data", true);
                        if ((float.IsNaN(activity.TotalAscendMetersEntered) || (activity.TotalAscendMetersEntered == 0))
                            && (float.IsNaN(activity.TotalDescendMetersEntered) || (activity.TotalDescendMetersEntered == 0))
                            )
                        {
                            WriteToLogfile("Calculate total descend / ascend meters for added activity", true);

                            float totalAscendMeters = 0;
                            float totalDescendMeters = 0;

                            ActivityInfo activityInfo = ActivityInfoCache.Instance.GetInfo(activity);
                            if (activityInfo != null)
                            {
                                ZoneFiveSoftware.Common.Data.INumericTimeDataSeries elevationTrack = activityInfo.SmoothedElevationTrack;
                                IEnumerator<ZoneFiveSoftware.Common.Data.ITimeValueEntry<float>> iter = elevationTrack.GetEnumerator();

                                if (iter != null)
                                {
                                    WriteToLogfile("Got enumerator", true);

                                    if (iter.Current == null)
                                    {
                                        WriteToLogfile("Move to first element", true);
                                        iter.MoveNext();
                                    }

                                    if (iter.Current != null)
                                    {
                                        float from = iter.Current.Value;

                                        while (iter.MoveNext())
                                        {
                                            if (iter.Current != null)
                                            {
                                                float current = iter.Current.Value;

                                                if (current > from)
                                                {
                                                    totalAscendMeters += current - from;
                                                }
                                                else if (current < from)
                                                {
                                                    totalDescendMeters += current - from;
                                                }

                                                from = current;

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    WriteToLogfile("Could not get enumerator", true);
                                }
                            }

                            activity.TotalAscendMetersEntered = totalAscendMeters;
                            activity.TotalDescendMetersEntered = totalDescendMeters;
                            WriteToLogfile("Done", true);
                        }
                    }
                }
            }
            foreach (object item in updated)
            {
                //WriteToLogfile("Updated item", true);
                if (item is IActivity)
                {
                    //WriteToLogfile("Updated activity", true);

                    IActivity activity = (IActivity)item;

                    if ((activity != null) && (activity.UseEnteredData == true) && (activity.ElevationMetersTrack != null))
                    {
                        WriteToLogfile("Found activity with elevation track and enabled using entered data", true);

                        if ((float.IsNaN(activity.TotalAscendMetersEntered) || (activity.TotalAscendMetersEntered == 0))
                            && (float.IsNaN(activity.TotalDescendMetersEntered) || (activity.TotalDescendMetersEntered == 0))
                            )
                        {
                            WriteToLogfile("Calculate total descend / ascend meters for updated activity", true);

                            float totalAscendMeters = 0;
                            float totalDescendMeters = 0;

                            ActivityInfo activityInfo = ActivityInfoCache.Instance.GetInfo(activity);
                            if (activityInfo != null)
                            {
                                ZoneFiveSoftware.Common.Data.INumericTimeDataSeries elevationTrack = activityInfo.SmoothedElevationTrack;
                                IEnumerator<ZoneFiveSoftware.Common.Data.ITimeValueEntry<float>> iter = elevationTrack.GetEnumerator();

                                if (iter != null)
                                {
                                    WriteToLogfile("Got enumerator", true);

                                    if (iter.Current == null)
                                    {
                                        WriteToLogfile("Move to first element", true);
                                        iter.MoveNext();
                                    }

                                    if (iter.Current != null)
                                    {
                                        float from = iter.Current.Value;

                                        while (iter.MoveNext())
                                        {
                                            if (iter.Current != null)
                                            {
                                                float current = iter.Current.Value;

                                                if (current > from)
                                                {
                                                    totalAscendMeters += current - from;
                                                }
                                                else if (current < from)
                                                {
                                                    totalDescendMeters += current - from;
                                                }

                                                from = current;

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    WriteToLogfile("Could not get enumerator", true);
                                }
                            }
                            
                            activity.TotalAscendMetersEntered = totalAscendMeters;
                            activity.TotalDescendMetersEntered = totalDescendMeters;
                            WriteToLogfile("Done", true);

                        }
                    }
                }
            }
        }

        public void BeforeImport(IList items)
        {
        }

        public IList<IFileImporter> FileImporters
        {
            get
            {
                return new IFileImporter[] { new FileImporter_CSV() };
            }
        }

        #endregion
    }
}
