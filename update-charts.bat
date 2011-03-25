@echo off
fsi StoriesPerType.fsx %* >> StoryTypeBreakdown-CSA3.csv
fsi StoryTypeBreakdownChart.fsx StoryTypeBreakdown-CSA3.csv > StoryTypeBreakdon-CSA3.html
