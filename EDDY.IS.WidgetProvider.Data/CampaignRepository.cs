using EDDY.IS.WidgetProvider.Core;
using MatchingEngine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EDDY.IS.WidgetProvider.Data
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly ILogger<CampaignRepository> logger;
        private readonly IConfiguration configuration;

        public CampaignRepository(ILogger<CampaignRepository> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task<bool> HasExitPop(Guid trackId)
        {
            try
            {
                var marchingEngineClient = new MatchingServiceClient(MatchingServiceClient.EndpointConfiguration.CustomBinding_IMatchingService, configuration.GetSection("MatchingServiceURL").Value.ToString());
                var campaign = await marchingEngineClient.GetCampaignDetailByTrackIDAsync(trackId);
                return campaign.AllowExitPops;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}