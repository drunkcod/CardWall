using System;
using System.Collections.Generic;

namespace CardWall.Models
{
    class CardViewFactory 
    {
        readonly IDictionary<int, PivotalProject> projects;
        readonly IDictionary<string, PivotalProjectMember> members;
        readonly IBuilder<string, CardBadge> badges;

        public CardViewFactory(IDictionary<int, PivotalProject> projects, IDictionary<string, PivotalProjectMember> members, IBuilder<string, CardBadge> badges) {
            this.projects = projects;
            this.members = members;
            this.badges = badges;
        }

        public string TaskCompleteUrl;
        public string TaskPendingUrl;

        public CardView MakeCardForStory(PivotalStory story) {
            var card = new CardView {
                Type = story.Type.ToString().ToLower(),
                CurrentState = TranslateState(story.CurrentState),
                Size = story.Estimate,
                Title = story.Name,
                Owner = GetOwner(story),
                AvatarUrl = GetAvatarUrl(story),
                Url = story.Url,
                ProjectName = GetProjectName(story),
				Id = story.Id
            };

            CardBadge badge;
            foreach(var item in story.Labels) {
                if(TryGetBadge(item, out badge))
                    card.AddBadge(badge);
                else
                    card.AddLabel(item);
            }
            
            if(TryGetBadge("type:" + story.Type, out badge))
                card.AddBadge(badge);

            foreach(var item in story.Tasks)
                card.AddTask(new CardTask
                {
                    Name = item.Description,
                    IsComplete = item.IsComplete,
                    ImageUrl = item.IsComplete ? TaskCompleteUrl : TaskPendingUrl
                });

            return card;
        }

        string TranslateState(PivotalStoryState state) {
            switch(state) {
                case PivotalStoryState.Unscheduled: return "unscheduled"; 
                case PivotalStoryState.Unstarted: return "unstarted";
                case PivotalStoryState.Started: return "started";
                case PivotalStoryState.Finished: return "finished";
                case PivotalStoryState.Delivered: return "delivered";
                case PivotalStoryState.Accepted: return "accepted";
            }
            throw new KeyNotFoundException();
        }

        bool TryGetBadge(string label, out CardBadge badge) {
            return badges.TryBuild(label, out badge);
        }

        string GetProjectName(PivotalStory story) {
            return projects[story.ProjectId].Name;
        }

        string GetAvatarUrl(PivotalStory story)
		{
			if (string.IsNullOrEmpty(story.OwnedBy))
				return "";
			try
			{
				return Gravatar.FromEmail(members[story.OwnedBy].EmailAddress);
			}
			catch (Exception)
			{
				return "http://www.southparkgames.org/games/images/hulk_hogan.gif";
			}
		}

        string GetOwner(PivotalStory story) {
            if(string.IsNullOrEmpty(story.OwnedBy))
                return "";
            return story.OwnedBy;
        }
    }
}