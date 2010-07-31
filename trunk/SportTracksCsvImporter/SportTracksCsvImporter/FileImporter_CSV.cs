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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Windows.Forms;


using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Data;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using ZoneFiveSoftware.Common.Data.GPS;


namespace WbSportTracksCsvImporter
{
    class FileImporter_CSV : IFileImporter
    {
        #region IFileImporter Members

        public string FileExtension
        {
            get
            {
                return "csv";
            }
        }

        public string Name
        {
            get
            {
                return "CSV File";
            }
        }

        public Guid Id
        {
            get
            {
                return new Guid("{DA4D08F2-BCE6-4b69-95F6-B821F165200A}");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return Properties.Resources.Image_24_FileCSV;
            }


        }

        #endregion

        #region HelperMethods

        static void MasqueradeUnwantedDelimeters(bool masquerade, char delimeter, ref string s)
        {
            if (masquerade)
            {
                //preserve double quotation marks, replace them with formfeeds
                s = s.Replace("\"\"", "\f");

                bool withinQuotedString = false;
                for (int i = 0; i < s.Length; i++)
                {
                    if (!withinQuotedString)
                    {
                        if (s[i] == '"')
                        {
                            withinQuotedString = true;
                        }
                    }
                    else
                    {
                        if (s[i] == '"')
                        {
                            withinQuotedString = false;
                        }
                        else if (s[i] == delimeter)
                        {
                            char[] sz = s.ToCharArray();
                            sz[i] = '\n';
                            s = new string(sz);
                        }
                    }
                }
                //restore double quotation marks
                s = s.Replace("\f", "\"\"");

            }
            else
            {
                s = s.Replace('\n', delimeter);
            }
        }

        IActivityCategory FindActivityCategory(string categoryName, IEnumerable<IActivityCategory> categoryList)
        {
            //WriteToLogfile("Look for category: " + categoryName, true);

            foreach (IActivityCategory cat in categoryList)
            {
                //WriteToLogfile("Checking " + cat.Name.ToLower(), true);

                if (categoryName.ToLower() == cat.Name.ToLower())
                {
                    //WriteToLogfile("Gotcha!", true);
                    return cat;
                }
            }

            if (categoryName.Contains(":"))
            {
                foreach (IActivityCategory cat in categoryList)
                {
                    string longCatName = cat.Name;

                    IActivityCategory parentCat = cat.Parent;
                    while (parentCat != null)
                    {
                        longCatName = parentCat.Name + ":" + longCatName;
                        parentCat = parentCat.Parent;
                        //WriteToLogfile("Checking " + longCatName.ToLower(), true);

                        if (categoryName.ToLower() == longCatName.ToLower())
                        {
                            //WriteToLogfile("Gotcha!", true);
                            return cat;
                        }
                    }
                }
            }

            //WriteToLogfile("Still not found, check subcatgories ...", true);
            foreach (IActivityCategory cat in categoryList)
            {
                if (cat.SubCategories != null)
                {
                    //WriteToLogfile("Checking subcategories of " + cat.Name, true);
                    IActivityCategory catSlibling = FindActivityCategory(categoryName, cat.SubCategories);

                    if (catSlibling != null)
                    {
                        //WriteToLogfile("Gotcha!", true);
                        return catSlibling;
                    }
                }
            }

            //WriteToLogfile("No matching category found", true);
            return null;
        }

        bool FindEquipment(string equipmentNames, IEnumerable<IEquipmentItem> equipmentList, ICollection<IEquipmentItem> foundEquipment)
        {
            string equipmentNamesToSearch=equipmentNames.ToLower();

            foreach (IEquipmentItem equipmentItem in equipmentList)
            {
                if (equipmentItem.InUse)
                {
                    if (equipmentNamesToSearch.Contains(equipmentItem.Name.ToLower()))
                    {
                        foundEquipment.Add(equipmentItem);
                    }
                    else if (equipmentNamesToSearch.Contains(equipmentItem.Model.ToLower()))
                    {
                        foundEquipment.Add(equipmentItem);
                    }
                }
            }

            return foundEquipment.Count>0;
        }

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

