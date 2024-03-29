The "WB CSV Importer" allows importing CSV files with SportTracks.
The importer is intended for migrating running journals or training data from other
applications into SportTracks.

http://code.google.com/p/sporttracks-csv-import/

Content
1) CSV File Format
2) Supported fields
3) Installation
4) Usage




1) CSV File Format

The following data fields are supported:
Date, time, datetime, timestamp, rowdate, entrydate,
distance, totaldistance, duration, tottime, elapsed time,
elevation, latitude, longitude,
hrm, heartrate, heartaverage, pulse, puls, climbed,
cadence, rpm, power, watt, averagewatts, calories, totalcalories, kjoule, intensity,
category, activity, subcategory, location, name, description,
comment, comments, details, equipment,
weight, bodyweight, bmi, bodyfat,
restingheartrate, resthr, maxheartrate, heartmax,
systolic, diastolic,
sleep, diary, mood



The csv file should look like this:

date;time;distance;duration;hrm;category;location;name;comment;weight;bmi;bodyfat;restingheartrate;maxheartrate;systolic;diastolic
06/24/2008;18:00;10;0:50:42;140;Running:Forrest;Seattle;Main;Interval workout;59.4;21.8;16.9;44;185;120;80
06/25/2008;18:45;8.5;0:40:38;150;Running:Forrest;Seattle;Main;;59.1;21.7;16.6;46;185;125;82
06/26/2008;18:30;25;2:15:30;130;Running:Forrest;Seattle;;Long run;59.6;21.9;17.0;44;185;118;78



The first line defines the contained fields. The fields can have any order.
The line lists the field names separated by a separator character.
Supported separators are: <Tab> ; ,
The same separator must be used for all lines.


Units can be given behind the values:
date;time;distance
06/24/2008;18:00;10 k
06/25/2008;18:45;8.5 k


Units can be defined in the header as well:
date;time;distance (km)
06/24/2008;18:00;10
06/25/2008;18:45;8.5


Each following line defines an activity or a trackpoint.

When no date is given, multiple lines refer to the same date, or GPS coordinates are found,
the file is handled as one acitivity with multiple trackpoints.
Otherwise each line is handled as one activity.


The number of entries for all data lines must match the number of defined fields in the first line.
When a value contains a separator, the value has to be quoted with double quotation marks,
like "running; long distance".
When a value is not known but the corresponding field was defined in the first line,
the value can be omitted but the following separator must be given nevertheless (see the sample above).




2) Supported fields:

date, entrydate: date of workout, formatted as configured in windows for the current locale.
So 04/24/2008 would be typical for the US, the same date would be 24.04.2008 for Germany.
The plugin tries parsing the date by using the current windows configuration.
If this does not work, it tries the US and German date format as a fallback.


time: start time of workout in local time, formatted as configured in windows for current locale.
The plugin tries parsing the time by using the current windows configuration.
If this does not work, it tries the US and German time format as a fallback.

datetime, timestamp or rowdate: date and time, formatted as configured in windows for the current locale.
2008-06-07T07:30:42 would be typical for the US.
The plugin tries parsing the time stamp by using the current windows configuration.
If this does not work, it tries the US and German date format as a fallback.

distance: distance of workout, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: km, k, meters, meter, mil, mile, miles, sm, nm, mi, m, yard, yards, yd, foot, feet, ft.
When no unit is given, the value is handled as km.
The value and the unit can be separated by a space.
Examples: 5k, 17 miles, 42.195 km, 10000m, 10,8km, 21

totaldistance: distance of workout, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: km, k, meters, meter, mil, mile, miles, sm, nm, mi, m, yard, yards, yd, foot, feet, ft.
When no unit is given, the value is handled as m.
The value and the unit can be separated by a space.
Examples: 5k, 17 miles, 42.195 km, 10000m, 10,8km, 21000

          
elevation: elevation of track point, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: km, k, meters, meter, mil, mile, miles, sm, nm, mi, m, yard, yards, yd, foot, feet, ft.
When no unit is given, the value is handled as m.
The value and the unit can be separated by a space.
Examples: 127m, 1.2 km, 138,6
          
latitude: latitude of track point, formatted as decimal value.
Supported decimal separators: , or .
Examples: 33.6745 

longitude: longitude of track point, formatted as decimal value.
Supported decimal separators: , or .
Examples: -118.0031 

duration, elapsed time, tottime: duration or elapsed time of workout / trackpoint,
formatted as H:mm:ss.f or decimal number followed by a unit.
Supported units: seconds, sec, sek, s, hours, hour, hrs, h, minutes, minute, min, m
Examples:
1:50:00 -> one hour and fifty minutes
50:00 -> fifty minutes
42:18.11 -> 42 minutes and 18.11 seconds
10.5 -> 10.5 seconds
42 -> 42 minutes
5 min -> 5 minutes
128 s -> 128 seconds



climbed: total ascended meters of workout, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Will be ignored when track data is given.
Supported units: km, k, meters, meter, mil, mile, miles, sm, nm, mi, m, yard, yards, yd, foot, feet, ft.
When no unit is given, the value is handled as m.
The value and the unit can be separated by a space.
Examples: 1,2k, 1.3 miles, 1230m, 217
          

hrm, heartrate, pulse, puls or heartaverage: average heart rate during the workout or track data
given as beats per minute, formatted as decimal value followed by a unit.
Supported units: bpm.
When no unit is given, the value is handled as bpm.
Examples:  80.4

heartmax or maxheartrate: max heart rate during the workout
given as beats per minute, formatted as decimal value followed by a unit.
Supported units: bpm.
When no unit is given, the value is handled as bpm.
Examples:  180

