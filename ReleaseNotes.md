# Version 3.0.6 #
Released 2011-02-23
  * Fixed [issue 7](https://code.google.com/p/sporttracks-csv-import/issues/detail?id=7): Fixed detection of valid date in "time" values. Use date from "time" column only if datetime or date is not given. The bug was a side effect from fixing [issue 6](https://code.google.com/p/sporttracks-csv-import/issues/detail?id=6).

# Version 3.0.5 #
Released 2011-02-20
  * Fixed [issue 6](https://code.google.com/p/sporttracks-csv-import/issues/detail?id=6): When time column values contain a date, us it as activity date.

# Version 3.0.4 #
Released 2011-02-12

  * Fixed [issue 4](https://code.google.com/p/sporttracks-csv-import/issues/detail?id=4): Support quoted geader tokens, supported tokens "heart rate" and altitude, tolerate longitude and latitude entries with invalid values

# Version 3.0.3 #
Released 2010-10-26

Changes compared to 3.0.2:
  * Fixed [issue 2](https://code.google.com/p/sporttracks-csv-import/issues/detail?id=2): lb following weight value pops up error message
  * Fixed [issue 3](https://code.google.com/p/sporttracks-csv-import/issues/detail?id=3): 0.2lb is added to each lb weight


# Version 3.0.2 Re-Release #
Released 2010-08-17

Changes compared to 3.0.2:
  * I have corrected the download package. The plugin.xml was missing.


# Version 3.0.2 #
Released 2010-08-16

Changes compared to 3.0.1:
  * In case of an import error, write full internal error message to message box



# Version 3.0.1 #

Changes compared to 1.0.x:
  * Compatible to SportTracks 3.0