@echo off
fsi BurndownData.fsx --project=173053 --team="team south" %* >> Site\TeamSouthBurndown.txt
fsi BurndownData.fsx --project=173053 --team="team north" %* >> Site\TeamNorthBurndown.txt
fsi BurndownData.fsx --project=173053 --team="incredible" %* >> Site\TeamIncrediblesBurndown.txt
