/*
drop table Stories
GO
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
GO
create table Labels(
	Id int identity,
	Label varchar(256) not null unique nonclustered)

drop table StoryLabels 
GO
create table StoryLabels (
	StoryId int not null,
	LabelId int not null)

drop view RankedStories
GO
create view RankedStories as (
	select *,
		RowNumber = row_number() over(partition by Id, CurrentState order by SnapshotDate),
		RowNumberDesc = row_number() over(partition by Id, CurrentState order by SnapshotDate desc)
	from Stories)
*/
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

--delete Stories;
select [Type], [Estimate], [CycleTime], Frequency = count(*)
from (
	select [Type], [Estimate], CycleTime = datediff(dd, StartedAt, AcceptedAt)
	from (
		select
			StartedAt = isnull(Start.SnapshotDate, RankedStories.AcceptedAt),
			RankedStories.*
		from RankedStories
		left join RankedStories Start on Start.Id = RankedStories.Id and Start.CurrentState = 'Started' and Start.RowNumber = 1
		where RankedStories.AcceptedAt is not null
		and RankedStories.CurrentState = 'Accepted' 
		and RankedStories.RowNumberDesc = 1
	) CycleTime
) CycleTime
where Estimate is not null
group by [Type], [Estimate], [CycleTime]
order by [Estimate], [CycleTime]

select [Estimate], avg(CycleTime * 1.0)
from (
	select [Type], [Estimate], CycleTime = datediff(dd, StartedAt, AcceptedAt)
	from (
		select
			StartedAt = isnull(Start.SnapshotDate, RankedStories.AcceptedAt),
			RankedStories.*
		from RankedStories
		left join RankedStories Start on Start.Id = RankedStories.Id and Start.CurrentState = 'Started' and Start.RowNumber = 1
		where RankedStories.AcceptedAt is not null
		and RankedStories.CurrentState = 'Accepted' 
		and RankedStories.RowNumberDesc = 1
	) CycleTime
) CycleTime
where [Estimate] is not null
group by [Type], [Estimate]