power, watt or averagewatts: bicycle power, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: w, watt.
When no unit is given, the value is handled as watt.
The value and the unit can be separated by a space.
Examples: 250 Watt or 300W or 226,7 

cadence or rpm: bicycle cadence, formatted as decimal value followed by a unit
Supported decimal separators: , or .
Supported units: rpm.
When no unit is given, the value is handled as rpm.
Examples: 250 Watt or 300W or 226,7 

calories, totalcalories or output: total burned kcalories, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: kjoule, kcal.
When no unit is given, the value is handled as kcal.
The value and the unit can be separated by a space.
Examples: 380 kJoule or 1238 kcal or 817.4 

kjoule: total burned kJoule, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: kjoule, kcal.
When no unit is given, the value is handled as kjoule.
The value and the unit can be separated by a space.
Examples: 380 or 1238 kcal or 817.4 kjoule

category, activity:  category name of the workout. The category name must be identical
with a unique subcategory name or the hierarchical category name as displayed
in SportTracks
Examples:
Hiking
Running: Forrest

subcategory, type:  sub category name of the workout. The sub category name must be identical
with a unique subcategory name or the hierarchical category name as displayed
in SportTracks. If both category and subcategory are given, the plugin will combine the
two fields (category "Run" and subcategory "Forrest" will be combined to "Run:Forrest").
Examples:
Hiking
Running: Forrest

equipment: name(s) of used equipment. The plugin will look if the name of existing equipment
can be found in the data entry.

location: location of the workout

name or description: name of the workout

comment, comments, details: comment for the workout

intensity: value between 1 and 10, giving the intensity of the activity

diary: journal entry for the athlete diary

mood, feeling: value between 0 and 3, giving the athlete mood of the day
0 -> not given, 1 -> poor, 2 -> normal, 3 -> excellent

bodyweight, weight: athlete weight of that day, formatted as decimal value followed by a unit.
Supported decimal separators: , or .
Supported units: kg, st, lb.
When no unit is given, the value is handled as kg.
The value and the unit can be separated by a space.
Examples: 58,4  60.3kg,  60,3kg 

bmi: athlete BMI of that day, formatted as decimal value.
Supported decimal separators: , or .
Examples: 20,4  or  21.2

bodyfat: athlete body fat percentage of that day, formatted as decimal value.
Supported decimal separators: , or .
Examples: 19,3  or  18.2

restingheartrate, resthr: athlete resting heart rate of that day, formatted as decimal value,
formatted as decimal value followed by a unit.
Supported units: bpm.
When no unit is given, the value is handled as bpm.
Supported decimal separators: , or .
Examples: 48 or  46.5

athleteheartmax, athletemaxheartrate: athlete maximum heart rate of that day, formatted as decimal value,
formatted as decimal value followed by a unit.
Supported units: bpm.
When no unit is given, the value is handled as bpm.
Supported decimal separators: , or .
Examples: 178 or  185.5

systolic: athlete systolic blood pressure of that day, formatted as decimal value.
Supported decimal separators: , or .
Examples: 120 or  120.5

diastolic: athlete diastolic blood pressure of that day, formatted as decimal value.
Supported decimal separators: , or .
Examples: 80 or  80.5

sleep: hours of sleep,
formatted as H:mm:ss.f or decimal number followed by a unit.
Supported units: seconds, sec, sek, s, hours, hrs, h, minutes, min, m
Examples:
7:50:00 -> seven hours and fifty minutes
50:00 -> fifty minutes
42:18.11 -> 42 minutes and 18.11 seconds
7 -> 7 hours
5 h -> 5 hours




3) Installation

3.1) First time installation of the WB CSV Importer Plugin:
- Click on the download link
- Download the st3plugin file to your harddisk
- Close SportTracks.
- Doubleclick the st3plugin file, SportTracks will install the plugin
- Launch SportTracks.
- The "WB CSV Importer" will be listed as one of the installed plugins (Settings -> Plugins).

3.2) Updating the WB CSV Importer Plugin:
- Launch SportTracks.
- Click on "Other Tasks\Settings" in the left pane, then click on "Plugins"
- SportTracks will check for updates and show that an update is available
- Select the "WB CSV Importer" plugin
- Then click on "Install Update" and follow the instructions
- If SportTracks reports that the plugin has to be uninstalled manually, just proceed with "3.1) First time installation". The clean installation will work and the "WB CSV Importer" plugin will remove the old file on first start.



4) Usage
From SportTracks click on import and choose the CSV file to be imported.

Excel Hints
Important: The plugin does not support line breaks within one data line.
So when you edit your data in Excel and there are e.g. comment fields with line breaks,
Excel would export a csv file containing additional line breaks.
Here is a trick for removing all obsolete line breaks with an Excel macro:
Record an dummy simple Excel macro. Then edit the macro and replace the sub content with something like this:

Sub Makro1()
Range("a1:ag1592").Replace _
What:=Chr(13), Replacement:=", "
Range("a1:ag1592").Replace _
What:=Chr(10), Replacement:=""
End Sub

This macro would replace all line breaks within the range a1:ag1592 with a comma.
Close the macro editor and run the macro.


iBike Hints

A solitude import of an iBike csv file works. You don't need to delete the header lines in advance.
The time delta per data line is detected from the header lines.

If you want to import both data from Garmin Edge and iBike, please import at first the
GPS data. In a second step import the iBike CSV file. 

The plugin gets the duration of the iBike file and the GPS track. The time difference is the pause time
that needs to be inserted into the iBike file (the iBike file does not contain information about pauses).
Then the plugin finds the threshold speed from the GPS track so that the resulting pause time matches
the "time to be inserted". For the pause intervals (where speed is below threshold in the GPS data),
the plugin inserts 0 for power and cadence. In that way the data profile of the iBike file is not changed.

