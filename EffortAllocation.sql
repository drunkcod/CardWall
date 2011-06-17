/*
drop table Stories
create table Stories(
	[SnapshotDate] datetime,
	[Project] int,
	[Id] int,
	[Type] varchar(max),
	[CurrentState] varchar(max),
	[Estimate] int,
	[Name] varchar(max),
	[RequestedBy] varchar(max),
	[OwnedBy] varchar(max),
	[CreatedAt] datetime,
	[AcceptedAt] datetime)

drop table Labels
create table Labels(
	Id int identity,
	Label varchar(256) not null unique nonclustered)

drop table StoryLabels 
create table StoryLabels (
	StoryId int not null,
	LabelId int not null)

drop view RankedStories
create view RankedStories as (
	select *,
		RowNumber = row_number() over(partition by Id, CurrentState order by SnapshotDate),
		RowNumberDesc = row_number() over(partition by Id, CurrentState order by SnapshotDate desc)
	from Stories)
*/
select top 10 * from Stories
where CurrentState = 'Started'
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