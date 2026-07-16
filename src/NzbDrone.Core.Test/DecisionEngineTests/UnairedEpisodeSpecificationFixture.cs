using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class UnairedEpisodeSpecificationFixture : CoreTest<UnairedEpisodeSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
            {
                Episodes = new List<Episode>
                {
                    new Episode { Id = 1, AirDateUtc = DateTime.UtcNow.AddDays(-1) }
                }
            };

            GivenSetting(true);
        }

        private void GivenSetting(bool enabled)
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(config => config.RejectUnairedEpisodes)
                .Returns(enabled);
        }

        [Test]
        public void should_accept_when_setting_is_disabled()
        {
            GivenSetting(false);
            _remoteEpisode.Episodes[0].AirDateUtc = DateTime.UtcNow.AddDays(1);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_an_episode_that_has_aired()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_an_episode_without_a_known_air_date()
        {
            _remoteEpisode.Episodes[0].AirDateUtc = null;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_an_episode_that_has_not_aired()
        {
            _remoteEpisode.Episodes[0].AirDateUtc = DateTime.UtcNow.AddDays(1);

            var result = Subject.IsSatisfiedBy(_remoteEpisode, null);

            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.BeforeAirDate);
        }

        [Test]
        public void should_reject_a_multi_episode_release_when_any_episode_has_not_aired()
        {
            _remoteEpisode.Episodes.Add(new Episode { Id = 2, AirDateUtc = DateTime.UtcNow.AddDays(1) });

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