        private TimeSpan GetDecimalDuration(string value, string unit, string defaultUnit)
        {
            TimeSpan duration = TimeSpan.Zero;

            bool unitGiven = false;
            bool defaultToSeconds = false;
            bool defaultToMinutes = false;
            bool defaultToHours = false;
            int countColons = 0;
            int countPoints = 0;
            string sValue = value.Replace(',', '.');

            for (int j = 0; j < sValue.Length; j++)
            {
                switch (sValue[j])
                {
                    case ':':
                        countColons++;
                        break;

                    case '.':
                        countPoints++;
                        break;
                }
            }

            if (sValue.EndsWith("seconds"))
            {
                sValue = sValue.Substring(0, sValue.Length - 7);
                sValue = sValue.Trim();
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("sec"))
            {
                sValue = sValue.Substring(0, sValue.Length - 3);
                sValue = sValue.Trim();
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("sek"))
            {
                sValue = sValue.Substring(0, sValue.Length - 3);
                sValue = sValue.Trim();
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("hours"))
            {
                sValue = sValue.Substring(0, sValue.Length - 5);
                sValue = sValue.Trim();
                defaultToHours = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("hour"))
            {
                sValue = sValue.Substring(0, sValue.Length - 4);
                sValue = sValue.Trim();
                defaultToHours = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("minutes"))
            {
                sValue = sValue.Substring(0, sValue.Length - 7);
                sValue = sValue.Trim();
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("minute"))
            {
                sValue = sValue.Substring(0, sValue.Length - 6);
                sValue = sValue.Trim();
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("hrs"))
            {
                sValue = sValue.Substring(0, sValue.Length - 3);
                sValue = sValue.Trim();
                defaultToHours = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("s"))
            {
                sValue = sValue.Substring(0, sValue.Length - 1);
                sValue = sValue.Trim();
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("min"))
            {
                sValue = sValue.Substring(0, sValue.Length - 3);
                sValue = sValue.Trim();
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("m"))
            {
                sValue = sValue.Substring(0, sValue.Length - 1);
                sValue = sValue.Trim();
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (sValue.EndsWith("h"))
            {
                sValue = sValue.Substring(0, sValue.Length - 1);
                sValue = sValue.Trim();
                defaultToHours = true;
                unitGiven = true;
            }
            else if (unit == "seconds")
            {
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (unit == "sec")
            {
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (unit == "sek")
            {
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (unit == "hours")
            {
                defaultToHours = true;
                unitGiven = true;
            }
            else if (unit == "hour")
            {
                defaultToHours = true;
                unitGiven = true;
            }
            else if (unit == "minutes")
            {
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (unit == "minute")
            {
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (unit == "hrs")
            {
                defaultToHours = true;
                unitGiven = true;
            }
            else if (unit == "s")
            {
                defaultToSeconds = true;
                unitGiven = true;
            }
            else if (unit == "min")
            {
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (unit == "m")
            {
                defaultToMinutes = true;
                unitGiven = true;
            }
            else if (unit == "h")
            {
                defaultToHours = true;
                unitGiven = true;
            }
            else if (defaultUnit == "h")
            {
                defaultToHours = true;
            }
            else if (defaultUnit == "m")
            {
                defaultToMinutes = true;
            }
            else if (defaultUnit == "s")
            {
                defaultToSeconds = true;
            }
            else
            {
                defaultToSeconds = true;
            }

            if ((countColons == 0) && (countPoints == 0))
            {
                if (defaultToHours)
                {
                    int iHours = System.Convert.ToInt32(sValue);
                    duration = new TimeSpan(iHours, 0, 0);
                }
                else if (defaultToSeconds)
                {
                    int iSeconds = System.Convert.ToInt32(sValue);
                    duration = new TimeSpan(0, 0, iSeconds);
                }
                else if (defaultToMinutes)
                {
                    int iMinutes = System.Convert.ToInt32(sValue);
                    duration = new TimeSpan(0, iMinutes, 0);
                }
            }
            else if ((countColons == 0) && (countPoints == 1))
            {
                if (unitGiven)
                {
                    if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Length > 0 )
                    {
                        string decimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                        if (sValue.IndexOf(decimalSeparator) == -1)
                        {
                            if (decimalSeparator == ".")
                            {
                                sValue = sValue.Replace(',', '.');
                            }
                            else if (decimalSeparator == ",")
                            {
                                sValue = sValue.Replace('.', ',');
                            }
                        }
                    }

                    double dValue = System.Convert.ToDouble(sValue);
                    double dIntegral = System.Math.Truncate(dValue);
                    double dFraction = dValue - dIntegral;

                    if (defaultToSeconds)
                    {
                        int iSeconds = System.Convert.ToInt32(dIntegral);
                        int iMilliSeconds = System.Convert.ToInt32(System.Math.Truncate(dFraction * 1000.0));
                        duration = new TimeSpan(0, 0, 0, iSeconds, iMilliSeconds);
                    }
                    else if (defaultToHours)
                    {
                        int iHours = System.Convert.ToInt32(dIntegral);
                        double dMinutes = dFraction * 60.0;
                        double dMinutesIntegral = System.Math.Truncate(dMinutes);
                        double dMinutesFraction = dMinutes - dMinutesIntegral;
                        int iMinutes = System.Convert.ToInt32(dMinutesIntegral);
                        double dSeconds = dMinutesFraction * 60.0;
                        double dSecondsIntegral = System.Math.Truncate(dSeconds);
                        double dSecondsFraction = dSeconds - dSecondsIntegral;
                        int iSeconds = System.Convert.ToInt32(dSecondsIntegral);
                        int iMilliSeconds = System.Convert.ToInt32(System.Math.Truncate(dSecondsFraction * 1000.0));

                        duration = new TimeSpan(0, iHours, iMinutes, iSeconds, iMilliSeconds);
                    }
                    else
                    {
                        int iMinutes = System.Convert.ToInt32(dIntegral);
                        double dSeconds = dFraction * 60.0;
                        double dSecondsIntegral = System.Math.Truncate(dSeconds);
                        double dSecondsFraction = dSeconds - dSecondsIntegral;
                        int iSeconds = System.Convert.ToInt32(dSecondsIntegral);
                        int iMilliSeconds = System.Convert.ToInt32(System.Math.Truncate(dSecondsFraction * 1000.0));

                        duration = new TimeSpan(0, 0, iMinutes, iSeconds, iMilliSeconds);
                    }

                }
                else
                {
                    sValue = "0:0:" + sValue;
                    duration = TimeSpan.Parse(sValue);
                }
            }
            else if (countColons == 1)
            {
                //one colon given, ignore unit
                //add 0 hours and parsing should work
                sValue = "0:" + sValue;
                duration = TimeSpan.Parse(sValue);
            }
            else
            {
                //obviously more than one colon given, ignore unit
                //parsing should work
                duration = TimeSpan.Parse(sValue);
            }

            return duration;
        }

        private double GetDecimalDistanceAsMeters(string value, string unit, double defaultFactor)
        {
            double distance = float.NaN;

            string sValue = value.ToLower();
            sValue = sValue.Trim();

            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Length > 0)
            {
                string decimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                if (sValue.IndexOf(decimalSeparator) == -1)
                {
                    if (decimalSeparator == ".")
                    {
                        sValue = sValue.Replace(',', '.');
                    }
                    else if (decimalSeparator == ",")
                    {
                        sValue = sValue.Replace('.', ',');
                    }
                }
            }

            if (sValue.EndsWith("km"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                distance = temp * 1000.0;
            }
            else if (sValue.EndsWith("k"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 1));
                distance = temp * 1000.0;
            }
            else if (sValue.EndsWith("meters"))
            {
                distance = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 6));
            }
            else if (sValue.EndsWith("meter"))
            {
                distance = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 5));
            }
            else if (sValue.EndsWith("mil"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 3));
                distance = temp * 1609.344;
            }
            else if (sValue.EndsWith("mile"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 4));
                distance = temp * 1609.344;
            }
            else if (sValue.EndsWith("miles"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 5));
                distance = temp * 1609.344;
            }
            else if (sValue.EndsWith("sm"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                distance = temp * 1609.344;
            }
            else if (sValue.EndsWith("nm"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                distance = temp * 1852.0;
            }
            else if (sValue.EndsWith("mi"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                distance = temp * 1852.0;
            }
            else if (sValue.EndsWith("m"))
            {
                distance = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 1));
            }
            else if (sValue.EndsWith("yard"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 4));
                distance = temp * 0.9144;
            }
            else if (sValue.EndsWith("yards"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 5));
                distance = temp * 0.9144;
            }
            else if (sValue.EndsWith("yd"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                distance = temp * 0.9144;
            }
            else if (sValue.EndsWith("foot"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 4));
                distance = temp * 0.3048;
            }
            else if (sValue.EndsWith("feet"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 4));
                distance = temp * 0.3048;
            }
            else if (sValue.EndsWith("ft"))
            {
                double temp = System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                distance = temp * 0.3048;
            }
            else if (unit == "km")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1000.0;
            }
            else if (unit == "k")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1000.0;
            }
            else if (unit == "meters")
            {
                distance = System.Convert.ToDouble(sValue);
            }
            else if (unit == "meter")
            {
                distance = System.Convert.ToDouble(sValue);
            }
            else if (unit == "mil")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1609.344;
            }
            else if (unit == "mile")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1609.344;
            }
            else if (unit == "miles")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1609.344;
            }
            else if (unit == "sm")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1609.344;
            }
            else if (unit == "nm")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1852.0;
            }
            else if (unit == "mi")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 1852.0;
            }
            else if (unit == "m")
            {
                distance = System.Convert.ToDouble(sValue);
            }
            else if (unit == "yard")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 0.9144;
            }
            else if (unit == "yards")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 0.9144;
            }
            else if (unit == "yd")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 0.9144;
            }
            else if (unit == "foot")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 0.3048;
            }
            else if (unit == "feet")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 0.3048;
            }
            else if (unit == "ft")
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * 0.3048;
            }
            else
            {
                double temp = System.Convert.ToDouble(sValue);
                distance = temp * defaultFactor;
            }

            distance=Math.Truncate(distance);

            return distance;
        }

        private float GetDecimalWeight(string value, string unit)
        {
            float weight = float.NaN;
            string sValue = value.ToLower();
            sValue = sValue.Trim();

            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Length > 0)
            {
                string decimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                if (sValue.IndexOf(decimalSeparator) == -1)
                {
                    if (decimalSeparator == ".")
                    {
                        sValue = sValue.Replace(',', '.');
                    }
                    else if (decimalSeparator == ",")
                    {
                        sValue = sValue.Replace('.', ',');
                    }
                }
            }

            if (sValue.EndsWith("kg"))
            {
                float temp = (float)System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 2));
                weight = temp;
            }
            else if (sValue.EndsWith("st"))
            {
                float temp = (float)System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 1));
                weight = temp * (float)6.35;
            }
            else if (sValue.EndsWith("lb"))
            {
                float temp = (float)System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 1));
                weight = temp * (float)0.454;
            }
            else if (unit == "kg")
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                weight = temp;
            }
            else if (unit == "st")
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                weight = temp * (float)6.35;
            }
            else if (unit == "lb")
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                weight = temp * (float)0.454;
            }
            else
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                weight = temp;
            }

            return weight;
        }

        private float GetDecimalEnergy(string value, string unit, float defaultFactor)
        {
            float calories = float.NaN;

            string sValue = value.ToLower();
            sValue = sValue.Trim();

            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Length > 0)
            {
                string decimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                if (sValue.IndexOf(decimalSeparator) == -1)
                {
                    if (decimalSeparator == ".")
                    {
                        sValue = sValue.Replace(',', '.');
                    }
                    else if (decimalSeparator == ",")
                    {
                        sValue = sValue.Replace('.', ',');
                    }
                }
            }

            if (sValue.EndsWith("kjoule"))
            {
                float temp = (float)System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 6));
                calories = temp * (float)4.1868;
            }
            else if (sValue.EndsWith("kcal"))
            {
                float temp = (float)System.Convert.ToDouble(sValue.Substring(0, sValue.Length - 4));
                calories = temp;
            }
            else if (unit == "kjoule")
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                calories = temp * (float)4.1868;
            }
            else if (unit == "kcal")
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                calories = temp;
            }
            else
            {
                float temp = (float)System.Convert.ToDouble(sValue);
                calories = temp * (float)defaultFactor;
            }

            return calories;
        }

        private float GetDecimalValue(string value, string[] units)
        {
            string sValue = value.ToLower();
            sValue = sValue.Trim();

            if (units != null)
            {
                for (int i = 0; i < units.GetLength(0); i++)
                {
                    string unit = units[i];

                    if (sValue.EndsWith(unit))
                    {
                        sValue = sValue.Substring(0, sValue.Length - unit.Length);
                        sValue = sValue.Trim();
                        break;
                    }
                }
            }

            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Length > 0)
            {
                string decimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                if (sValue.IndexOf(decimalSeparator) == -1)
                {
                    if (decimalSeparator == ".")
                    {
                        sValue = sValue.Replace(',', '.');
                    }
                    else if (decimalSeparator == ",")
                    {
                        sValue = sValue.Replace('.', ',');
                    }
                }
            }

            float temp = (float)System.Convert.ToDouble(sValue);

            return temp;
        }


        class Pause
        {
            public Pause(TimeSpan begin, TimeSpan end, TimeSpan dur)
            {
                beginPause = begin;
                endPause = end;
                duration = dur;
            }
            public TimeSpan beginPause;
            public TimeSpan endPause;
            public TimeSpan duration;
        }

        class GpsElement
        {
            public GpsElement()
            {
                elapsedtime=TimeSpan.Zero;
                distance=0;
                speed=0;
                isPause=false;
            }

            public TimeSpan elapsedtime;
            public float distance;
            public float speed;
            public bool isPause;
        }

        class IBikeElement
        {
            public IBikeElement()
            {
                elapsedtime=TimeSpan.Zero;
                distance=0;
                speed=0;
                power=0;
                cadence=0;
                heartrate=0;
                elevation=0;
                newLap = false;
                needSync = false;
                insertZeroCadenceBefore = false;
            }

            public TimeSpan elapsedtime;
            public float distance;
            public float speed;
            public float power;
            public float cadence;
            public float heartrate;
            public float elevation;
            public bool newLap;
            public bool needSync;
            public bool insertZeroCadenceBefore;
        }

        bool GetPausedTimes(IGPSRoute route, TimeSpan expectedPausedTime, out Pause[] pausesArray, out GpsElement[] gpsElementsArray)
        {
            List<Pause> pauses=new List<Pause>();
            List<GpsElement> gpsElements = new List<GpsElement>();
            bool retVal = false;

            float speedThresholdMax = 20;
            float speedThresholdMin = 0.1F;
            int iterationCount = 0;

            bool iterating = false;
            TimeSpan limit = new TimeSpan(0, 0, 10);

            do
            {
                iterating = false;
                pauses.Clear();
                gpsElements.Clear();

                WriteToLogfile("Get enumerator", true);
                IEnumerator<ITimeValueEntry<IGPSPoint>> iter = route.GetEnumerator();

                if (iter != null)
                {
                    WriteToLogfile("Got enumerator", true);

                    TimeSpan totalStopped = TimeSpan.Zero;
                    float speedThreshold = (speedThresholdMax + speedThresholdMin) / 2;
                    iterationCount++;

                    if (iter.Current == null)
                    {
                        WriteToLogfile("Move to first element", true);
                        iter.MoveNext();
                    }

                    ITimeValueEntry<IGPSPoint> from = iter.Current;
                    ITimeValueEntry<IGPSPoint> stopped = iter.Current;

                    if (from != null)
                    {
                        int stoppedPoints = 0;
                        int count = 1;

                        GpsElement elementFrom = new GpsElement();

                        elementFrom.elapsedtime = new TimeSpan(0, 0, (int)from.ElapsedSeconds);
                        elementFrom.distance = 0;
                        elementFrom.speed = 0;
                        elementFrom.isPause = true;
                        gpsElements.Add(elementFrom);

                        while (iter.MoveNext())
                        {
                            if (iter.Current != null)
                            {
                                count++;

                                IGPSPoint fromPoint = from.Value;
                                IGPSPoint stoppedPoint = stopped.Value;
                                IGPSPoint toPoint = iter.Current.Value;

                                if (fromPoint != null && toPoint != null)
                                {
                                    float delta = toPoint.DistanceMetersToPoint(fromPoint);
                                    TimeSpan deltaTime = new TimeSpan(0, 0, (int)(iter.Current.ElapsedSeconds - from.ElapsedSeconds));
                                    float speed = delta / (float)deltaTime.TotalSeconds * 3600 / 1000;

                                    GpsElement elementTo = new GpsElement();
                                    elementTo.elapsedtime = new TimeSpan(0, 0, (int)iter.Current.ElapsedSeconds);
                                    elementTo.distance = elementFrom.distance + delta;
                                    elementTo.speed = speed;
                                    elementTo.isPause = false;
                                    gpsElements.Add(elementTo);

                                    if (speed > speedThreshold)
                                    {
                                        if (stoppedPoints > 0)
                                        {
                                            TimeSpan stoppedTime = new TimeSpan(0, 0, (int)(iter.Current.ElapsedSeconds - stopped.ElapsedSeconds));
                                            totalStopped += stoppedTime;
                                            TimeSpan fromTime = new TimeSpan(0, 0, (int)stopped.ElapsedSeconds);
                                            TimeSpan toTime = new TimeSpan(0, 0, (int)iter.Current.ElapsedSeconds);
                                            //WriteToLogfile(string.Format("{0} points with slow speed, stopped for {1} seconds from {2} to {3}",stoppedPoints, stoppedTime.TotalSeconds,fromTime, toTime), true);

                                            pauses.Add(new Pause(fromTime, toTime, stoppedTime));
                                        }
                                        elementFrom = elementTo;
                                        from = iter.Current;
                                        stopped = iter.Current;
                                        stoppedPoints = 0;
                                    }
                                    else
                                    {
                                        elementTo.isPause = true;
                                        elementFrom = elementTo;
                                        from = iter.Current;
                                        stoppedPoints++;
                                    }
                                }
                                else
                                {
                                    WriteToLogfile("Could not get values", true);
                                }
                            }
                            else
                            {
                                WriteToLogfile("Could not get current element", true);
                            }
                        }

                        WriteToLogfile(string.Format("Analyzed track with {0} points", count), true);
                        WriteToLogfile(string.Format("Stopped for {0} seconds", totalStopped.TotalSeconds), true);
                    }
                    else
                    {
                        WriteToLogfile("Could not get first element", true);
                    }

                    WriteToLogfile(string.Format("Iteration {0}, [{1} - {2}], threshold {3}, expected pause: {4}, calculated pause: {5}",
                        iterationCount, speedThresholdMin, speedThresholdMax, speedThreshold, expectedPausedTime, totalStopped), true);

                    TimeSpan diff = expectedPausedTime - totalStopped;

                    //WriteToLogfile("Pause difference from expected value: " + diff, true);

                    if (totalStopped > expectedPausedTime)
                    {
                        speedThresholdMax = speedThreshold;
                        iterating = true;
                    }
                    else
                    {
                        if (diff < limit)
                        {
                            iterating = false;
                            retVal = true;
                            WriteToLogfile("Iteration completed", true);
                        }
                        else
                        {
                            speedThresholdMin = speedThreshold;
                            iterating = true;
                        }
                    }

                    //0,6092831 - 0,6092832
                    if (speedThresholdMax - speedThresholdMin < 0.0000100)
                    {
                        iterating = false;
                        retVal = true;
                        WriteToLogfile("Iteration completed", true);
                    }
                }
                else
                {
                    WriteToLogfile("Could not get enumerator", true);
                }


                if ((iterationCount > 1000) && iterating)
                {
                    iterating = false;
                    WriteToLogfile("Too many iterations, stop here", true);
                }
            }
            while (iterating);

            pausesArray = pauses.ToArray();
            gpsElementsArray = gpsElements.ToArray();

            return retVal;
        }


        #endregion

        #region IDataImporter Members

        public bool Import(string configurationInfo, IJobMonitor monitor, IImportResults importResults)
        {
            IApplication Application = WbSportTracksCsvImporter.Plugin.GetApplication();

            bool resultCode = true;
            string filename = configurationInfo;
            StreamReader file = null;
            int totalLines = 0;
            int lineWithMaxKeywords = 0;
            int maxKeywordCount = 0;
            int linesToIgnore = 0;
            int currentLine = 0;
            int importedLines = 0;

            System.Globalization.DateTimeFormatInfo systemDateTimeformat = System.Globalization.DateTimeFormatInfo.CurrentInfo;
            System.Globalization.DateTimeFormatInfo usDateTimeformat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
            System.Globalization.DateTimeFormatInfo deDateTimeformat = new System.Globalization.CultureInfo("de-DE", false).DateTimeFormat;

            System.Globalization.DateTimeFormatInfo chosenDateTimeformat = systemDateTimeformat;
            System.Globalization.DateTimeFormatInfo chosenDateformat = systemDateTimeformat;
            System.Globalization.DateTimeFormatInfo chosenTimeformat = systemDateTimeformat;

            bool bSystemDateFormatok = true;
            bool bSystemTimeFormatok = true;
            bool bSystemDateTimeFormatok = true;
            bool bUsDateFormatok = true;
            bool bUsTimeFormatok = true;
            bool bUsDateTimeFormatok = true;
            bool bDeDateFormatok = true;
            bool bDeTimeFormatok = true;
            bool bDeDateTimeFormatok = true;
            bool bContainsTrackData = false;
            bool bContainsGpsData = false;

            bool bIsIbikeFile = false;
            bool bIsrowpro3File = false;
            bool bCalculateIBikePauseFromGpsTrack = false;
            int originalGpsTrackTotalSeconds = 0;
            IActivity mergeIBikeDestinationActivity = null;
            Pause[] pausesFromGpsTrack = null;
            GpsElement[] elementsFromGpsTrack = null;
            IBikeElement[] elementsFromIBike = null;
            List<IBikeElement> elementsFromIBikeList = new List<IBikeElement>();
            TimeSpan iBikeCurrentDuration = TimeSpan.Zero;
            DateTime iBikeTimeStampStart = DateTime.Now;
            int iBikeTimeInterval = 1; //seconds per data line
            //bool bIsDaumErgometerFile = false;
            double distanceLastValue = double.NaN;
            float elevationLastValue = float.NaN;

            DateTime fileDateTime = DateTime.Now;


            WriteToLogfile("Import: " + filename, false);

            try
            {
                fileDateTime=File.GetLastWriteTime(filename);
                WriteToLogfile("Date from file information: " + fileDateTime.ToString(), true);
            }
            catch(Exception)
            {
            }







            try
            {
                string filenameShort = Path.GetFileName(filename);
                char[] daumFilenameDelimiterChars = { '-' };
                char[] daumDateTimeDelimiterChars = { ' ' };
                string[] daumFilenameParts = filenameShort.Split(daumFilenameDelimiterChars);
                string[] daumDateParts = filenameShort.Split(daumFilenameDelimiterChars);

                if (daumFilenameParts.GetLength(0) > 4)
                {
                    daumDateParts = daumFilenameParts[1].Trim().Split(daumDateTimeDelimiterChars);

                    if (daumDateParts.GetLength(0) == 2)
                    {
                        string daumDateString = daumDateParts[0].Replace('_', '.').Trim();
                        string daumTimeString = daumDateParts[1].Replace('_', ':').Trim();
                        WriteToLogfile("Date string from Daum Ergometer file name: " + daumDateString, true);
                        WriteToLogfile("Time string from Daum Ergometer file name: " + daumTimeString, true);
                        DateTime daumDate = DateTime.Parse(daumDateString, deDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                        DateTime daumTime = DateTime.Parse(daumTimeString, deDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);

                        fileDateTime = new DateTime(daumDate.Year, daumDate.Month, daumDate.Day, daumTime.Hour, daumTime.Minute, daumTime.Second);
                        WriteToLogfile("Date from Daum Ergometer file name: " + fileDateTime.ToString(), true);
                    }
                }
            }
            catch (Exception)
            {
            }





            WriteToLogfile("Short date format of current locale: " + systemDateTimeformat.ShortDatePattern, true);
            WriteToLogfile("Short time format of current locale: " + systemDateTimeformat.ShortTimePattern, true);

            monitor.StatusText = Properties.Resources.ID_CountingLines;

            try
            {
                file = File.OpenText(filename);

                string lineDummy = file.ReadLine();

                while (lineDummy != null)
                {
                    totalLines++;
                    lineDummy = file.ReadLine();
                    
                    if (monitor.Cancelled)
                    {
                        break;
                    }
                    
                }
            }
            catch(Exception e)
            {
                WriteToLogfile("Could not import file: " + e.Message, true);
                monitor.ErrorText = Properties.Resources.ID_FileOpen + Properties.Resources.ID_ReferToLogFile + LogfileName();
                resultCode = false;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            WriteToLogfile("Check header", true);
            monitor.StatusText = Properties.Resources.ID_CheckingHeader;

            try
            {
                string[] keywords = new string[]{"date", "entrydate", "time", "elapsed time", "datetime", "timestamp", "rowdate", "distance", "totaldistance", "duration", "tottime",
                                               "power", "watt", "averagewatts", "avgwatts", "cadence", "rpm", "calories", "cals", "output", "output", "kjoule", "intensity", 
                                               "location", "activity", "category", "subcategory", "type", "name", "description", "comment", "hrm", "heartrate", "pulse", "puls", "heartaverage", "endhr", "details", "comments",
                                               "equipment", 
                                               "elevation", "latitude", "longitude", "climbed", "lap marker", "lap",
                                               "weight", "bodyweight", "bmi","restingheartrate", "resthr", "maxheartrate", "heartmax", "athletemaxheartrate", "athleteheartmax", "diary", 
                                               "systolic", "diastolic", "bodyfat", "sleep", "mood", "feeling"};

                file = File.OpenText(filename);

                string lineDummy = file.ReadLine();

                while (lineDummy != null)
                {
                    currentLine++;

                    if (currentLine == 1)
                    {
                        if (lineDummy.IndexOf("iBike", 0) == 0)
                        {
                            //iBike,9,english
                            bIsIbikeFile = true;
                            WriteToLogfile("iBike file", true);
                        }
                        else if (lineDummy.IndexOf("ergo_bike", 0) == 0)
                        {
                            //ergo_bike 2003 training data;;;;;;;
                            //bIsDaumErgometerFile = true;
                            WriteToLogfile("Daum ergometer file", true);
                        }
                    }
                    if (currentLine == 2)
                    {
                        if (bIsIbikeFile)
                        {
                            //2008,8,28,5,17,50
                            string[] timeStampElements=lineDummy.Split(',');
                            if (timeStampElements.GetLength(0) == 6)
                            {
                                try
                                {
                                    iBikeTimeStampStart = new DateTime(System.Convert.ToInt32(timeStampElements[0]),
                                                                System.Convert.ToInt32(timeStampElements[1]),
                                                                System.Convert.ToInt32(timeStampElements[2]),
                                                                System.Convert.ToInt32(timeStampElements[3]),
                                                                System.Convert.ToInt32(timeStampElements[4]),
                                                                System.Convert.ToInt32(timeStampElements[5]), DateTimeKind.Local);
                                    WriteToLogfile("iBike file timestamp: " + iBikeTimeStampStart.ToLocalTime().ToString(), true);
                                }
                                catch (Exception)
                                {
                                    bIsIbikeFile = false;
                                }
                            }
                            else
                            {
                                bIsIbikeFile = false;
                            }
                        }
                    }
                    if (currentLine == 4)
                    {
                        if (bIsIbikeFile)
                        {
                            //222.000000,660.699424,0.350000,10.279000,1.000000,28.000000, ...
                            string[] settingsElements = lineDummy.Split(',');
                            if (settingsElements.GetLength(0) >= 5)
                            {
                                string siBikeTimeInterval = settingsElements[4];
                                WriteToLogfile("iBike file, time interval settings: " + siBikeTimeInterval, true);
                                try
                                {
                                    siBikeTimeInterval = siBikeTimeInterval.Substring(0, 1);
                                    iBikeTimeInterval = System.Convert.ToInt32(siBikeTimeInterval);
                                    WriteToLogfile(string.Format("iBike file, {0} seconds per line", iBikeTimeInterval), true);
                                }
                                catch (Exception)
                                {
                                    WriteToLogfile(string.Format("iBike file, number configuration failed, use default {0} seconds per line", iBikeTimeInterval), true);
                                }
                            }
                            else
                            {
                                WriteToLogfile(string.Format("iBike file, could not get configuration, use default {0} seconds per line", iBikeTimeInterval), true);
                            }
                        }
                    }

                    lineDummy = lineDummy.ToLower();
                    int iFoundKeywords = 0;

                    for(int iKeyWord=0; iKeyWord<keywords.GetLength(0); iKeyWord++)
                    {
                        if(lineDummy.IndexOf(keywords[iKeyWord])>=0)
                        {
                            iFoundKeywords++;
                        }
                    }

                    if (iFoundKeywords > maxKeywordCount)
                    {
                        WriteToLogfile(lineDummy, true);
                        lineWithMaxKeywords = currentLine;
                        maxKeywordCount = iFoundKeywords;
                    }

                    if (lineDummy.Contains("rowfile_id"))
                    {
                        WriteToLogfile("rowpro3 export for conceptrower2", true);
                        WriteToLogfile(lineDummy, true);
                        lineWithMaxKeywords = currentLine;
                        maxKeywordCount = iFoundKeywords;
                        bIsrowpro3File = true;
                    }

                    
                    if (monitor.Cancelled)
                    {
                        break;
                    }
                    

                    if (currentLine > 50)
                    {
                        break;
                    }

                    lineDummy = file.ReadLine();
                }
            }
            catch (Exception e)
            {
                WriteToLogfile("Could not import file: " + e.Message, true);
                monitor.ErrorText = Properties.Resources.ID_FileOpen + Properties.Resources.ID_ReferToLogFile + LogfileName();
                resultCode = false;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            linesToIgnore = lineWithMaxKeywords - 1;
            WriteToLogfile("Header lines to be ignored: "+linesToIgnore, true);


            if (monitor.Cancelled)
            {
                WriteToLogfile("Import cancelled by user", true);
                resultCode = false;
            }

            if (resultCode)
            {
                DateTime dateFirstLine = DateTime.Now;
                DateTime dateAndTimeFirstLine = DateTime.Now;
                int sameDate = 0;
                int sameDateAndTime = 0;

                if (bIsIbikeFile)
                {
                    WriteToLogfile("iBike file -> look for existing activity with GPS data", true);
                    for(int i=0; i<Application.Logbook.Activities.Count; i++)
                    {
                        IActivity activity = Application.Logbook.Activities[i];
                        if ((activity.GPSRoute!=null) && (activity.StartTime.Date == iBikeTimeStampStart.Date))
                        {
                            TimeSpan trackDuration = new TimeSpan(0,0,(int)activity.GPSRoute.TotalElapsedSeconds);
                            DialogResult res = MessageDialog.Show(string.Format(Properties.Resources.ID_UseExistingGpsTrack, activity.StartTime.ToLocalTime(), activity.TotalDistanceMetersEntered / 1000.0, trackDuration.ToString()), Plugin.GetName(), MessageBoxButtons.YesNo);

                            if (res==DialogResult.Yes)
                            {
                                WriteToLogfile("Use distance meters track of existing activity", true);
                                //mergeIBikeDestinationActivity = activity;
                                iBikeTimeStampStart = activity.StartTime.ToLocalTime();
                                bCalculateIBikePauseFromGpsTrack = true;

                                int iBikeTotalSeconds = iBikeTimeInterval * (totalLines - linesToIgnore);
                                int expectedPausedSeconds = (int)activity.GPSRoute.TotalElapsedSeconds - iBikeTotalSeconds;
                                originalGpsTrackTotalSeconds = (int)activity.GPSRoute.TotalElapsedSeconds;

                                if(expectedPausedSeconds>0)
                                {
                                    //WriteToLogfile("GPS track is longer, expected pause in iBike data: " + expectedPausedSeconds, true);
                                    TimeSpan expectedPause = new TimeSpan(0, 0, expectedPausedSeconds);
                                    monitor.StatusText = Properties.Resources.ID_GettingPauseTimeFromGpsTrack;
                                    GetPausedTimes(activity.GPSRoute, expectedPause, out pausesFromGpsTrack, out elementsFromGpsTrack);
                                }
                                break;
                            }
                        }
                    }
                }


                for (int iImportStep = 0; iImportStep < 3; iImportStep++)
                {
                    bool bCheckingFormat = false;
                    bool bCheckingTrack = false;
                    bool bImporting = false;

                    currentLine = 0;
                    iBikeCurrentDuration = TimeSpan.Zero;

                    double heartRateAverageTotal = 0;
                    int heartRateCount = 0;
                    float heartRateAverageMax = 0;
                    double powerTotal = 0;
                    float powerMax = 0;
                    int powerCount = 0;
                    double cadenceTotal = 0;
                    float cadenceMax = 0;
                    int cadenceCount = 0;
                    float totalAscendMeters = 0;
                    float totalDescendMeters = 0;



                    IActivity activityTrack = null;

                    if (iImportStep == 0)
                    {
                        bCheckingFormat = true;
                    }
                    if (iImportStep > 0)
                    {
                        if (bSystemDateFormatok)
                        {
                            chosenDateformat = systemDateTimeformat;
                            WriteToLogfile("Use system date format: " + chosenDateformat.ShortDatePattern, true);
                        }
                        else if (bUsDateFormatok)
                        {
                            chosenDateformat = usDateTimeformat;
                            WriteToLogfile("Use US date format: " + chosenDateformat.ShortDatePattern, true);
                        }
                        else if (bDeDateFormatok)
                        {
                            chosenDateformat = deDateTimeformat;
                            WriteToLogfile("Use DE date format: " + chosenDateformat.ShortDatePattern, true);
                        }

                        if (bSystemTimeFormatok)
                        {
                            chosenTimeformat = systemDateTimeformat;
                            WriteToLogfile("Use system time format: " + chosenDateformat.ShortTimePattern, true);
                        }
                        else if (bUsTimeFormatok)
                        {
                            chosenTimeformat = usDateTimeformat;
                            WriteToLogfile("Use US time format: " + chosenDateformat.ShortTimePattern, true);
                        }
                        else if (bDeTimeFormatok)
                        {
                            chosenTimeformat = deDateTimeformat;
                            WriteToLogfile("Use DE time format: " + chosenDateformat.ShortTimePattern, true);
                        }

                        if (bSystemDateTimeFormatok)
                        {
                            chosenDateTimeformat = systemDateTimeformat;
                            WriteToLogfile("Use system datetime format: " + chosenDateformat.FullDateTimePattern, true);
                        }
                        else if (bUsDateTimeFormatok)
                        {
                            chosenDateTimeformat = usDateTimeformat;
                            WriteToLogfile("Use US datetime format: " + chosenDateformat.FullDateTimePattern, true);
                        }
                        else if (bDeDateTimeFormatok)
                        {
                            chosenDateTimeformat = deDateTimeformat;
                            WriteToLogfile("Use DE datetime format: " + chosenDateformat.FullDateTimePattern, true);
                        }
                    }
                    if (iImportStep == 1)
                    {
                        bCheckingTrack = true;
                    }

                    if (iImportStep == 2)
                    {
                        if (sameDate > 5)
                        {
                            if (!bContainsTrackData)
                            {
                                bContainsTrackData = true;
                                WriteToLogfile("Found multiple details for same date, assume track data", true);
                            }
                        }
                        if (sameDateAndTime > 5)
                        {
                            bContainsTrackData = true;
                            if (!bContainsTrackData)
                            {
                                bContainsTrackData = true;
                                WriteToLogfile("Found multiple details for same timestamp, assume track data", true);
                            }
                        }

                        bImporting = true;
                    }
                    

                    try
                    {
                        if (bCheckingFormat)
                        {
                            monitor.StatusText = Properties.Resources.ID_CheckingDateFormat;
                        }
                        if (bCheckingTrack)
                        {
                            monitor.StatusText = Properties.Resources.ID_CheckingActivityType;
                        }
                        if (bImporting)
                        {
                            monitor.StatusText = Properties.Resources.ID_Importing;
                        }



                        file = new StreamReader(filename, Encoding.Default);

                        try
                        {
                            bool delimeterValid = false;
                            char[] delimiterChars = { ',', ';', '\t' };
                            char delimeter = '\t';

                            if(linesToIgnore>0)
                            {
                                for(int iIgnore=0; iIgnore<linesToIgnore; iIgnore++)
                                {
                                    string dummy=file.ReadLine();

                                    if(bImporting)
                                    {
                                        WriteToLogfile("Ignore line: " + dummy, true);
                                    }
                                }
                            }

                            string headerline = file.ReadLine();

                            if (headerline != null)
                            {
                                delimeterValid = true;

                                if (headerline.Contains("\t"))
                                {
                                    delimiterChars = new char[] { '\t' };
                                }
                                else if (headerline.Contains(";"))
                                {
                                    delimiterChars = new char[] { ';' };
                                }
                                else if (headerline.Contains(","))
                                {
                                    delimiterChars = new char[] { ',' };
                                }
                                else if (headerline.Contains(" "))
                                {
                                    delimiterChars = new char[] { ' ' };
                                }
                                else
                                {
                                    delimeterValid = false;
                                    resultCode = false;
                                }

                                if (delimeterValid)
                                {
                                    delimeter = delimiterChars[0];

                                    //remove trailing delimeters
                                    while ((headerline.Length > 0) && (headerline[headerline.Length - 1] == delimeter))
                                    {
                                        headerline = headerline.Substring(0, headerline.Length - 1);
                                    }

                                    string[] columns = headerline.Split(delimiterChars);
                                    string[] units = new string[columns.GetLength(0)];

                                    for (int i = 0; i < columns.GetLength(0); i++)
                                    {
                                        units[i] = "";
                                        if (columns[i] != null)
                                        {
                                            columns[i] = columns[i].ToLower();
                                            if(columns[i].Contains("(") && columns[i].Contains(")"))
                                            {
                                                int iStartUnit = columns[i].IndexOf('(');
                                                int iEndUnit = columns[i].IndexOf(')');
                                                if (iStartUnit < iEndUnit)
                                                {
                                                    units[i] = columns[i].Substring(iStartUnit + 1, iEndUnit - iStartUnit - 1);
                                                    units[i]=units[i].Trim();
                                                    columns[i] = columns[i].Substring(0, iStartUnit);
                                                }
                                            }
                                            else if (columns[i].Contains("[") && columns[i].Contains("]"))
                                            {
                                                int iStartUnit = columns[i].IndexOf('[');
                                                int iEndUnit = columns[i].IndexOf(']');
                                                if (iStartUnit < iEndUnit)
                                                {
                                                    units[i] = columns[i].Substring(iStartUnit + 1, iEndUnit - iStartUnit - 1);
                                                    units[i] = units[i].Trim();
                                                    columns[i] = columns[i].Substring(0, iStartUnit);
                                                }
                                            }
                                        }
                                        columns[i]=columns[i].Trim();

                                        if (bImporting)
                                        {
                                            WriteToLogfile("Column " + i + ": " + columns[i], true);
                                            if (units[i] != null)
                                            {
                                                WriteToLogfile("Unit " + i + ":   " + units[i], true);
                                            }
                                        }

                                    }

                                    try
                                    {
                                        string line = file.ReadLine();
                                        bool bFirstLine = true;
                                        bool bFoundEmptyLine = false;

                                        bool foundLapStart = false;
                                        DateTime lapStartTime = DateTime.Now;
                                        DateTime lapEndTime = DateTime.Now;


                                        while (line != null)
                                        {
                                            if (line.Length > 0)
                                            {
                                                string originalLine = line;

                                                if (bImporting)
                                                {
                                                    currentLine++;

                                                    try
                                                    {
                                                        if (totalLines != 0)
                                                        {
                                                            double percent = currentLine / (totalLines / 100.0);
                                                            if (percent > 100.0)
                                                            {
                                                                percent = 100.0;
                                                            }
                                                            monitor.PercentComplete = (float)percent;
                                                        }
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }

                                                //danger: the split method does not care about quoted strings
                                                //replace delimeter characters within quoted strings by \n
                                                try
                                                {
                                                    MasqueradeUnwantedDelimeters(true, delimeter, ref line);
                                                }
                                                catch
                                                {
                                                }

                                                //remove trailing delimeters
                                                while ((line.Length > 0) && (line[line.Length - 1] == delimeter))
                                                {
                                                    line = line.Substring(0, line.Length - 1);
                                                }

                                                string[] values = line.Split(delimiterChars);

                                                try
                                                {
                                                    //re-concatenate quoted strings that got splitted because they contained delimeter chars
                                                    for (int i = 0; i < values.GetLength(0); i++)
                                                    {
                                                        values[i] = values[i].Trim();

                                                        if ((values[i].Length > 1)
                                                            && (values[i].StartsWith("\""))
                                                            && (values[i].EndsWith("\"")))
                                                        {
                                                            //string enclosed with "
                                                            values[i] = values[i].Substring(1, values[i].Length - 2);
                                                            values[i] = values[i].Replace("\"\"", "\"");
                                                            values[i] = values[i].Trim();
                                                        }
                                                        //replace \n again with the delimeter character
                                                        MasqueradeUnwantedDelimeters(false, delimeter, ref values[i]);
                                                    }
                                                }
                                                catch
                                                {
                                                }

                                                if (values.GetLength(0) <= columns.GetLength(0))
                                                {
                                                    try
                                                    {
                                                        DateTime startTime = DateTime.Now;
                                                        bool startTimeContainsTime = false;
                                                        bool startTimeContainsDate = false;
                                                        bool trackPointContainsTime = false;

                                                        DateTime date = DateTime.Now;
                                                        bool dateValid = false;
                                                        DateTime time = DateTime.Now;
                                                        bool timeValid = false;
                                                        DateTime dateAndTime = DateTime.Now;
                                                        bool dateAndTimeValid = false;

                                                        TimeSpan duration = TimeSpan.Zero;
                                                        double distance = double.NaN;
                                                        float heartRateAverage = float.NaN;
                                                        string location = string.Empty;
                                                        string category = string.Empty;
                                                        string subcategory = string.Empty;
                                                        string comment = string.Empty;
                                                        string diarytext = string.Empty;
                                                        string name = string.Empty;
                                                        string equipment = string.Empty;

                                                        bool foundAthleteInfo = false;
                                                        float weight = float.NaN;
                                                        float bodyweight = float.NaN;
                                                        float calories = float.NaN;
                                                        float bmi = float.NaN;
                                                        float fat = float.NaN;
                                                        float restingHeartRate = float.NaN;
                                                        float maxHeartRate = float.NaN;
                                                        float maxHeartRateAthlete = float.NaN;
                                                        float systolic = float.NaN;
                                                        float diastolic = float.NaN;
                                                        float power = float.NaN;
                                                        float cadence = float.NaN;
                                                        float elevation = float.NaN;
                                                        float ascendedMeters = float.NaN;
                                                        float latitude = float.NaN;
                                                        float longitude = float.NaN;
                                                        float speed = float.NaN;
                                                        int intensity = 0;
                                                        int mood = 0;
                                                        bool newLap = false;
                                                        TimeSpan sleep = TimeSpan.Zero;




                                                        for (int i = 0; i < values.GetLength(0); i++)
                                                        {
                                                            if ((columns[i] != null) && (columns[i].Length > 0) && (values[i] != null) && (values[i].Length > 0))
                                                            {
                                                                switch (columns[i])
                                                                {
                                                                    case "datetime":
                                                                    case "timestamp":
                                                                    case "rowdate":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                dateAndTime = DateTime.Parse(values[i], chosenDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                                dateAndTimeValid = true;

                                                                                WriteToLogfile("Timestamp: " + dateAndTime.ToString(), true);
                                                                                WriteToLogfile("Timestamp as local: " + dateAndTime.ToLocalTime().ToString(), true);
                                                                                WriteToLogfile("Timestamp as UTC: " + dateAndTime.ToUniversalTime().ToString(), true);
                                                                                if (dateAndTime.Kind == DateTimeKind.Unspecified)
                                                                                {
                                                                                    WriteToLogfile("Date time kind: Unspecified", true);
                                                                                }
                                                                                else if (dateAndTime.Kind == DateTimeKind.Utc)
                                                                                {
                                                                                    WriteToLogfile("Date time kind: UTC", true);
                                                                                }
                                                                                else if (dateAndTime.Kind == DateTimeKind.Local)
                                                                                {
                                                                                    WriteToLogfile("Date time kind: Local", true);
                                                                                }

                                                                            }
                                                                            catch
                                                                            {
                                                                                WriteToLogfile("Line contains wrong datetime / timestamp format: " + originalLine, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongDateTimeFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (bCheckingFormat)
                                                                        {
                                                                            try
                                                                            {
                                                                                dateAndTime = DateTime.Parse(values[i], systemDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bSystemDateTimeFormatok = false;
                                                                            }
                                                                            try
                                                                            {
                                                                                dateAndTime = DateTime.Parse(values[i], usDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bUsDateTimeFormatok = false;
                                                                            }
                                                                            try
                                                                            {
                                                                                dateAndTime = DateTime.Parse(values[i], deDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bDeDateTimeFormatok = false;
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "date":
                                                                    case "entrydate":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                date = DateTime.Parse(values[i], chosenDateformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                                dateValid = true;
                                                                            }
                                                                            catch
                                                                            {
                                                                                DateTime example = new DateTime(2008, 1, 31);
                                                                                WriteToLogfile("Line contains wrong date format: " + originalLine, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongDateFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_Example + example.ToShortDateString()
                                                                                        + Properties.Resources.ID_Example + example.ToLongDateString()
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (bCheckingFormat)
                                                                        {
                                                                            try
                                                                            {
                                                                                date = DateTime.Parse(values[i], systemDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bSystemDateFormatok = false;
                                                                            }
                                                                            try
                                                                            {
                                                                                date = DateTime.Parse(values[i], usDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bUsDateFormatok = false;
                                                                            }
                                                                            try
                                                                            {
                                                                                date = DateTime.Parse(values[i], deDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bDeDateFormatok = false;
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "time":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                time = DateTime.Parse(values[i], chosenTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                                timeValid = true;
                                                                            }
                                                                            catch
                                                                            {
                                                                                try
                                                                                {
                                                                                    duration = GetDecimalDuration(values[i], units[i], "s");
                                                                                }
                                                                                catch (Exception)
                                                                                {
                                                                                    DateTime example = new DateTime(2008, 1, 31, 14, 38, 52);
                                                                                    WriteToLogfile("Line contains wrong time format: " + originalLine, true);
                                                                                    if (!bFoundEmptyLine)
                                                                                    {
                                                                                        monitor.ErrorText = Properties.Resources.ID_WrongTimeFormat
                                                                                            + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                            + Properties.Resources.ID_Example + example.ToShortTimeString()
                                                                                            + Properties.Resources.ID_Example + example.ToLongTimeString()
                                                                                            + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                        resultCode = false;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        if (bCheckingFormat)
                                                                        {
                                                                            try
                                                                            {
                                                                                time = DateTime.Parse(values[i], systemDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bSystemTimeFormatok = false;
                                                                            }
                                                                            try
                                                                            {
                                                                                time = DateTime.Parse(values[i], usDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bUsTimeFormatok = false;
                                                                            }
                                                                            try
                                                                            {
                                                                                time = DateTime.Parse(values[i], deDateTimeformat, System.Globalization.DateTimeStyles.AssumeLocal);
                                                                            }
                                                                            catch
                                                                            {
                                                                                bDeTimeFormatok = false;
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "duration":
                                                                    case "elapsed time":
                                                                    case "tottime":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                duration = GetDecimalDuration(values[i], units[i], "m");
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong duration format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongDurationFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "distance":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                if (bIsrowpro3File && (units[i]=="") )
                                                                                {
                                                                                    distance = GetDecimalDistanceAsMeters(values[i], units[i], 1.0);
                                                                                }
                                                                                else
                                                                                {
                                                                                    distance = GetDecimalDistanceAsMeters(values[i], units[i], 1000.0);
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong distance format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongDistanceFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "totaldistance":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                distance = GetDecimalDistanceAsMeters(values[i], units[i], 1.0);
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong distance format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongDistanceFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "speed":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                speed = GetDecimalValue(values[i], new string[1] { "mph" });
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong speed format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "elevation":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                elevation = (float)GetDecimalDistanceAsMeters(values[i], units[i], 1.0);

                                                                                if (!bContainsTrackData)
                                                                                {
                                                                                    bContainsTrackData = true;
                                                                                    WriteToLogfile("Found elevation details, assume track data", true);
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong elevation format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongDistanceFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "climbed":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                ascendedMeters = (float)GetDecimalDistanceAsMeters(values[i], units[i], 1.0);
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong climbed format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongClimbedFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "weight":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                weight = GetDecimalWeight(values[i], units[i]);
                                                                                if (!float.IsNaN(weight))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong weight format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongWeightFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "bodyweight":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                bodyweight = GetDecimalWeight(values[i], units[i]);
                                                                                if (!float.IsNaN(bodyweight))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong weight format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongWeightFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "calories":
                                                                    case "output":
                                                                    case "totalcalories":
                                                                    case "cals":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                calories = GetDecimalEnergy(values[i], units[i], 1);
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong calories format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongCaloriesFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "kjoule":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                calories = GetDecimalEnergy(values[i], units[i], (float)4.1868);
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong calories format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongCaloriesFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "bmi":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                bmi = GetDecimalValue(values[i], null);

                                                                                if (!float.IsNaN(bmi))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong bmi format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongBmiFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "intensity":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                string sValue = values[i].ToLower();
                                                                                sValue = sValue.Trim();

                                                                                intensity = System.Convert.ToInt32(sValue);
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong intensity format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongIntensityFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "mood":
                                                                    case "feeling":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                string sValue = values[i].ToLower();
                                                                                sValue = sValue.Trim();

                                                                                mood = System.Convert.ToInt32(sValue);
                                                                                foundAthleteInfo = true;
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong mood format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongMoodFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "restingheartrate":
                                                                    case "resthr":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                restingHeartRate = GetDecimalValue(values[i], new string[1] { "bpm" });

                                                                                if (!float.IsNaN(restingHeartRate))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong restingHeartRate format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongHeartRateFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "maxheartrate":
                                                                    case "heartmax":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                maxHeartRate = GetDecimalValue(values[i], new string[1] { "bpm" });
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong maxHeartRate format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongHeartRateFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "athletemaxheartrate":
                                                                    case "athleteheartmax":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                maxHeartRateAthlete = GetDecimalValue(values[i], new string[1] { "bpm" });

                                                                                if (!float.IsNaN(maxHeartRateAthlete))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong maxHeartRate format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongHeartRateFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "systolic":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                systolic = GetDecimalValue(values[i], null);

                                                                                if (!float.IsNaN(systolic))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong systolic format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongBloodPressureFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "diastolic":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                diastolic = GetDecimalValue(values[i], null);

                                                                                if (!float.IsNaN(diastolic))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong diastolic format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongBloodPressureFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "bodyfat":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                float temp = GetDecimalValue(values[i], null);

                                                                                if ((temp > 0.0) && (temp < 100.0))
                                                                                {
                                                                                    fat = (temp);
                                                                                }

                                                                                if (!float.IsNaN(fat))
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong body fat format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongBodyFatFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "sleep":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                sleep = GetDecimalDuration(values[i], units[i], "h");

                                                                                if (sleep.TotalHours != 0.0)
                                                                                {
                                                                                    foundAthleteInfo = true;
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong sleep format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongSleepFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "location":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            location = values[i];
                                                                        }
                                                                        break;

                                                                    case "category":
                                                                    case "activity":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            category = values[i];
                                                                            category = category.Replace(": ", ":");
                                                                        }
                                                                        break;

                                                                    case "equipment":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            equipment = values[i];
                                                                        }
                                                                        break;

                                                                    case "subcategory":
                                                                    case "type":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            subcategory = values[i];
                                                                            subcategory = subcategory.Replace(": ", ":");
                                                                        }
                                                                        break;

                                                                    case "name":
                                                                    case "description":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            name = values[i];
                                                                        }
                                                                        break;

                                                                    case "comment":
                                                                    case "details":
                                                                    case "comments":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            comment = values[i];
                                                                        }
                                                                        break;

                                                                    case "diary":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            diarytext = values[i];
                                                                            if (diarytext.Length > 0)
                                                                            {
                                                                                foundAthleteInfo = true;
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "hrm":
                                                                    case "heartrate":
                                                                    case "pulse":
                                                                    case "puls":
                                                                    case "heartaverage":
                                                                    case "endhr":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                heartRateAverage = GetDecimalValue(values[i], new string[1] { "bpm" });

                                                                                if (!float.IsNaN(heartRateAverage))
                                                                                {
                                                                                    heartRateAverageTotal += heartRateAverage;
                                                                                    heartRateCount++;
                                                                                    if (heartRateAverage > heartRateAverageMax)
                                                                                    {
                                                                                        heartRateAverageMax = heartRateAverage;
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong hrm / heartrate format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongHrmFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "power":
                                                                    case "watt":
                                                                    case "averagewatts":
                                                                    case "avgwatts":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                power = GetDecimalValue(values[i], new string[2] { "watt", "w" });

                                                                                if (!float.IsNaN(power))
                                                                                {
                                                                                    powerTotal += power;
                                                                                    powerCount++;
                                                                                    if (power > powerMax)
                                                                                    {
                                                                                        powerMax = power;
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong power format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongPowerFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "cadence":
                                                                    case "rpm":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                cadence = GetDecimalValue(values[i], new string[1] { "rpm" });

                                                                                if (!float.IsNaN(cadence))
                                                                                {
                                                                                    cadenceTotal += cadence;
                                                                                    cadenceCount++;
                                                                                    if (cadence > cadenceMax)
                                                                                    {
                                                                                        cadenceMax = cadence;
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong cadence format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongCadenceFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "lap marker":
                                                                    case "lap":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                string sValue = values[i].ToLower();
                                                                                sValue = sValue.Trim();

                                                                                if (sValue.Length > 0)
                                                                                {
                                                                                    switch (sValue)
                                                                                    {
                                                                                        case "0":
                                                                                        case "-":
                                                                                        case "no":
                                                                                            newLap = false;
                                                                                            break;

                                                                                        case "1":
                                                                                        case "yes":
                                                                                        default:
                                                                                            newLap = true;
                                                                                            break;
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong mood format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongMoodFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "latitude":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                latitude = GetDecimalValue(values[i], null);
                                                                                bContainsGpsData = true;

                                                                                if (!bContainsTrackData)
                                                                                {
                                                                                    bContainsTrackData = true;
                                                                                    WriteToLogfile("Found latitude details, assume track data", true);
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong latitude format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongLatLongFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    case "longitude":
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            try
                                                                            {
                                                                                longitude = GetDecimalValue(values[i], null);
                                                                                bContainsGpsData = true;

                                                                                if (!bContainsTrackData)
                                                                                {
                                                                                    bContainsTrackData = true;
                                                                                    WriteToLogfile("Found longitude details, assume track data", true);
                                                                                }
                                                                            }
                                                                            catch (Exception e)
                                                                            {
                                                                                WriteToLogfile("Line contains wrong longitude format: " + originalLine, true);
                                                                                WriteToLogfile("Error: " + e.Message, true);
                                                                                if (!bFoundEmptyLine)
                                                                                {
                                                                                    monitor.ErrorText = Properties.Resources.ID_WrongLatLongFormat
                                                                                        + "\n" + columns[i] + " --> " + values[i] + "\n"
                                                                                        + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                                    resultCode = false;
                                                                                }
                                                                            }
                                                                        }
                                                                        break;

                                                                    default:
                                                                        if (bImporting || bCheckingTrack)
                                                                        {
                                                                            if (currentLine == 1)
                                                                            {
                                                                                WriteToLogfile("Header contains unsupported field name: " + columns[i], true);
                                                                            }
                                                                        }
                                                                        break;

                                                                }
                                                            }
                                                        }

                                                        if (bIsIbikeFile)
                                                        {
                                                            //iBikeTimeStamp
                                                            //
                                                            if ((!dateAndTimeValid) && (!timeValid))
                                                            {
                                                                dateAndTime = iBikeTimeStampStart + iBikeCurrentDuration;
                                                                dateAndTimeValid=true;
                                                                //WriteToLogfile("Build time for iBike line: " + dateAndTime.ToString(), true);
                                                            }
                                                        }

                                                        if (dateAndTimeValid)
                                                        {
                                                            startTime = dateAndTime;
                                                            startTimeContainsDate = true;
                                                            startTimeContainsTime = true;
                                                            trackPointContainsTime = true;
                                                        }
                                                        else if (dateValid && timeValid)
                                                        {
                                                            startTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Local);
                                                            startTimeContainsDate = true;
                                                            startTimeContainsTime = true;
                                                            trackPointContainsTime = true;
                                                        }
                                                        else if (dateValid)
                                                        {
                                                            startTime = date;
                                                            startTimeContainsDate = true;
                                                            startTimeContainsTime = false;
                                                            trackPointContainsTime = false;
                                                        }
                                                        else if (timeValid)
                                                        {
                                                            startTime = new DateTime(fileDateTime.Year, fileDateTime.Month, fileDateTime.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Local);
                                                            startTimeContainsDate = true;
                                                            startTimeContainsTime = true;
                                                            trackPointContainsTime = true;
                                                        }
                                                        else
                                                        {
                                                            startTime = new DateTime(fileDateTime.Year, fileDateTime.Month, fileDateTime.Day, fileDateTime.Hour, fileDateTime.Minute, fileDateTime.Second, DateTimeKind.Local);
                                                            startTimeContainsDate = true;
                                                            startTimeContainsTime = true;
                                                            trackPointContainsTime = false;
                                                        }

                                                        if (bCheckingTrack)
                                                        {
                                                            if (bFirstLine)
                                                            {
                                                                dateFirstLine = startTime;
                                                            }
                                                            else
                                                            {
                                                                if (startTime.Date == dateFirstLine.Date)
                                                                {
                                                                    sameDate++;
                                                                }
                                                            }
                                                        }


                                                        if (bImporting)
                                                        {
                                                            if (resultCode)
                                                            {
                                                                if (startTimeContainsDate)
                                                                {
                                                                    try
                                                                    {
                                                                        IActivity activity = null;

                                                                        if (bContainsTrackData)
                                                                        {
                                                                            if (activityTrack == null)
                                                                            {
                                                                                activityTrack = importResults.AddActivity(startTime);
                                                                            }
                                                                            activity = activityTrack;
                                                                        }
                                                                        else
                                                                        {
                                                                            activity = importResults.AddActivity(startTime);
                                                                        }

                                                                        /*
                                                                        WriteToLogfile("Acticity start time kind: " + startTime.Kind.ToString(), true);
                                                                        WriteToLogfile("Acticity start time: " + startTime.ToString(), true);
                                                                        WriteToLogfile("Acticity start time as UTC: " + startTime.ToUniversalTime().ToString(), true);
                                                                        WriteToLogfile("Acticity start time as local time: " + startTime.ToLocalTime().ToString(), true);
                                                                        WriteToLogfile("Acticity start time components: " + startTime.Year + "/" + startTime.Month + "/" + startTime.Day + " " + startTime.Hour + ":" + startTime.Minute + ":" + startTime.Second, true);
                                                                        */
                                                                        

                                                                        activity.HasStartTime = startTimeContainsTime;

                                                                        //activity.Weather.ConditionsText = "test";
                                                                        //activity.Weather.TemperatureCelsius = 30;
                                                                        //activity.Weather.ConditionsNotes = "blah";


                                                                        if (bContainsTrackData)
                                                                        {
                                                                            IBikeElement iBikeElement=null;

                                                                            if (bCalculateIBikePauseFromGpsTrack)
                                                                            {
                                                                                iBikeElement = new IBikeElement();
                                                                                iBikeElement.elapsedtime = iBikeCurrentDuration;
                                                                                elementsFromIBikeList.Add(iBikeElement);
                                                                            }

                                                                            if ((!trackPointContainsTime) && (duration != TimeSpan.Zero))
                                                                            {
                                                                                startTime += duration;
                                                                            }

                                                                            if (!double.IsNaN(distance))
                                                                            {
                                                                                if (activity.DistanceMetersTrack == null)
                                                                                {
                                                                                    activity.DistanceMetersTrack = new DistanceDataTrack();
                                                                                    activity.DistanceMetersTrack.AllowMultipleAtSameTime = true;
                                                                                }

                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.distance = (float)distance;
                                                                                }
                                                                                else
                                                                                {
                                                                                    if(double.IsNaN(distanceLastValue))
                                                                                    {
                                                                                        activity.DistanceMetersTrack.Add(startTime, (float)distance);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (distanceLastValue != distance)
                                                                                        {
                                                                                            activity.DistanceMetersTrack.Add(startTime, (float)distance);
                                                                                        }
                                                                                    }
                                                                                    distanceLastValue = distance;
                                                                                }
                                                                                //WriteToLogfile(string.Format("Distance track point: {0} -> {1}   count: {2}", startTime.ToShortTimeString(), distance, activity.DistanceMetersTrack.Count), true);
                                                                            }

                                                                            if (!float.IsNaN(heartRateAverage))
                                                                            {
                                                                                if (activity.HeartRatePerMinuteTrack == null)
                                                                                {
                                                                                    activity.HeartRatePerMinuteTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                                                }

                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.heartrate = heartRateAverage;
                                                                                }
                                                                                else
                                                                                {
                                                                                    activity.HeartRatePerMinuteTrack.Add(startTime, heartRateAverage);
                                                                                }
                                                                            }

                                                                            if (!float.IsNaN(power))
                                                                            {
                                                                                if (activity.PowerWattsTrack == null)
                                                                                {
                                                                                    activity.PowerWattsTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                                                }

                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.power = power;
                                                                                }
                                                                                else
                                                                                {
                                                                                    activity.PowerWattsTrack.Add(startTime, power);
                                                                                }
                                                                            }

                                                                            if (!float.IsNaN(cadence))
                                                                            {
                                                                                if (activity.CadencePerMinuteTrack == null)
                                                                                {
                                                                                    activity.CadencePerMinuteTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                                                }

                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.cadence = cadence;
                                                                                }
                                                                                else
                                                                                {
                                                                                    activity.CadencePerMinuteTrack.Add(startTime, cadence);
                                                                                }
                                                                            }

                                                                            if (!float.IsNaN(elevation))
                                                                            {
                                                                                if (activity.ElevationMetersTrack == null)
                                                                                {
                                                                                    activity.ElevationMetersTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                                                }

                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.elevation = elevation;
                                                                                }
                                                                                else
                                                                                {
                                                                                    activity.ElevationMetersTrack.Add(startTime, elevation);
                                                                                }

                                                                                if (float.IsNaN(elevationLastValue))
                                                                                {
                                                                                    elevationLastValue = elevation;
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (elevation > elevationLastValue)
                                                                                    {
                                                                                        totalAscendMeters += elevation - elevationLastValue;
                                                                                        //activity.TotalAscendMetersEntered = totalAscendMeters;
                                                                                    }
                                                                                    else if (elevation < elevationLastValue)
                                                                                    {
                                                                                        totalDescendMeters += elevationLastValue - elevation;
                                                                                        //activity.TotalDescendMetersEntered = totalDescendMeters;
                                                                                    }
                                                                                    elevationLastValue = elevation;
                                                                                }
                                                                            }

                                                                            if (!float.IsNaN(speed))
                                                                            {
                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.speed = speed;
                                                                                }
                                                                            }

                                                                            if (newLap)
                                                                            {
                                                                                if (bCalculateIBikePauseFromGpsTrack)
                                                                                {
                                                                                    iBikeElement.newLap = true;
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (foundLapStart)
                                                                                    {
                                                                                        TimeSpan lapDuration = startTime - lapStartTime;
                                                                                        activity.Laps.Add(lapStartTime, lapDuration);
                                                                                        WriteToLogfile("Add lap: " + lapStartTime + ", duration: " + lapDuration, true);

                                                                                    }
                                                                                    foundLapStart = true;
                                                                                    lapStartTime = startTime;
                                                                                    lapEndTime = startTime;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (foundLapStart)
                                                                                {
                                                                                    lapEndTime = startTime;
                                                                                }
                                                                                else
                                                                                {
                                                                                    foundLapStart = true;
                                                                                    lapStartTime = startTime;
                                                                                    lapEndTime = startTime;
                                                                                }
                                                                            }

                                                                            if(!bCalculateIBikePauseFromGpsTrack)
                                                                            {
                                                                                if ((!float.IsNaN(latitude)) && (!float.IsNaN(longitude))
                                                                                    && (latitude!=0.0) && (longitude!=0.0)
                                                                                    )
                                                                                {
                                                                                    if (activity.GPSRoute == null)
                                                                                    {
                                                                                        activity.GPSRoute = new GPSRoute();
                                                                                    }
                                                                                    activity.GPSRoute.Add(startTime, new GPSPoint(latitude, longitude, elevation));
                                                                                }
                                                                            }
                                                                        }

                                                                        if ((!bCalculateIBikePauseFromGpsTrack) && (!double.IsNaN(distance)))
                                                                        {
                                                                            activity.TotalDistanceMetersEntered = (float)distance;
                                                                            //WriteToLogfile(string.Format("Total distance entered: {0}", distance), true);
                                                                        }

                                                                        if (!bCalculateIBikePauseFromGpsTrack)
                                                                        {
                                                                            if (duration != TimeSpan.Zero)
                                                                            {
                                                                                activity.TotalTimeEntered = duration;
                                                                            }
                                                                            else if (iBikeCurrentDuration != TimeSpan.Zero)
                                                                            {
                                                                                activity.TotalTimeEntered = iBikeCurrentDuration;
                                                                            }
                                                                        }

                                                                        if (!float.IsNaN(ascendedMeters))
                                                                        {
                                                                            activity.TotalAscendMetersEntered = ascendedMeters;
                                                                        }

                                                                        if (!bContainsTrackData)
                                                                        {
                                                                            if (!float.IsNaN(heartRateAverage))
                                                                            {
                                                                                activity.AverageHeartRatePerMinuteEntered = heartRateAverage;
                                                                            }
                                                                        }

                                                                        if (!float.IsNaN(maxHeartRate))
                                                                        {
                                                                            activity.MaximumHeartRatePerMinuteEntered = maxHeartRate;
                                                                        }

                                                                        if (!bContainsTrackData)
                                                                        {
                                                                            if (!float.IsNaN(power))
                                                                            {
                                                                                activity.AveragePowerWattsEntered = power;
                                                                            }
                                                                        }

                                                                        if (!bContainsTrackData)
                                                                        {
                                                                            if (!float.IsNaN(cadence))
                                                                            {
                                                                                activity.AverageCadencePerMinuteEntered = cadence;
                                                                            }
                                                                        }

                                                                        if (!float.IsNaN(calories))
                                                                        {
                                                                            activity.TotalCalories = calories;
                                                                        }

                                                                        if (name.Length > 0)
                                                                        {
                                                                            activity.Name = name;
                                                                        }

                                                                        if (location.Length > 0)
                                                                        {
                                                                            activity.Location = location;
                                                                        }

                                                                        if (comment.Length > 0)
                                                                        {
                                                                            activity.Notes = comment;
                                                                        }
                                                                        else
                                                                        {
                                                                            activity.Notes = Path.GetFileName(filename);
                                                                        }

                                                                        if ((intensity >= 1) && (intensity <= 10))
                                                                        {
                                                                            activity.Intensity = intensity;
                                                                        }

                                                                        if ((category.Length > 0) && (subcategory.Length > 0))
                                                                        {
                                                                            category = category + ":" + subcategory;
                                                                        }
                                                                        else if ((category.Length == 0) && (subcategory.Length > 0))
                                                                        {
                                                                            category = subcategory;
                                                                        }


                                                                        if (category.Length > 0)
                                                                        {
                                                                            try
                                                                            {
                                                                                IActivityCategory categoryId = null;

                                                                                categoryId = FindActivityCategory(category, Application.Logbook.ActivityCategories);

                                                                                if (categoryId == null)
                                                                                {
                                                                                    //Plan B: reduce subsequently: alpha:beta:gamma -> alpha:beta -> alpha
                                                                                    if (category.Contains(":"))
                                                                                    {
                                                                                        char[] categoryDelimiterChars = { ':' };
                                                                                        string[] categoryParts = category.Split(categoryDelimiterChars);

                                                                                        for (int iParts = categoryParts.GetLength(0) - 1; iParts > 0; iParts--)
                                                                                        {
                                                                                            string categoryReduced = string.Empty;
                                                                                            for (int iCombine = 0; iCombine < iParts; iCombine++)
                                                                                            {
                                                                                                if (categoryReduced.Length > 0)
                                                                                                {
                                                                                                    categoryReduced += ":";
                                                                                                }
                                                                                                categoryReduced += categoryParts[iCombine];
                                                                                            }

                                                                                            WriteToLogfile("Line contains unknown category: " + originalLine + ", try " + categoryReduced, true);

                                                                                            categoryId = FindActivityCategory(categoryReduced, Application.Logbook.ActivityCategories);

                                                                                            if (categoryId != null)
                                                                                            {
                                                                                                WriteToLogfile("Line contains unknown category: " + originalLine + ", succeeded with " + categoryReduced, true);
                                                                                                break;
                                                                                            }
                                                                                        }

                                                                                        if (categoryId == null)
                                                                                        {
                                                                                            //Plan C: look only for subcategories
                                                                                            for (int iParts = 1; iParts < categoryParts.GetLength(0); iParts++)
                                                                                            {
                                                                                                WriteToLogfile("Line contains unknown category: " + originalLine + ", try " + categoryParts[iParts], true);

                                                                                                categoryId = FindActivityCategory(categoryParts[iParts], Application.Logbook.ActivityCategories);

                                                                                                if (categoryId != null)
                                                                                                {
                                                                                                    WriteToLogfile("Line contains unknown category: " + originalLine + ", succeeded with " + categoryParts[iParts], true);
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    activity.Category = categoryId;
                                                                                }
                                                                            }
                                                                            catch
                                                                            {
                                                                            }
                                                                        }

                                                                        if (equipment.Length > 0)
                                                                        {
                                                                            FindEquipment(equipment, Application.Logbook.Equipment, activity.EquipmentUsed);
                                                                            if (activity.EquipmentUsed.Count == 0)
                                                                            {
                                                                                WriteToLogfile("Did not find equipment: " + equipment, true);
                                                                            }
                                                                        }

                                                                        activity.Metadata.Source = string.Format("Imported from {0}", Path.GetFileName(filename));

                                                                        if (bIsIbikeFile)
                                                                        {
                                                                            activity.UseEnteredData = true;
                                                                        }
                                                                        else if (bContainsGpsData || bContainsTrackData)
                                                                        {
                                                                            activity.UseEnteredData = false;
                                                                        }
                                                                        else
                                                                        {
                                                                            activity.UseEnteredData = true;
                                                                        }
                                                                    }
                                                                    catch (Exception e)
                                                                    {
                                                                        WriteToLogfile("Could not add activity to SportTracks: " + originalLine, true);
                                                                        WriteToLogfile("Error: " + e.Message, true);
                                                                        monitor.ErrorText = Properties.Resources.ID_AddActivityError
                                                                            + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                                        resultCode = false;
                                                                    }

                                                                    importedLines++;
                                                                }
                                                                else
                                                                {
                                                                    WriteToLogfile("Line contains no date: " + originalLine, true);
                                                                }

                                                                if (startTimeContainsDate && foundAthleteInfo)
                                                                {
                                                                    IAthleteInfoEntry infoEntry = Application.Logbook.Athlete.InfoEntries.EntryForDate(startTime);

                                                                    if (infoEntry == null)
                                                                    {
                                                                        WriteToLogfile("Add athlete info for date: " + startTime.ToShortDateString(), true);

                                                                        Application.Logbook.Athlete.InfoEntries.EntryDates.Add(startTime);
                                                                        infoEntry = Application.Logbook.Athlete.InfoEntries.EntryForDate(startTime);
                                                                    }
                                                                    else
                                                                    {
                                                                        WriteToLogfile("Update athlete info for date: " + startTime.ToShortDateString(), true);
                                                                    }
                                                                    if (infoEntry != null)
                                                                    {
                                                                        if (!float.IsNaN(bodyweight))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.WeightKilograms) || (infoEntry.WeightKilograms == 0.0))
                                                                            {
                                                                                infoEntry.WeightKilograms = bodyweight;
                                                                            }
                                                                            if (float.IsNaN(bmi))
                                                                            {
                                                                                if (!float.IsNaN(Application.Logbook.Athlete.HeightCentimeters)
                                                                                    && (Application.Logbook.Athlete.HeightCentimeters != 0.0))
                                                                                {

                                                                                    bmi = (float)(infoEntry.WeightKilograms /
                                                                                        (Math.Pow(Application.Logbook.Athlete.HeightCentimeters / 100.0, 2.0)));
                                                                                }
                                                                            }
                                                                        }
                                                                        else if (!float.IsNaN(weight))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.WeightKilograms) || (infoEntry.WeightKilograms == 0.0))
                                                                            {
                                                                                infoEntry.WeightKilograms = weight;
                                                                            }
                                                                            if (float.IsNaN(bmi))
                                                                            {
                                                                                if (!float.IsNaN(Application.Logbook.Athlete.HeightCentimeters)
                                                                                    && (Application.Logbook.Athlete.HeightCentimeters != 0.0))
                                                                                {

                                                                                    bmi = (float)(infoEntry.WeightKilograms /
                                                                                        (Math.Pow(Application.Logbook.Athlete.HeightCentimeters / 100.0, 2.0)));
                                                                                }
                                                                            }
                                                                        }
                                                                        if (!float.IsNaN(bmi))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.BMI) || (infoEntry.BMI == 0.0))
                                                                            {
                                                                                infoEntry.BMI = bmi;
                                                                            }
                                                                        }
                                                                        if (!float.IsNaN(fat))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.BodyFatPercentage) || (infoEntry.BodyFatPercentage <= 1.0))
                                                                            {
                                                                                infoEntry.BodyFatPercentage = fat;
                                                                            }
                                                                        }
                                                                        if (!float.IsNaN(restingHeartRate))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.RestingHeartRatePerMinute))
                                                                            {
                                                                                infoEntry.RestingHeartRatePerMinute = restingHeartRate;
                                                                            }
                                                                        }
                                                                        if (!float.IsNaN(maxHeartRateAthlete))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.MaximumHeartRatePerMinute))
                                                                            {
                                                                                infoEntry.MaximumHeartRatePerMinute = maxHeartRateAthlete;
                                                                            }
                                                                        }
                                                                        if (!float.IsNaN(systolic))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.SystolicBloodPressure))
                                                                            {
                                                                                infoEntry.SystolicBloodPressure = systolic;
                                                                            }
                                                                        }
                                                                        if (!float.IsNaN(diastolic))
                                                                        {
                                                                            if (float.IsNaN(infoEntry.DiastolicBloodPressure))
                                                                            {
                                                                                infoEntry.DiastolicBloodPressure = diastolic;
                                                                            }
                                                                        }
                                                                        if (sleep.TotalHours != 0.0)
                                                                        {
                                                                            if (float.IsNaN(infoEntry.SleepHours))
                                                                            {
                                                                                infoEntry.SleepHours = (float)sleep.TotalHours;
                                                                            }
                                                                        }
                                                                        if (diarytext.Length > 0)
                                                                        {
                                                                            infoEntry.DiaryText += diarytext;
                                                                        }
                                                                        if ((mood >= 0) && (mood <= 3))
                                                                        {
                                                                            infoEntry.Mood = mood;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (bIsIbikeFile)
                                                        {
                                                            //iBikeTimeStamp
                                                            iBikeCurrentDuration += new TimeSpan(0, 0, iBikeTimeInterval);

                                                            /*
                                                            if (bCalculateIBikePauseFromGpsTrack && (pausesFromGpsTrack != null) && bImporting && (iBikeCurrentDuration.TotalSeconds < originalGpsTrackTotalSeconds))
                                                            {
                                                                for (int iPause = 0; iPause < pausesFromGpsTrack.GetLength(0); iPause++)
                                                                {
                                                                    if ((iBikeCurrentDuration >= pausesFromGpsTrack[iPause].beginPause) && (iBikeCurrentDuration < pausesFromGpsTrack[iPause].endPause))
                                                                    {
                                                                        for (int seconds = 0; seconds < pausesFromGpsTrack[iPause].duration.TotalSeconds; seconds++)
                                                                        {
                                                                            bool bIsFirstOfPause = (seconds == 0);
                                                                            bool bIsLastOfPause = (seconds == pausesFromGpsTrack[iPause].duration.TotalSeconds - 1);

                                                                            try
                                                                            {
                                                                                if (bIsFirstOfPause || bIsLastOfPause)
                                                                                {
                                                                                    DateTime trackPointTime = iBikeTimeStampStart + iBikeCurrentDuration;

                                                                                    if (activityTrack != null)
                                                                                    {
                                                                                        if (activityTrack.PowerWattsTrack != null)
                                                                                        {
                                                                                            activityTrack.PowerWattsTrack.Add(trackPointTime, 0);
                                                                                        }
                                                                                        if (activityTrack.CadencePerMinuteTrack != null)
                                                                                        {
                                                                                            activityTrack.CadencePerMinuteTrack.Add(trackPointTime, 0);
                                                                                        }

                                                                                        //WriteToLogfile(string.Format("Timestamp in pause: {0}   Power: {1}  Cadence: {2}", iBikeCurrentDuration.ToString(), 0, 0), true);
                                                                                    }
                                                                                }
                                                                            }
                                                                            catch (Exception)
                                                                            {
                                                                            }

                                                                            iBikeCurrentDuration += new TimeSpan(0, 0, 1);
                                                                            if (iBikeCurrentDuration.TotalSeconds >= originalGpsTrackTotalSeconds)
                                                                            {
                                                                                break;
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            */
                                                        }
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }
                                                else
                                                {
                                                    WriteToLogfile("Line contains wrong field count: " + originalLine, true);
                                                    monitor.ErrorText = Properties.Resources.ID_WrongFieldCount + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                                    resultCode = false;
                                                }
                                            }
                                            else
                                            {
                                                bFoundEmptyLine = true;
                                            }

                                            line = file.ReadLine();

                                            if (monitor.Cancelled)
                                            {
                                                WriteToLogfile("Import cancelled by user", true);
                                                break;
                                            }

                                            if (!resultCode)
                                            {
                                                WriteToLogfile("Import cancelled due to error", true);
                                                break;
                                            }

                                            bFirstLine = false;

                                            if (bIsIbikeFile && bImporting && bCalculateIBikePauseFromGpsTrack && (iBikeCurrentDuration.TotalSeconds >= originalGpsTrackTotalSeconds))
                                            {
                                                break;
                                            }
                                        }


                                        if (bImporting && (activityTrack!=null))
                                        {
                                            IActivity activity=activityTrack;

                                            if (heartRateCount > 0)
                                            {
                                                activity.AverageHeartRatePerMinuteEntered = (float)(heartRateAverageTotal / heartRateCount);
                                                if (mergeIBikeDestinationActivity != null)
                                                {
                                                    mergeIBikeDestinationActivity.AverageHeartRatePerMinuteEntered = activity.AverageHeartRatePerMinuteEntered;
                                                }
                                            }
                                            if (heartRateAverageMax > 0)
                                            {
                                                activity.MaximumHeartRatePerMinuteEntered = heartRateAverageMax;
                                                if (mergeIBikeDestinationActivity != null)
                                                {
                                                    mergeIBikeDestinationActivity.MaximumHeartRatePerMinuteEntered = activity.MaximumHeartRatePerMinuteEntered;
                                                }
                                            }
                                            if (powerCount > 0)
                                            {
                                                activity.AveragePowerWattsEntered = (float)(powerTotal / powerCount);
                                                if (mergeIBikeDestinationActivity != null)
                                                {
                                                    mergeIBikeDestinationActivity.AveragePowerWattsEntered = activity.AveragePowerWattsEntered;
                                                }
                                            }
                                            if (powerMax > 0)
                                            {
                                                activity.MaximumPowerWattsEntered = powerMax;
                                                if (mergeIBikeDestinationActivity != null)
                                                {
                                                    mergeIBikeDestinationActivity.MaximumPowerWattsEntered = activity.MaximumPowerWattsEntered;
                                                }
                                            }
                                            if (cadenceCount > 0)
                                            {
                                                activity.AverageCadencePerMinuteEntered = (float)(cadenceTotal / cadenceCount);
                                                if (mergeIBikeDestinationActivity != null)
                                                {
                                                    mergeIBikeDestinationActivity.AverageCadencePerMinuteEntered = activity.AverageCadencePerMinuteEntered;
                                                }
                                            }
                                            if (cadenceMax > 0)
                                            {
                                                activity.MaximumCadencePerMinuteEntered = cadenceMax;
                                                if (mergeIBikeDestinationActivity != null)
                                                {
                                                    mergeIBikeDestinationActivity.MaximumCadencePerMinuteEntered = activity.MaximumCadencePerMinuteEntered;
                                                }
                                            }

                                            if (mergeIBikeDestinationActivity != null)
                                            {
                                                mergeIBikeDestinationActivity.UseEnteredData = true;
                                            }

                                            if (!bCalculateIBikePauseFromGpsTrack)
                                            {
                                                if (foundLapStart)
                                                {
                                                    TimeSpan lapDuration = lapEndTime - lapStartTime;
                                                    activity.Laps.Add(lapStartTime, lapDuration);
                                                    WriteToLogfile("Add lap: " + lapStartTime + ", duration: " + lapDuration, true);
                                                }
                                            }
                                        }

                                        if (bCalculateIBikePauseFromGpsTrack && bImporting)
                                        {
                                            elementsFromIBike = elementsFromIBikeList.ToArray();
                                            WriteToLogfile("Merge iBike data and GPS track", true);

                                            //merge iBike data and GPS data
                                            int countGpsElements = elementsFromGpsTrack.GetLength(0);
                                            int countIBikeElements = elementsFromIBike.GetLength(0);

                                            //ensure that we have 0 power when cadence is 0
                                            WriteToLogfile("Ensure that we have 0 power when cadence is 0", true);
                                            for (int indexIBikeElements = 0; indexIBikeElements < countIBikeElements; indexIBikeElements++)
                                            {
                                                if (elementsFromIBike[indexIBikeElements].cadence == 0)
                                                {
                                                    elementsFromIBike[indexIBikeElements].power = 0;
                                                }
                                            }

                                            WriteToLogfile("Find cadence pauses with zero speed", true);
                                            bool bPedalPause = false;
                                            int indexPedalPauseStart = -1;
                                            int indexPedalPauseEnd = -1;
                                            int indexZeroSpeed = -1;
                                            for (int indexIBikeElements = 0; indexIBikeElements < countIBikeElements; indexIBikeElements++)
                                            {
                                                if (bPedalPause)
                                                {
                                                    if (elementsFromIBike[indexIBikeElements].cadence == 0)
                                                    {
                                                        //still in pedal pause phase
                                                        indexPedalPauseEnd = indexIBikeElements;
                                                        if (elementsFromIBike[indexIBikeElements].speed <= 0.2)
                                                        {
                                                            indexZeroSpeed = indexIBikeElements;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //found end of pedal pause
                                                        if (indexZeroSpeed != -1)
                                                        {
                                                            WriteToLogfile("Found cadence pause with zero speed", true);

                                                            if (indexZeroSpeed >= indexPedalPauseStart)
                                                            {
                                                                if (indexZeroSpeed == indexPedalPauseEnd)
                                                                {
                                                                    elementsFromIBike[indexIBikeElements].needSync = true;
                                                                    elementsFromIBike[indexIBikeElements].insertZeroCadenceBefore = true;
                                                                }
                                                                else if (indexZeroSpeed < indexPedalPauseEnd)
                                                                {
                                                                    elementsFromIBike[indexZeroSpeed + 1].needSync = true;
                                                                }
                                                            }
                                                        }

                                                        bPedalPause = false;
                                                        indexPedalPauseStart = -1;
                                                        indexPedalPauseEnd = -1;
                                                        indexZeroSpeed = -1;
                                                    }
                                                }
                                                else
                                                {
                                                    if (elementsFromIBike[indexIBikeElements].cadence == 0)
                                                    {
                                                        bPedalPause = true;
                                                        indexPedalPauseStart = indexIBikeElements;
                                                        indexPedalPauseEnd = indexIBikeElements;
                                                        if (elementsFromIBike[indexIBikeElements].speed <= 0.2)
                                                        {
                                                            indexZeroSpeed = indexIBikeElements;
                                                        }
                                                    }
                                                }
                                            }

                                            elementsFromIBike[0].needSync = true;

                                            iBikeCurrentDuration = TimeSpan.Zero;
                                            TimeSpan lastSetElapsedTime = TimeSpan.Zero;

                                            WriteToLogfile("Adjust elapsed time in iBike data", true);
                                            for (int indexIBikeElements = 0; indexIBikeElements < countIBikeElements; indexIBikeElements++)
                                            {
                                                //WriteToLogfile(string.Format("Elapsed time: {0}", iBikeCurrentDuration.ToString()), true);

                                                if (elementsFromIBike[indexIBikeElements].needSync)
                                                {
                                                    WriteToLogfile("Sync iBike with GPS track after cadence pause with zero speed", true);
                                                    for (int indexGpsElements = countGpsElements - 1; indexGpsElements >= 0; indexGpsElements--)
                                                    {
                                                        if (elementsFromGpsTrack[indexGpsElements].distance == elementsFromIBike[indexIBikeElements].distance)
                                                        {
                                                            iBikeCurrentDuration = elementsFromGpsTrack[indexGpsElements].elapsedtime;
                                                            WriteToLogfile(string.Format("Found GPS point with same distance, adjust elapsed time: {0}", iBikeCurrentDuration.ToString()), true);
                                                            break;
                                                        }
                                                        else if (elementsFromGpsTrack[indexGpsElements].distance < elementsFromIBike[indexIBikeElements].distance)
                                                        {
                                                            float deltaDistance = elementsFromIBike[indexIBikeElements].distance - elementsFromGpsTrack[indexGpsElements].distance;
                                                            float deltaTime = 0;
                                                            if (elementsFromGpsTrack[indexGpsElements].speed != 0)
                                                            {
                                                                deltaTime = deltaDistance / (elementsFromGpsTrack[indexGpsElements].speed * 1000 / 3600);
                                                            }
                                                            iBikeCurrentDuration = elementsFromGpsTrack[indexGpsElements].elapsedtime + new TimeSpan(0, 0, (int)deltaTime);
                                                            WriteToLogfile(string.Format("Found GPS point with similar distance, adjust elapsed time: {0}", iBikeCurrentDuration.ToString()), true);
                                                            break;
                                                        }
                                                    }
                                                    if (iBikeCurrentDuration <= lastSetElapsedTime)
                                                    {
                                                        //problem! this cannot be, just proceed
                                                        iBikeCurrentDuration = lastSetElapsedTime + new TimeSpan(0, 0, iBikeTimeInterval);
                                                        WriteToLogfile(string.Format("Problem: Adjustment to GPS track did not work, reset to time interval: {0}", iBikeCurrentDuration.ToString()), true);
                                                        elementsFromIBike[indexIBikeElements].needSync = false;
                                                        elementsFromIBike[indexIBikeElements].insertZeroCadenceBefore = false;
                                                    }
                                                }

                                                elementsFromIBike[indexIBikeElements].elapsedtime = iBikeCurrentDuration;
                                                lastSetElapsedTime = iBikeCurrentDuration;
                                                iBikeCurrentDuration += new TimeSpan(0, 0, iBikeTimeInterval);
                                            }

                                            WriteToLogfile("Now import the iBike data", true);
                                            IActivity activity = activityTrack;


                                            bool foundiBikeLapStart = false;
                                            DateTime lapStartTimeiBike = DateTime.Now;

                                            for (int indexIBikeElements = 0; indexIBikeElements < countIBikeElements; indexIBikeElements++)
                                            {
                                                DateTime startTime = iBikeTimeStampStart + elementsFromIBike[indexIBikeElements].elapsedtime;

                                                if (elementsFromIBike[indexIBikeElements].insertZeroCadenceBefore)
                                                {
                                                    WriteToLogfile("Insert point with zero cadence / power", true);
                                                    DateTime startTimeBefore = startTime - new TimeSpan(0, 0, 1);

                                                    if (activity.PowerWattsTrack == null)
                                                    {
                                                        activity.PowerWattsTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                    }
                                                    activity.PowerWattsTrack.Add(startTimeBefore, 0);

                                                    if (activity.CadencePerMinuteTrack == null)
                                                    {
                                                        activity.CadencePerMinuteTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                    }
                                                    activity.CadencePerMinuteTrack.Add(startTimeBefore, 0);
                                                }

                                                float distance = elementsFromIBike[indexIBikeElements].distance;
                                                float heartRateAverage = elementsFromIBike[indexIBikeElements].heartrate;
                                                float power = elementsFromIBike[indexIBikeElements].power;
                                                float cadence = elementsFromIBike[indexIBikeElements].cadence;
                                                float elevation = elementsFromIBike[indexIBikeElements].elevation;

                                                if (elementsFromIBike[indexIBikeElements].newLap)
                                                {
                                                    if (foundiBikeLapStart)
                                                    {
                                                        TimeSpan lapDuration=startTime-lapStartTimeiBike;
                                                        activity.Laps.Add(lapStartTimeiBike, lapDuration);
                                                        WriteToLogfile("Add lap: " + lapStartTimeiBike + ", duration: " + lapDuration, true);
                                                    }
                                                    foundiBikeLapStart = true;
                                                    lapStartTimeiBike = startTime;
                                                }


                                                if (!float.IsNaN(distance))
                                                {
                                                    if (activity.DistanceMetersTrack == null)
                                                    {
                                                        activity.DistanceMetersTrack = new DistanceDataTrack();
                                                        activity.DistanceMetersTrack.AllowMultipleAtSameTime = true;
                                                    }
                                                    activity.DistanceMetersTrack.Add(startTime, (float)distance);

                                                    activity.TotalDistanceMetersEntered = (float)distance;
                                                }

                                                if (!float.IsNaN(heartRateAverage))
                                                {
                                                    if (activity.HeartRatePerMinuteTrack == null)
                                                    {
                                                        activity.HeartRatePerMinuteTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                    }
                                                    activity.HeartRatePerMinuteTrack.Add(startTime, heartRateAverage);
                                                }

                                                if (!float.IsNaN(power))
                                                {
                                                    if (activity.PowerWattsTrack == null)
                                                    {
                                                        activity.PowerWattsTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                    }
                                                    activity.PowerWattsTrack.Add(startTime, power);
                                                }

                                                if (!float.IsNaN(cadence))
                                                {
                                                    if (activity.CadencePerMinuteTrack == null)
                                                    {
                                                        activity.CadencePerMinuteTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                    }
                                                    activity.CadencePerMinuteTrack.Add(startTime, cadence);
                                                }

                                                if (!float.IsNaN(elevation))
                                                {
                                                    if (activity.ElevationMetersTrack == null)
                                                    {
                                                        activity.ElevationMetersTrack = new ZoneFiveSoftware.Common.Data.NumericTimeDataSeries();
                                                    }
                                                    activity.ElevationMetersTrack.Add(startTime, elevation);
                                                }                                                

                                                activity.TotalTimeEntered = elementsFromIBike[indexIBikeElements].elapsedtime;

                                                if (foundiBikeLapStart && (indexIBikeElements+1 == countIBikeElements))
                                                {
                                                    TimeSpan lapDuration = startTime - lapStartTimeiBike;
                                                    activity.Laps.Add(lapStartTimeiBike, lapDuration);
                                                    WriteToLogfile("Add lap: " + lapStartTimeiBike + ", duration: " + lapDuration, true);
                                                }

                                            }
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        WriteToLogfile("Could not read line: " + e.Message, true);
                                        monitor.ErrorText = Properties.Resources.ID_ReadLine + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                        resultCode = false;
                                    }
                                }
                                else
                                {
                                    WriteToLogfile("Header line does not contain valid delimeters: , ; tab", true);
                                    monitor.ErrorText = Properties.Resources.ID_InvalidDelimeter + Properties.Resources.ID_ReferToLogFile + LogfileName();
                                    resultCode = false;
                                }
                            }
                            else
                            {
                                WriteToLogfile("Empty header line", true);
                                monitor.ErrorText = Properties.Resources.ID_NoHeaderLine;
                                resultCode = false;
                            }
                        }
                        catch (Exception e)
                        {
                            WriteToLogfile("Could not read header line: " + e.Message, true);
                            monitor.ErrorText = Properties.Resources.ID_NoHeaderLine
                                + Properties.Resources.ID_ReferToLogFile + LogfileName();
                            resultCode = false;
                        }
                    }
                    catch (Exception e)
                    {
                        WriteToLogfile("Could not import file: " + e.Message, true);
                        monitor.ErrorText = Properties.Resources.ID_FileOpen
                            + Properties.Resources.ID_ReferToLogFile + LogfileName();
                        resultCode = false;
                    }
                    finally
                    {
                        if (file != null)
                        {
                            file.Close();
                        }
                    }


                    if (monitor.Cancelled || (resultCode==false))
                    {
                        break;
                    }
                }
            }

            if (importedLines == 0)
            {
                WriteToLogfile("No activity imported", true);
                resultCode = false;
            }
            else
            {
                if (resultCode && (!monitor.Cancelled))
                {
                    WriteToLogfile("Imported lines: " + importedLines, true);
                }
            }

            monitor.StatusText = Properties.Resources.ID_Done;

            return resultCode;
        }

        #endregion
    }
}
