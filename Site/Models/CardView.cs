using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

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
        public ReadOnlyCollection<CardTask> Tasks { get { return tasks.AsReadOnly(); } }
        public string ProjectName;
        public int? Size;
        public DateTime Started;

        public void AddBadge(CardBadge item) { badges.Add(item); }
        public void AddTask(CardTask item){ tasks.Add(item); }
        public void AddLabel(string label){ labels.Add(label); }
    }
}