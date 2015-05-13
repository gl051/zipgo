# zipgo

Zip periodically files located in a folder, zip names are based on the current timestamp.

Usage:

zipgo [-i <Input>] [-o <Output>] [-p <SearchPattern>] interval

-Interval : frequency of checking for input files, value is in minutes
-o Output : output folder, default value is current directory
-p SearchPattern : the search string to match against the file name. You can use wildcard * and ?, default value is *.*

Examples:

zipgo 5 -o G:\\tmp
zipgo 5 -i G:\\input -o G:\\ouput -p *.log
zipgo 10 -p WP*.log