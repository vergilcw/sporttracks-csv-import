﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WbSportTracksCsvImporter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WbSportTracksCsvImporter.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not add activity to SportTracks..
        /// </summary>
        internal static string ID_AddActivityError {
            get {
                return ResourceManager.GetString("ID_AddActivityError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activity type detection.
        /// </summary>
        internal static string ID_CheckingActivityType {
            get {
                return ResourceManager.GetString("ID_CheckingActivityType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Checking date format.
        /// </summary>
        internal static string ID_CheckingDateFormat {
            get {
                return ResourceManager.GetString("ID_CheckingDateFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header detection.
        /// </summary>
        internal static string ID_CheckingHeader {
            get {
                return ResourceManager.GetString("ID_CheckingHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Counting Lines.
        /// </summary>
        internal static string ID_CountingLines {
            get {
                return ResourceManager.GetString("ID_CountingLines", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Done.
        /// </summary>
        internal static string ID_Done {
            get {
                return ResourceManager.GetString("ID_Done", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///Example: .
        /// </summary>
        internal static string ID_Example {
            get {
                return ResourceManager.GetString("ID_Example", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not open the csv file..
        /// </summary>
        internal static string ID_FileOpen {
            get {
                return ResourceManager.GetString("ID_FileOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calculating paused segments from GPS track.
        /// </summary>
        internal static string ID_GettingPauseTimeFromGpsTrack {
            get {
                return ResourceManager.GetString("ID_GettingPauseTimeFromGpsTrack", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Importing activities.
        /// </summary>
        internal static string ID_Importing {
            get {
                return ResourceManager.GetString("ID_Importing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The header line does not contain valid delimeters. Supported delimeters are , ; tab.
        /// </summary>
        internal static string ID_InvalidDelimeter {
            get {
                return ResourceManager.GetString("ID_InvalidDelimeter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No header line found. Please use correct field names like date, time, duration or distance..
        /// </summary>
        internal static string ID_NoHeaderLine {
            get {
                return ResourceManager.GetString("ID_NoHeaderLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not read from file..
        /// </summary>
        internal static string ID_ReadLine {
            get {
                return ResourceManager.GetString("ID_ReadLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///For more information refer to the log file: .
        /// </summary>
        internal static string ID_ReferToLogFile {
            get {
                return ResourceManager.GetString("ID_ReferToLogFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with unknown categories. The category name must be identical
        ///with a unique subcategory name or the hierarchical category name as displayed
        ///in SportTracks
        ///Examples:
        ///Hiking
        ///Running: Forrest.
        /// </summary>
        internal static string ID_UnknownCategory {
            get {
                return ResourceManager.GetString("ID_UnknownCategory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use GPS track of existing activity for calculating timestamps?
        ///{0}, {1} km, {2}.
        /// </summary>
        internal static string ID_UseExistingGpsTrack {
            get {
                return ResourceManager.GetString("ID_UseExistingGpsTrack", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong blood pressure format. The blood pressure has to be formatted as decimal value.
        ///Supported decimal separators are: , or .
        ///Examples: 128  or  119.5.
        /// </summary>
        internal static string ID_WrongBloodPressureFormat {
            get {
                return ResourceManager.GetString("ID_WrongBloodPressureFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong BMI format. The BMI has to be formatted as decimal value.
        ///Supported decimal separators are: , or .
        ///Examples: 17,3  or  18.1.
        /// </summary>
        internal static string ID_WrongBmiFormat {
            get {
                return ResourceManager.GetString("ID_WrongBmiFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong bodyfat format. The bodyfat has to be formatted as decimal value.
        ///Supported decimal separators are: , or .
        ///Examples: 17,3  or  18.1.
        /// </summary>
        internal static string ID_WrongBodyFatFormat {
            get {
                return ResourceManager.GetString("ID_WrongBodyFatFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong cadence format. The cadence has to be given as rpm, formatted as decimal value.
        ///Supported decimal separators are: , or . Example: 80.4 rpm or 85,3.
        /// </summary>
        internal static string ID_WrongCadenceFormat {
            get {
                return ResourceManager.GetString("ID_WrongCadenceFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong calories format. The calories have to be formatted as decimal value followed by a unit.
        ///Supported decimal separators are: , or .
        ///Supported units are: kcal, kjoule.
        ///When no unit is given, the value is handled as kcal.
        ///The value and the unit can be separated by a space.
        ///Examples: 580kcal, 590.3.
        /// </summary>
        internal static string ID_WrongCaloriesFormat {
            get {
                return ResourceManager.GetString("ID_WrongCaloriesFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong climbed format. The climbed value has to be formatted as decimal value followed by a unit.
        ///Supported decimal separators are: , or .
        ///Supported units are: km, k, mil, mile, miles, sm, nm, mi, m, meters, meter, yard, yards, yd, foot, feet, ft.
        ///When no unit is given, the value is handled as m.
        ///The value and the unit can be separated by a space.
        ///Examples: 5k, 17 miles, 42.195 km, 10000m, 10,8km, 21.
        /// </summary>
        internal static string ID_WrongClimbedFormat {
            get {
                return ResourceManager.GetString("ID_WrongClimbedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong date format. The date must be formated as configured in Windows..
        /// </summary>
        internal static string ID_WrongDateFormat {
            get {
                return ResourceManager.GetString("ID_WrongDateFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong datetime / timestamp format. The datetime / timestamp must be formated as configured in Windows..
        /// </summary>
        internal static string ID_WrongDateTimeFormat {
            get {
                return ResourceManager.GetString("ID_WrongDateTimeFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong distance format. The distance has to be formatted as decimal value followed by a unit.
        ///Supported decimal separators are: , or .
        ///Supported units are: km, k, mil, mile, miles, sm, nm, mi, m, meters, meter, yard, yards, yd, foot, feet, ft.
        ///When no unit is given, the value is handled as km.
        ///The value and the unit can be separated by a space.
        ///Examples: 5k, 17 miles, 42.195 km, 10000m, 10,8km, 21.
        /// </summary>
        internal static string ID_WrongDistanceFormat {
            get {
                return ResourceManager.GetString("ID_WrongDistanceFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong duration format. The duration has to be formatted as H:mm:ss.f
        ///Examples:
        ///1:50:00 -&gt; one hour and fifty minutes
        ///50:00 -&gt; fifty minutes
        ///42 -&gt; 42 minutes
        ///42:18.11 -&gt; 42 minutes and 18.11 seconds
        ///10.5 -&gt; 10.5 seconds.
        /// </summary>
        internal static string ID_WrongDurationFormat {
            get {
                return ResourceManager.GetString("ID_WrongDurationFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong elevation format. The elevation has to be formatted as decimal value followed by a unit.
        ///Supported decimal separators are: , or .
        ///Supported units are: km, k, mil, mile, miles, sm, nm, mi, m, meters, meter, yard, yards, yd, foot, feet, ft.
        ///When no unit is given, the value is handled as m.
        ///The value and the unit can be separated by a space.
        ///Examples: 118m, 0,823km, 217.
        /// </summary>
        internal static string ID_WrongElevationFormat {
            get {
                return ResourceManager.GetString("ID_WrongElevationFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with different field count than defined by header line..
        /// </summary>
        internal static string ID_WrongFieldCount {
            get {
                return ResourceManager.GetString("ID_WrongFieldCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The header line contains a not supported field name: .
        /// </summary>
        internal static string ID_WrongFieldName {
            get {
                return ResourceManager.GetString("ID_WrongFieldName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong heart rate format. The heart rate has to be formatted as decimal value.
        ///Supported decimal separators are: , or .
        ///Examples: 48  or  50.1.
        /// </summary>
        internal static string ID_WrongHeartRateFormat {
            get {
                return ResourceManager.GetString("ID_WrongHeartRateFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong hrm / heartrate format. The hrm  has to be formatted as decimal value.
        ///Supported decimal separators are: , or ..
        /// </summary>
        internal static string ID_WrongHrmFormat {
            get {
                return ResourceManager.GetString("ID_WrongHrmFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong intensity format. The intensity has to be given as a whole number between 1 and 10..
        /// </summary>
        internal static string ID_WrongIntensityFormat {
            get {
                return ResourceManager.GetString("ID_WrongIntensityFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong latidute / longitude format. The latitude / longitude has to be formatted as decimal value.
        ///Supported decimal separators are: , or ..
        /// </summary>
        internal static string ID_WrongLatLongFormat {
            get {
                return ResourceManager.GetString("ID_WrongLatLongFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong mood format. The mood has to be given as a whole number between 1 and 3..
        /// </summary>
        internal static string ID_WrongMoodFormat {
            get {
                return ResourceManager.GetString("ID_WrongMoodFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong power format. The power has to be given as W, formatted as decimal value.
        ///Supported decimal separators are: , or . Example: 238.4 W or 128,3.
        /// </summary>
        internal static string ID_WrongPowerFormat {
            get {
                return ResourceManager.GetString("ID_WrongPowerFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong sleep format. The sleep value has to be formatted as H:mm:ss.f
        ///Examples:
        ///1:50:00 -&gt; one hour and fifty minutes
        ///50:00 -&gt; fifty minutes
        ///42 -&gt; 42 minutes
        ///42:18.11 -&gt; 42 minutes and 18.11 seconds
        ///10.5 -&gt; 10.5 seconds..
        /// </summary>
        internal static string ID_WrongSleepFormat {
            get {
                return ResourceManager.GetString("ID_WrongSleepFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong time format. The time must be formated as configured in Windows..
        /// </summary>
        internal static string ID_WrongTimeFormat {
            get {
                return ResourceManager.GetString("ID_WrongTimeFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Found line(s) with wrong weight format. The weight has to be formatted as decimal value followed by a unit.
        ///Supported decimal separators are: , or .
        ///Supported units are: kg, st, lb.
        ///When no unit is given, the value is handled as kg.
        ///The value and the unit can be separated by a space.
        ///Examples: 58kg, 59.3kg.
        /// </summary>
        internal static string ID_WrongWeightFormat {
            get {
                return ResourceManager.GetString("ID_WrongWeightFormat", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap Image_24_FileCSV {
            get {
                object obj = ResourceManager.GetObject("Image_24_FileCSV", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}