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

        public ActionResult CurrentIteration(int? id, string projects,string q) {
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
            AppendCurrentIteration(iteration, ids, CreateCardFilter(q));
            return View(iteration);
        }

        Func<CardView, bool> CreateCardFilter(string q) {
            if(string.IsNullOrEmpty(q))
                return _ => true;
            return x => x.Badges.Any(badge => badge.Name.Equals(q, StringComparison.InvariantCultureIgnoreCase));
        }
           

        void AppendCurrentIteration(IterationView iteration, List<int> projects, Func<CardView, bool> cardFilter) {
            var tasks = new Task<List<CardView>>[projects.Count];
            for(var i = 0; i != projects.Count; ++i) {
                tasks[i] = CardsForCurrentIteration(projects[i]);
            }
            iteration.AddRange(tasks.SelectMany(x => x.Result).Where(cardFilter), x => x.CurrentState);
        }

        Task<List<CardView>> CardsForCurrentIteration(int project) {
            var currentIteration = tracker.CurrentIteration(project);
            var projectsLookup = tracker.Projects().ContinueWith(task => CreateLookup(task.Result));
            var membersLookup = tracker.ProjectMembers(project).ContinueWith(task => CreateLookup(task.Result));
            return Task.Factory.ContinueWhenAll(new Task[]{ currentIteration, projectsLookup, membersLookup }, _ => {
                var cards = new CardViewFactory(projectsLookup.Result, membersLookup.Result, CreateBadgeBuilder());
                cards.TaskCompleteUrl = Url.Content("~/Content/FamFamFam/tick.png");
                cards.TaskPendingUrl = Url.Content("~/Content/FamFamFam/bullet_orange.png");
                return new List<CardView>(currentIteration.Result.Select(cards.MakeCardForStory));
            });
        }

        IBuilder<string, CardBadge> CreateBadgeBuilder() {
            var badges = new DefaultBadgeBuilder();
            badges.LoadConfigurationSection("Badges");
            return badges;
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
