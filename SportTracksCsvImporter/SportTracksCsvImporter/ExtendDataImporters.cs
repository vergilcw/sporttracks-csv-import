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

                            ActivityInfoCache infoCache = new ActivityInfoCache();
                            if (infoCache != null)
                            {
                                ActivityInfo activityInfo = infoCache.GetInfo(activity);
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

                            ActivityInfoCache infoCache = new ActivityInfoCache();
                            if (infoCache != null)
                            {
                                ActivityInfo activityInfo = infoCache.GetInfo(activity);
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
