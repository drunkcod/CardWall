using System.Collections.Generic;

namespace CardWall.Models
{
    class CardViewFactory 
    {
        readonly IDictionary<string, PivotalProjectMember> members;

        public CardViewFactory(IDictionary<string, PivotalProjectMember> members) {
            this.members = members;
        }

        public CardView MakeCardForStory(PivotalStory story) {
            return new CardView {
                Type = story.Type,
                CurrentState = story.CurrentState,
                Title = story.Name,
                Owner = GetOwner(story),
                AvatarUrl = GetAvatarUrl(story),
                Url = story.Url,
                Labels = story.Labels
            };
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