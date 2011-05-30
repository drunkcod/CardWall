/*
drop table Stories
create table Stories(
	[Project] varchar(max),
	[StoryType] varchar(max),
	[StoryState] varchar(max),
	[RequestedBy] varchar(max),
	[CreatedAt] datetime,
	[AcceptedAt] datetime
)
*/

select RequestedBy, 
	Completed = (select count(*) from Stories x where RequestedBy = Stories.RequestedBy and StoryState = 'Accepted'),
	Total = count(*)
from Stories
group by RequestedBy
order by count(*) desc