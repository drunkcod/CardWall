using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CardWall.Models
{
    public class CardView
    {
        readonly List<CardBadge> badges = new List<CardBadge>();
        readonly List<CardTask> tasks = new List<CardTask>();
        readonly List<string> labels = new List<string>();

        public string Title;
        public string Summary;
        public string Type;
        public string Owner;
        public string AvatarUrl;
        public string Url;
        public string CurrentState;
        public IEnumerable<string> Labels { get { return labels; } }
        public IEnumerable<CardBadge> Badges { get { return badges; } }
        public int TasksDone { get { return Tasks.Count(x => x.IsComplete); } }
        public ReadOnlyCollection<CardTask> Tasks { get { return tasks.AsReadOnly(); } }
        public string ProjectName;
        public int? Size;
        public DateTime Started;
		public int Id;

        public bool ShowSummary { get { return string.IsNullOrEmpty(Summary) == false; } }
        public bool HasFooter { get { return string.IsNullOrEmpty(Owner) == false || Labels.Count() > 0; } }

        public void AddBadge(CardBadge item) { badges.Add(item); }
        public void AddTask(CardTask item){ tasks.Add(item); }
        public void AddLabel(string label){ labels.Add(label); }
    }
}