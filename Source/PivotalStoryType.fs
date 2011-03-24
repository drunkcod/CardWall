namespace CardWall

type PivotalStoryType = 
    | Unknown = 0
    | Bug = 1
    | Chore = 2
    | Feature = 3
    | Release = 4

type PivotalStoryState =
    | Unknown = 0
    | Unscheduled = 1
    | Unstarted = 2
    | Started = 3
    | Finished = 4
    | Delivered = 5
    | Accepted = 6
