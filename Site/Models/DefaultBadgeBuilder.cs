using System;
using System.Collections.Generic;
using System.Configuration;

namespace CardWall.Models
{
    class DefaultBadgeBuilder : IBuilder<string, CardBadge>
    {
        readonly Dictionary<string, CardBadge> badges = new Dictionary<string, CardBadge>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "type:chore", new CardBadge { Name = "Chore", Url = "~/Content/PivotalTracker/chore.png" } },
            { "type:feature", new CardBadge { Name = "Feature", Url = "~/Content/PivotalTracker/feature.png" } },
            { "type:release", new CardBadge { Name = "Release", Url = "~/Content/PivotalTracker/release.png" } }                
        };

        public void LoadConfigurationSection(string name) {
            var badgeConfiguration = (BadgeConfiguration)ConfigurationManager.GetSection(name);
            foreach(var item in badgeConfiguration.Badges)
                badges.Add(item.Key, item);
        }

        bool IBuilder<string, CardBadge>.TryBuild(string input, out CardBadge output) {
            return badges.TryGetValue(input, out output);
        }
    }
}