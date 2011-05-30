/*
drop table Stories
create table Stories(
	[Project] varchar(max),
	[StoryId] int,
	[StoryType] varchar(max),
	[StoryState] varchar(max),
	[Name] varchar(max),
	[RequestedBy] varchar(max),
	[OwnedBy] varchar(max),
	[CreatedAt] datetime,
	[AcceptedAt] datetime
)
*/
select top 10 * from Stories
where StoryState = 'Started'
/*
select RequestedBy, 
	Completed = (select count(*) from Stories x where RequestedBy = Stories.RequestedBy and StoryState = 'Accepted'),
	Total = count(*)
from Stories
group by RequestedBy
order by count(*) desc

select OwnedBy, 
	Completed = (select count(*) from Stories x where OwnedBy = Stories.OwnedBy and StoryState = 'Accepted'),
	Total = count(*)
from Stories
group by OwnedBy
order by count(*) desc
*/