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
            var projectsLookup = tracker.Projects().ContinueWith(task => CreateLookup(task.Result));
            var membersLookup = tracker.ProjectMembers(id).ContinueWith(task => CreateLookup(task.Result));
            return Task.Factory.ContinueWhenAll(new Task[]{ currentIteration, membersLookup }, _ => {
                var cards = new CardViewFactory(projectsLookup.Result, membersLookup.Result, CreateBadgeLookup());
                return new List<CardView>(currentIteration.Result.Select(cards.MakeCardForStory));
            });
        }

        IKeyValueLookup<string, CardBadge> CreateBadgeLookup() {
            var badges = new Dictionary<string, CardBadge>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "team north", new CardBadge { Name = "Team North", Url = Url.Content("~/Content/FamFamFam/flag_blue.png") } },
                { "team south", new CardBadge { Name = "Team South", Url = Url.Content("~/Content/FamFamFam/flag_red.png") } },
                { "sg 3 pricing", new CardBadge { Name = "Pricing", Url = Url.Content("~/Content/FamFamFam/money.png") } },
                { "sg 1 usability", new CardBadge { Name = "Usability", Url = Url.Content("~/Content/FamFamFam/user_female.png") } },
                { "type:bug", new CardBadge { Name = "Bug", Url = Url.Content("~/Content/PivotalTracker/bug.png") } },
                { "type:chore", new CardBadge { Name = "Chore", Url = Url.Content("~/Content/PivotalTracker/chore.png") } },
                { "type:feature", new CardBadge { Name = "Feature", Url = Url.Content("~/Content/PivotalTracker/feature.png") } },
                { "type:release", new CardBadge { Name = "Release", Url = Url.Content("~/Content/PivotalTracker/release.png") } }                
            };
            return new DictionaryKeyValueLookup<string, CardBadge>(badges);        
        }

        Dictionary<string, PivotalProjectMember> CreateLookup(IEnumerable<PivotalProjectMember> members) {
            var result = new Dictionary<string, PivotalProjectMember>();
            foreach(var item in members)
                result.Add(item.Name, item);
            return result;
        }

        Dictionary<int, PivotalProject> CreateLookup(IEnumerable<PivotalProject> projects) {
            var result = new Dictionary<int, PivotalProject>();
            foreach(var item in projects)
                result.Add(item.Id, item);
            return result;
        }

    }
}
