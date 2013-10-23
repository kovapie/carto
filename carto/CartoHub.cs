using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using carto.Models;

namespace carto
{
    [HubName("cartoHub")]
    public class CartoHub : Hub
    {
        private readonly CmdbRepository _repository;

        public CartoHub():this(CmdbRepository.Instance) {}

        public CartoHub(CmdbRepository repository)
        {
            _repository = repository;
        }
    }
}