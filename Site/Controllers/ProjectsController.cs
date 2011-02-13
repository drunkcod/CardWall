using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CardWall.Models;

namespace CardWall.Controllers
{
    public class ProjectsController : Controller
    {
        PivotalTracker tracker = new PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine));

        public ActionResult CurrentIteration(int? id, string projects) {
            var iteration = new IterationView {
                { "unstarted", new LaneView { Name = "Unstarted" } },
                { "started", new LaneView { Name = "Started" } },
                { "finished", new LaneView { Name = "Finished" } },
                { "delivered", new LaneView { Name = "Delivered" } },
                { "accepted", new LaneView { Name = "Done" } }
            };
            
            var ids = new List<int>();
            if(id.HasValue)
                ids.Add(id.Value);
            if(!string.IsNullOrEmpty(projects))
                foreach(var item in projects.Split(' '))
                    ids.Add(int.Parse(item));
            AppendCurrentIteration(iteration, ids);
            return View(iteration);
        }

        void AppendCurrentIteration(IterationView iteration, List<int> id) {
            var tasks = new Task<List<CardView>>[id.Count];
            for(var i = 0; i != id.Count; ++i) {
                tasks[i] = CardsForCurrentIteration(id[i]);
            }
            iteration.AddRange(tasks.SelectMany(x => x.Result), x => x.CurrentState);
        }

        Task<List<CardView>> CardsForCurrentIteration(int id) {
            var currentIteration = tracker.CurrentIteration(id);
            var membersLookup = tracker.ProjectMembers(id).ContinueWith(task => CreatePersonLookup(task.Result));
            return Task.Factory.ContinueWhenAll(new Task[]{ currentIteration, membersLookup }, _ => {
                var result = new List<CardView>();
                var stories = currentIteration.Result;
                var members = membersLookup.Result;
                foreach(var item in stories) {
                    var card = new CardView {
                        Type = item.Type,
                        CurrentState = item.CurrentState,
                        Title = item.Name,
                        Owner = GetOwner(members, item),
                        AvatarUrl = GetAvatarUrl(members, item)
                    };
                    result.Add(card);
                }
                return result;
            });
        }

        Dictionary<string, PivotalProjectMember> CreatePersonLookup(IEnumerable<PivotalProjectMember> members) {
            var result = new Dictionary<string, PivotalProjectMember>();
            foreach(var item in members)
                result.Add(item.Name, item);
            return result;
        }

        string GetAvatarUrl(Dictionary<string, PivotalProjectMember> members, PivotalStory story) {
            if(string.IsNullOrEmpty(story.OwnedBy))
                return "";
            return Gravatar.FromEmail(members[story.OwnedBy].EmailAddress);
        }

        string GetOwner(Dictionary<string, PivotalProjectMember> members, PivotalStory story) {
            if(string.IsNullOrEmpty(story.OwnedBy))
                return "<none>";
            return story.OwnedBy;
        }

    }
}
