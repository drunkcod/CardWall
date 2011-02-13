using System;
using System.Collections.Generic;

namespace CardWall.Models
{
    public class LaneView 
    {
        public string Name;
        public List<CardView> Cards = new List<CardView>();

        public void Add(CardView card){ Cards.Add(card); }
    }

    public class IterationView : IEnumerable<LaneView>
    {
        readonly Dictionary<string, LaneView> laneLookup = new Dictionary<string,LaneView>();
        readonly List<LaneView> lanes = new List<LaneView>();

        public IEnumerable<LaneView> Lanes { get { return lanes; } }

        public void Add(string key, LaneView value) { 
            lanes.Add(value); 
            laneLookup.Add(key, value);
        }

        public void AddRange(IEnumerable<CardView> cards, Func<CardView, string> laneSelector) {
            foreach(var item in cards) 
                this[laneSelector(item)].Add(item);

        }

        public LaneView this[string laneKey] { get { return laneLookup[laneKey]; } }

        IEnumerator<LaneView> IEnumerable<LaneView>.GetEnumerator() {
            return lanes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return lanes.GetEnumerator();
        }
    }
}