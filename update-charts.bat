@echo off
fsi StoriesPerType.fsx --project=173053 %* >> CSA3.csv
fsi StoriesPerType.fsx --project=11621 %* >> CDS.csv
fsi StoryTypeBreakdownChart.fsx CSA3.csv > CSA3.html
fsi StoryTypeBreakdownChart.fsx CDS.csv > CDS.html 
