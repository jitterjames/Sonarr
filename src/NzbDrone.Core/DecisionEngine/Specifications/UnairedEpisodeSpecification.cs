using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UnairedEpisodeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UnairedEpisodeSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (!_configService.RejectUnairedEpisodes)
            {
                return DownloadSpecDecision.Accept();
            }

            var now = DateTime.UtcNow;
            var nextUnairedEpisode = subject.Episodes
                .Where(episode => episode.AirDateUtc.HasValue && episode.AirDateUtc.Value > now)
                .OrderBy(episode => episode.AirDateUtc)
                .FirstOrDefault();

            if (nextUnairedEpisode == null)
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Release contains an episode that has not aired yet. Episode {0} airs at {1:u}",
                nextUnairedEpisode.Id,
                nextUnairedEpisode.AirDateUtc.Value);

            return DownloadSpecDecision.Reject(
                DownloadRejectionReason.BeforeAirDate,
                "Episode has not aired yet (airs {0:u})",
                nextUnairedEpisode.AirDateUtc.Value);
        }
    }
}
