using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Common.Requests;
using FisherTournament.Contracts.Common;
using Mapster;

namespace FisherTournament.Infrastructure.Mapping
{
    public class CommonMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(PagedList<>), typeof(PagedListResponse<>));
        }
    }
}