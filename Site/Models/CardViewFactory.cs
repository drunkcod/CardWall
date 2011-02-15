using System.Collections.Generic;
using System.Web.Mvc;

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

        public CardView MakeCardForStory(PivotalStory story) {
            var card = new CardView {
                Type = story.Type,
                CurrentState = story.CurrentState,
                Title = story.Name,
                Owner = GetOwner(story),
                AvatarUrl = GetAvatarUrl(story),
                Url = story.Url,
                ProjectName = GetProjectName(story)
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

            return card;
        }

        bool TryGetBadge(string label, out CardBadge badge) {
            return badges.TryBuild(label, out badge);
        }

        string GetProjectName(PivotalStory story) {
            return projects[story.ProjectId].Name;
        }

        string GetAvatarUrl(PivotalStory story) {
            if(string.IsNullOrEmpty(story.OwnedBy))
                return "";
            return Gravatar.FromEmail(members[story.OwnedBy].EmailAddress);
        }

        string GetOwner(PivotalStory story) {
            if(string.IsNullOrEmpty(story.OwnedBy))
                return "<none>";
            return story.OwnedBy;
        }
    }
}